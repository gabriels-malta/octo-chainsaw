using TechCase.FundTransfer.Core.Domain.EnumAndKeys;

namespace TechCase.Services.Api.FundTransfer.Models
{
    public class CheckTransactionStatusResponse : ServiceApiResponse
    {
        public string Status { get; set; }

        public bool HasError() => TransferRequestStatus.Error == Status;
    }
}
