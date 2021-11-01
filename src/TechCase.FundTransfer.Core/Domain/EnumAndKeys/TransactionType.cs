namespace TechCase.FundTransfer.Core.Domain.EnumAndKeys
{
    public class TransactionType
    {
        protected TransactionType() { }

        public const string Credit = nameof(Credit);
        public const string Debit = nameof(Debit);
    }
}
