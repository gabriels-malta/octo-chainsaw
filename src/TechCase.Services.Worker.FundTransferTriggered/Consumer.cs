using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Threading;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;
using TechCase.FundTransfer.Core.Interfaces;
using TechCase.FundTransfer.Infrastructure.Queue;

namespace TechCase.Services.Worker.FundTransferTriggered
{
    internal class Consumer : ConsumerBase
    {
        private readonly IRepository<Account> _accountRepository;
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly IPublisher _publisher;
        private readonly ILogger _logger;

        public override string QueueName => QueueTopics.FundTransferTriggered;

        public Consumer(IRepository<Account> transferRepository, IRepository<Transaction> transactionRepository, IPublisher publisher, ManualResetEvent resetEvent, ILogger logger)
            : base(resetEvent)
        {
            _accountRepository = transferRepository;
            _transactionRepository = transactionRepository;
            _publisher = publisher;
            _logger = logger;
        }

        public override void EventReceivedHandler(object model, BasicDeliverEventArgs deliveryEventArgs)
        {
            Event eventReceived = deliveryEventArgs.Body.ToArray().To<Event>();
            _logger.Information("Event received {@EventReceived}", eventReceived);

            var transferRequest = eventReceived.GetEntity<TransferRequest>();

            Account originAcc = _accountRepository.GetFirstBy(x => x.AccountNumber == transferRequest.OriginAcc);
            Account destinationAcc = _accountRepository.GetFirstBy(x => x.AccountNumber == transferRequest.DestinationAcc);

            if (!originAcc.HaveEnoughMoney(transferRequest.Value))
            {
                _logger.Information("Origin account {AccountNumber} does not have enough money to commit the transfer", destinationAcc.AccountNumber);
                EventError eventError = new($"Destination account {destinationAcc.AccountNumber} does not have enough fund.");
                var transferRequestFailEvent = Event.UseToSeedNew(eventError, QueueTopics.FundTransferFailed, eventReceived);
                _publisher.Publish(transferRequestFailEvent);
                _logger.Information("An event was sent to {Subject}. {@Event}", transferRequestFailEvent.Subject, transferRequestFailEvent);
                return;
            }

            var debitTransaction = CreateTransaction(transferRequest.Id, transferRequest.OriginAcc, transferRequest.Value, TransactionType.Debit);
            var creditTransaction = CreateTransaction(transferRequest.Id,transferRequest.DestinationAcc, transferRequest.Value, TransactionType.Credit);

            var debitTransactionEvent = Event.UseToSeedNew(debitTransaction, QueueTopics.AccountUpdate, eventReceived);
            _publisher.Publish(debitTransactionEvent);
            _logger.Information("An event was sent to {Subject}. {@Event}", debitTransactionEvent.Subject, debitTransactionEvent);

            var creditTransactionEvent = Event.UseToSeedNew(creditTransaction, QueueTopics.AccountUpdate, eventReceived);
            _publisher.Publish(creditTransactionEvent);
            _logger.Information("An event was sent to {Subject}. {@Event}", creditTransactionEvent.Subject, creditTransactionEvent);
        }

        private Transaction CreateTransaction(Guid transferRequestId, string accountNumber, double value, string transactionType)
        {
            Transaction newTransaction = new(transactionType, accountNumber, value, transferRequestId);
            _transactionRepository.Insert(newTransaction);
            _logger.Information("New transaction created. {@NewTransaction}", newTransaction);
            return newTransaction;
        }
    }
}
