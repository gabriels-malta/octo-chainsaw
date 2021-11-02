using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            var transferRequest = eventReceived.GetEntity<TransferRequest>();

            if (eventReceived.ReachedAttemptLimitOf(AttempLimit))
            {
                _logger.Error("Message reached the attempt limit of {AttempLimit}. {@EventReceived}", AttempLimit, eventReceived);
                AccountDiscoveryFailed(eventReceived, "Limit of processing attempts reached");
                return;
            }

            var originAccount = await GetAccountDataFromAcessoService(transferRequest.OriginAcc);
            if (HaveToAbortOrRetry(eventReceived, originAccount))
                return;

            var destinationAccount = await GetAccountDataFromAcessoService(transferRequest.DestinationAcc);
            if (HaveToAbortOrRetry(eventReceived, destinationAccount))
                return;

            if (originAccount is null || destinationAccount is null)
            {
                eventReceived.MarkForRetry();
                _publisher.Publish(eventReceived);
                _logger.Warning("Event sent for retry, attempt: {AttempCount}", eventReceived.AttemptCount);
            }
            else
            {
                if (new double[] { originAccount.balance, destinationAccount.balance }.Contains(-1))
                {
                    _logger.Error("Message reached the attempt limit of {AttempLimit}. {@EventReceived}", AttempLimit, eventReceived);
                    AccountDiscoveryFailed(eventReceived, $"");
                    return;
                }

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

        private async Task<AccountResponse> GetAccountDataFromAcessoService(string accountNumber)
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
                responseMessage.EnsureSuccessStatusCode();
                return JsonSerializer.Deserialize<AccountResponse>(responseContent);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                if (ex is HttpRequestException httpRequestException)
                {
                    _logger.Error(httpRequestException, "Acesso-API was unsuccessful", httpRequestException.StatusCode);
                    if (httpRequestException.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return new AccountResponse(null, -1) with { comments = $"Account {accountNumber} not found" };
                }
                else
                    _logger.Error(ex, "Error when trying to reach Acesso-API");
            }
            finally
            {
                _logger.Information("{ElapsedMilliseconds} milliseconds chatting with Acesso-API", stopwatch.ElapsedMilliseconds);
            }
            return null;
        }

        private bool HaveToAbortOrRetry(Event eventReceived, AccountResponse accountResponse)
        {
            if (accountResponse is null) //retry
            {
                eventReceived.MarkForRetry();
                _publisher.Publish(eventReceived);
                _logger.Warning("Event sent for retry, attempt: {AttempCount}", eventReceived.AttemptCount);
                return true;
            }

            if (accountResponse.balance == -1) //abort
            {
                _logger.Error(accountResponse.comments);
                AccountDiscoveryFailed(eventReceived, accountResponse.comments);
                return true;
            }
            return false;
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
    public record AccountResponse(string accountNumber, double balance, string comments = null);
}
