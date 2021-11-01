namespace TechCase.FundTransfer.Core.Domain.EnumAndKeys
{
    public class TransferRequestStatus
    {
        protected TransferRequestStatus() { }

        public const string InQueue = "In Queue";
        public const string Processing = nameof(Processing);
        public const string Confirmed = nameof(Confirmed);
        public const string Error = nameof(Error);
    }
}