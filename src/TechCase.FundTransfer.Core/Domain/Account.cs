using System;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;

namespace TechCase.FundTransfer.Core.Domain
{
    public class Account : Trackable
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public double Balance { get; set; }


        public void IsNew()
        {
            Id = 0;
            TimestampOnCreate();
        }

        public bool HaveEnoughMoney(double intendedValue) => Balance - intendedValue >= 0;

        public void UpdateBalance(double balance)
        {
            Balance = balance;
            TimestampOnUpdate();
        }
        public void UpdateBalance(string transactionType, double value)
        {
            switch (transactionType)
            {
                case TransactionType.Credit:
                    Balance += value;
                    break;
                case TransactionType.Debit:
                    if (HaveEnoughMoney(value))
                        Balance -= value;
                    break;
                default:
                    throw new InvalidOperationException("Invalid transaction type");
            }

            TimestampOnUpdate();
        }
    }
}
