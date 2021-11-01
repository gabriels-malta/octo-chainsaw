using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using TechCase.Services.Api.FundTransfer.Interfaces;
using TechCase.Services.Api.FundTransfer.Models;

namespace TechCase.Services.Api.FundTransfer.Controllers
{
    [ApiController]
    [Route("api/fund-transfer")]
    public class FundTransferController : ControllerBase
    {
        private readonly ILogger<FundTransferController> _logger;
        private readonly IFundTransferService _fundTransferService;

        public FundTransferController(ILogger<FundTransferController> logger, IFundTransferService transactionService)
        {
            _logger = logger;
            _fundTransferService = transactionService;
        }

        [HttpGet("{transactionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CheckTransactionStatus(Guid transactionId)
        {
            var transactionRequested = new { TransactionId = transactionId };
            _logger.LogInformation("Check transaction status for {@TransactionRequested}", transactionRequested);

            CheckTransactionStatusResponse response = new();
            (response.Status, response.Message) = _fundTransferService.GetProcessStatus(transactionId);

            if (string.IsNullOrWhiteSpace(response.Status))
            {
                _logger.LogInformation("Not found: {@TransactionRequested}", transactionRequested);
                return NotFound();
            }

            _logger.LogInformation("Transaction status is {@Response}", response);

            return Ok(response);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public IActionResult TransferRequest([FromBody] FundTransferRequest request)
        {
            try
            {
                _logger.LogInformation("Requesting the transfer {@Request}", request);

                FundTransferResponse response = _fundTransferService.InitializeTransferProcess(request.AccountOrigin, request.AccountDestination, request.Value);

                return CreatedAtAction(nameof(CheckTransactionStatus), new { transactionId = response.TransactionId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong with the request. {@Request}", request);

                return new ObjectResult(new FundTransferResponse { Message = "Fail to start the transfer process" })
                {
                    StatusCode = (int)HttpStatusCode.ServiceUnavailable
                };
            }
        }
    }
}
