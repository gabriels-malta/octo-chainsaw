using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Threading;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;
using TechCase.FundTransfer.Core.Interfaces;
using TechCase.FundTransfer.Infrastructure.Queue;

namespace TechCase.Services.Worker.FundTransferFailed
{
    internal class Consumer : ConsumerBase
    {
        private readonly ITransferRepository _transferRepository;
        private readonly ILogger _logger;

        public override string QueueName => QueueTopics.FundTransferFailed;

        public Consumer(ITransferRepository transferRepository, ManualResetEvent resetEvent, ILogger logger)
            : base(resetEvent)
        {
            _transferRepository = transferRepository;
            _logger = logger;
        }

        public override void EventReceivedHandler(object model, BasicDeliverEventArgs deliveryEventArgs)
        {
            Event eventReceived = deliveryEventArgs.Body.ToArray().To<Event>();
            _logger.Information("Event received {@EventReceived}", eventReceived);

            Console.WriteLine($"Message received with Correlation: {eventReceived.Correlation}", ConsoleColor.Green);

            var eventError = eventReceived.GetEntity<EventError>();
            var transferRequest = _transferRepository.GetFirstBy(x => x.Id == Guid.Parse(eventReceived.TransferRequestId));
            transferRequest.UpdateStatus(TransferRequestStatus.Error).UpdateComments(eventError.Message);
            _transferRepository.Update(transferRequest);

            _logger.Information("Transaction has been updated. {@TransferRequest}", transferRequest);
        }
    }
}
