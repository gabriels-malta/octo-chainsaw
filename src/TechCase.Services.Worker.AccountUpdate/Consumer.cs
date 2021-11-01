using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;
using TechCase.FundTransfer.Core.Interfaces;
using TechCase.FundTransfer.Infrastructure.Queue;

namespace TechCase.Services.Worker.AccountUpdate
{
    internal class Consumer : ConsumerBase
    {
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<Transaction> _transactionRespository;
        private readonly IPublisher _publisher;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public override string QueueName => QueueTopics.AccountUpdate;
        protected override int AttempLimit => 5;

        public Consumer(IRepository<Account> accountRepository, IRepository<Transaction> transactionRespository, IPublisher publisher, HttpClient httpClient, ManualResetEvent resetEvent, ILogger logger)
            : base(resetEvent)
        {
            _accountRepository = accountRepository;
            _publisher = publisher;
            _transactionRespository = transactionRespository;
            _httpClient = httpClient;
            _logger = logger;
        }

        public override void EventReceivedHandler(object model, BasicDeliverEventArgs deliveryEventArgs)
        {
            Event eventReceived = deliveryEventArgs.Body.ToArray().To<Event>();
            _logger.Information("Event received for the {AttempCount} time {@EventReceived}", eventReceived.AttemptCount, eventReceived);

            var transactionFromEvent = eventReceived.GetEntity<Transaction>();

            var accountFromDb = _accountRepository.GetFirstBy(x => x.AccountNumber == transactionFromEvent.AccountNumber);
            var transactionFromDb = _transactionRespository.GetFirstBy(x => x.Id == transactionFromEvent.Id);

            if (eventReceived.ReachedAttemptLimitOf(AttempLimit))
            {
                _logger.Error("Message reached the attempt limit of {AttempLimit}. {@EventReceived}", AttempLimit, eventReceived);
                TransactionFailed(eventReceived, transactionFromDb, "Limit of processing attempts reached");
                return;
            }

            var (statusCode, errorMessage) = SendTransactionToAcesso(transactionFromEvent);

            if (string.IsNullOrEmpty(errorMessage)) // there is no error
            {
                UpdateTransactionStatus(transactionFromDb);
                UpdateAccountBalance(transactionFromEvent, accountFromDb);

                var fundTransferFinishEvent = Event.UseToSeedNew(transactionFromDb, QueueTopics.FundTransferFinished, basedOn: eventReceived);
                _publisher.Publish(fundTransferFinishEvent);
                _logger.Information("An event was sent to {Subject}. {@Event}", fundTransferFinishEvent.Subject, fundTransferFinishEvent);
            }
            else
            {
                eventReceived.MarkForRetry();
                _publisher.Publish(eventReceived);
                _logger.Warning("Event sent for retry, attempt: {AttempCount}", eventReceived.AttemptCount);
            }
        }

        private void UpdateAccountBalance(Transaction transactionFromEvent, Account accountFromDb)
        {
            accountFromDb.UpdateBalance(transactionType: transactionFromEvent.Type, value: transactionFromEvent.Value);
            _accountRepository.Update(accountFromDb);
            _logger.Information("Account was updated. {@Account}", accountFromDb);
        }

        private void UpdateTransactionStatus(Transaction transactionFromDb)
        {
            transactionFromDb.UpdateStatus(status: TransactionStatus.Sucess);
            _transactionRespository.Update(transactionFromDb);
            _logger.Information("Transaction was updated. {@Transaction}", transactionFromDb);
        }

        private void TransactionFailed(Event eventReceived, Transaction transactionFromDb, string errorMessage)
        {
            transactionFromDb.UpdateStatus(status: TransactionStatus.Refused);
            _transactionRespository.Update(transactionFromDb);

            EventError eventError = new(errorMessage);
            var transferRequestFailEvent = Event.UseToSeedNew(eventError, QueueTopics.FundTransferFailed, eventReceived);
            _publisher.Publish(transferRequestFailEvent);
            _logger.Information("An event was sent to {Subject}. {@Event}", transferRequestFailEvent.Subject, transferRequestFailEvent);
        }

        private (HttpStatusCode, string) SendTransactionToAcesso(Transaction transaction)
        {
            Stopwatch stopwatch = new();
            HttpRequestMessage requestMessage = new(HttpMethod.Post, $"api/Account")
            {
                Content = JsonContent.Create<AccountRequest>(new(transaction.AccountNumber, transaction.Value, transaction.Type))
            };

            try
            {
                _logger.Information("Sending balance update to Acesso-API. {@RequestMessage}", requestMessage);
                stopwatch.Start();
                HttpResponseMessage responseMessage = _httpClient.Send(requestMessage);
                _logger.Information("Acesso-API returned: {@ResponseMessage}", responseMessage);
                stopwatch.Stop();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    var stream = responseMessage.Content.ReadAsStream();
                    StreamReader streamReader = new(stream);
                    var responseContent = streamReader.ReadToEnd();
                    _logger.Information("Update failed with \"{responseContent}\"", responseContent);
                    return (responseMessage.StatusCode, responseContent);
                }
                return (responseMessage.StatusCode, null);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                if (ex is HttpRequestException httpRequestException)
                    _logger.Error(httpRequestException, "Acesso-API has returned StatusCode {StatusCode}", httpRequestException.StatusCode);
                else
                    _logger.Error(ex, "Error when trying to reach Acesso-API");

                return (HttpStatusCode.ServiceUnavailable, "Error when trying to reach Acesso-API");
            }
            finally
            {
                _logger.Information("{ElapsedMilliseconds} milliseconds chatting with Acesso-API", stopwatch.ElapsedMilliseconds);
            }
        }
    }

    public record AccountRequest(string accountNumber, double value, string type);
}
