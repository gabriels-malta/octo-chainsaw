using RabbitMQ.Client.Events;
using Serilog;
using System;
using System.Threading;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;
using TechCase.FundTransfer.Core.Interfaces;
using TechCase.FundTransfer.Infrastructure.Queue;

namespace TechCase.Services.Worker.FundTransferStarter
{
    internal class Consumer : ConsumerBase
    {
        private readonly ITransferRepository _transferRepository;
        private readonly IPublisher _publisher;
        private readonly ILogger _logger;

        public override string QueueName => QueueTopics.FundTransferStarted;

        public Consumer(ITransferRepository transferRepository, IPublisher publisher, ManualResetEvent resetEvent, ILogger logger)
            : base(resetEvent)
        {
            _transferRepository = transferRepository;
            _publisher = publisher;
            _logger = logger;
        }

        public override void EventReceivedHandler(object model, BasicDeliverEventArgs deliveryEventArgs)
        {
            Event eventReceived = deliveryEventArgs.Body.ToArray().To<Event>();
            _logger.Information("Event received {@EventReceived}", eventReceived);

            var transferRequest = eventReceived.GetEntity<TransferRequest>();
            transferRequest.UpdateStatus(TransferRequestStatus.Processing);
             _transferRepository.Update(transferRequest);

            _logger.Information("Transaction has been updated. {@TransferRequest}", transferRequest);

            var discoveryAccountEvent = Event.UseToSeedNew(transferRequest, QueueTopics.AccountDiscovery, eventReceived);
            _publisher.Publish(discoveryAccountEvent);
            _logger.Information("An event was sent to {Subject}. {@Event}", discoveryAccountEvent.Subject, discoveryAccountEvent);
        }
    }
}
