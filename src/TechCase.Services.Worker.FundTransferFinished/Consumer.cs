using Microsoft.Extensions.Logging;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;
using TechCase.FundTransfer.Core.Interfaces;
using TechCase.FundTransfer.Infrastructure.Queue;

namespace TechCase.Services.Worker.FundTransferFinished
{
    internal class Consumer : ConsumerBase
    {
        private readonly ITransferRepository _transferRepository;
        private readonly IRepository<Transaction> _transactionRepository;
        private readonly ILogger _logger;

        public override string QueueName => QueueTopics.FundTransferFinished;

        public Consumer(ITransferRepository transferRepository, IRepository<Transaction> transactionRepository, ManualResetEvent resetEvent, ILogger logger)
            : base(resetEvent)
        {
            _transferRepository = transferRepository;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        public override void EventReceivedHandler(object model, BasicDeliverEventArgs deliveryEventArgs)
        {
            Event eventReceived = deliveryEventArgs.Body.ToArray().To<Event>();
            _logger.LogInformation("Event received {@EventReceived}", eventReceived);

            var transaction = eventReceived.GetEntity<Transaction>();

            var transactions = _transactionRepository.GetBy(x => x.Id == transaction.Id);
            var transferRequest = _transferRepository.GetFirstBy(x => x.Id == transaction.TransferRequestId);

            if (transactions.AnyOnProgress())
                return; // nothing to do yet

            string transferRequestStatus = TransferRequestStatus.Processing;

            if (transactions.AllFinished())
                transferRequestStatus = TransferRequestStatus.Confirmed;

            if (transactions.AnyError())
                transferRequestStatus = TransferRequestStatus.Error;

            transferRequest.UpdateStatus(transferRequestStatus);
            _transferRepository.Update(transferRequest);
            _logger.LogInformation("TransferRequest {Id} has been updated to \"{Status}\"", transferRequest.Id, transferRequest.Status);
        }
    }
}
