using Microsoft.Extensions.Logging;
using System;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;
using TechCase.FundTransfer.Core.Interfaces;
using TechCase.Services.Api.FundTransfer.Interfaces;

namespace TechCase.Services.Api.FundTransfer.Services
{
    internal class FundTransferService : IFundTransferService
    {
        private readonly ITransferRepository _transferRequestRepository;
        private readonly IPublisher _publisher;
        private readonly ILogger<FundTransferService> _logger;

        public FundTransferService(ILogger<FundTransferService> logger, ITransferRepository repository, IPublisher publisher)
        {
            _transferRequestRepository = repository;
            _publisher = publisher;
            _logger = logger;
        }

        public Guid InitializeTransferProcess(string origin, string destination, double value)
        {
            var newTransferRequest = new TransferRequest(origin, destination, value);

            var (isGoing, transactionId) = _transferRequestRepository.HasSameTransferOngoing(newTransferRequest);

            if (isGoing)
            {
                newTransferRequest.Id = transactionId;
                _logger.LogWarning("{@Transaction} is already in place", transactionId);
                return newTransferRequest.Id;
            }

            _logger.LogInformation($"Attempt to initialize the transfer of {value:C} from {origin} to {destination}");

            newTransferRequest = _transferRequestRepository.Insert(newTransferRequest);
            _logger.LogInformation("Transfer successfully request. {@NewTransferRequest}", newTransferRequest);


            var newTransferRequestedEvent = Event
                .UseToSeedNew(newTransferRequest, QueueTopics.FundTransferStarted)
                .WithTransferRequestId(newTransferRequest.Id);

            _publisher.Publish(newTransferRequestedEvent);

            _logger.LogInformation("An event was sent to {Subject} with the {@Event}", newTransferRequestedEvent.Subject, newTransferRequestedEvent);

            return newTransferRequest.Id;
        }

        public (string, string) GetProcessStatus(Guid transferProcessId)
        {
            var transferProcess = _transferRequestRepository.GetFirstBy(x => x.Id == transferProcessId);

            if (transferProcess == null)
            {
                _logger.LogWarning("Transfer process not fund for the ginven {@TransactionId}", transferProcessId);
                return (null, null);
            }

            return (transferProcess.Status, transferProcess.Comments);
        }
    }
}
