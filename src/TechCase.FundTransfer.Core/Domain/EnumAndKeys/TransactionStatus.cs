namespace TechCase.FundTransfer.Core.Domain.EnumAndKeys
{
    public class TransactionStatus
    {
        protected TransactionStatus() { }

        public const string Attempt = nameof(Attempt);
        public const string Sucess = nameof(Sucess);
        public const string Refused = nameof(Refused);
    }
}