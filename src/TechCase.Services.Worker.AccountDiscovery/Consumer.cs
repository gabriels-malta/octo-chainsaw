using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;
using TechCase.FundTransfer.Core.Interfaces;
using TechCase.FundTransfer.Infrastructure.Queue;

namespace TechCase.Services.Worker.AccountDiscovery
{
    internal class Consumer : ConsumerBase
    {
        private readonly IRepository<Account> _accountRepository;
        private readonly IPublisher _publisher;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public override string QueueName => QueueTopics.AccountDiscovery;
        protected override int AttempLimit => 5;

        public Consumer(IRepository<Account> accountRepository, IPublisher publisher, HttpClient httpClient, ManualResetEvent resetEvent, ILogger logger)
            : base(resetEvent)
        {
            _accountRepository = accountRepository;
            _publisher = publisher;
            _httpClient = httpClient;
            _logger = logger;
        }

        public override async void EventReceivedHandler(object model, BasicDeliverEventArgs deliveryEventArgs)
        {
            Event eventReceived = deliveryEventArgs.Body.ToArray().To<Event>();
            _logger.Information("Event received {@EventReceived}", eventReceived);
            Console.WriteLine($"Message received with Correlation: {eventReceived.Correlation}", ConsoleColor.Green);

            var transferRequest = eventReceived.GetEntity<TransferRequest>();

            if (eventReceived.ReachedAttemptLimitOf(AttempLimit))
            {
                _logger.Error("Message reached the attempt limit of {AttempLimit}. {@EventReceived}", AttempLimit, eventReceived);
                AccountDiscoveryFailed(eventReceived, "Limit of processing attempts reached");
                return;
            }

            var originAccount = await ValidateAccountNumner(transferRequest.OriginAcc);
            var destinationAccount = await ValidateAccountNumner(transferRequest.DestinationAcc);

            if (originAccount is null || destinationAccount is null)
            {
                eventReceived.MarkForRetry();
                _publisher.Publish(eventReceived);
                _logger.Warning("Event sent for retry, attempt: {AttempCount}", eventReceived.AttemptCount);
            }
            else
            {
                SaveNewAccount(new Account { AccountNumber = originAccount.accountNumber, Balance = originAccount.balance });
                SaveNewAccount(new Account { AccountNumber = destinationAccount.accountNumber, Balance = destinationAccount.balance });

                var discoveryAccountsEvent = Event.UseToSeedNew(transferRequest, QueueTopics.FundTransferTriggered, basedOn: eventReceived);
                _publisher.Publish(discoveryAccountsEvent);
                _logger.Information("An event was sent to {Subject}. {@Event}", discoveryAccountsEvent.Subject, discoveryAccountsEvent);
            }
        }

        private void AccountDiscoveryFailed(Event eventReceived, string errorMessage)
        {
            EventError eventError = new(errorMessage);
            var transferRequestFailEvent = Event.UseToSeedNew(eventError, QueueTopics.FundTransferFailed, basedOn: eventReceived);
            _publisher.Publish(transferRequestFailEvent);
            _logger.Information("An event was sent to {Subject}. {@Event}", transferRequestFailEvent.Subject, transferRequestFailEvent);
        }

        private async Task<AccountResponse> ValidateAccountNumner(string accountNumber)
        {
            Stopwatch stopwatch = new();
            HttpRequestMessage requestMessage = new(HttpMethod.Get, $"api/Account/{accountNumber}");
            try
            {
                _logger.Information("Checking account number on Acesso-API. {@RequestMessage}", requestMessage);
                stopwatch.Start();
                HttpResponseMessage responseMessage = await _httpClient.SendAsync(requestMessage);
                _logger.Information("Acesso-API returned: {@ResponseMessage}", responseMessage);
                stopwatch.Stop();

                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                if (responseMessage.IsSuccessStatusCode)
                    return JsonSerializer.Deserialize<AccountResponse>(responseContent);
                else
                    _logger.Warning("Acesso-API has returned StatusCode {StatusCode} and Message: \"{ResponseContent}\"", responseMessage.StatusCode, responseContent);

            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                if (ex is HttpRequestException httpRequestException)
                    _logger.Error(httpRequestException, "Acesso-API has returned StatusCode {StatusCode}", httpRequestException.StatusCode);
                else
                    _logger.Error(ex, "Error when trying to reach Acesso-API");
            }
            finally
            {
                _logger.Information("{ElapsedMilliseconds} milliseconds chatting with Acesso-API", stopwatch.ElapsedMilliseconds);
            }

            _logger.Warning("It was not possible validate the {accountNumber} in Acesso's service", accountNumber);
            return null;
        }

        private void SaveNewAccount(Account account)
        {
            var existingOne = _accountRepository.GetFirstBy(x => x.AccountNumber == account.AccountNumber);
            if (existingOne != null)
            {
                existingOne.UpdateBalance(account.Balance);
                _accountRepository.Update(existingOne);
                _logger.Information("Existing account \"{AccountNumber}\" has been updated ", existingOne.AccountNumber);
            }
            else
            {
                account.IsNew();
                _accountRepository.Insert(account);
                _logger.Information("New account created. {@NewAccount}", account);
            }
        }
    }

    public record AccountResponse(string accountNumber, double balance);
}
