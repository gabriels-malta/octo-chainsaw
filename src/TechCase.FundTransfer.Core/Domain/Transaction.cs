using System;
using System.Collections.Generic;
using System.Linq;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;

namespace TechCase.FundTransfer.Core.Domain
{
    public class Transaction : Trackable
    {
        public Transaction() { }

        public Transaction(string type, string accountNumber, double value, Guid transferRequestId)
        {
            Type = type;
            AccountNumber = accountNumber ?? throw new ArgumentNullException(nameof(accountNumber));
            Value = value;
            Status = TransactionStatus.Attempt;
            TransferRequestId = transferRequestId;
            TimestampOnCreate();
        }

        public Guid Id { get; set; }
        public string AccountNumber { get; set; }
        public double Value { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }

        public Guid TransferRequestId { get; set; }

        public void UpdateStatus(string status)
        {
            Status = status;
            TimestampOnUpdate();
        }
    }

    public static class TransactionExtensions
    {
        public static bool AnyError(this IEnumerable<Transaction> transactions) => transactions.Any(x => x.Status == TransactionStatus.Refused);
        public static bool AnyOnProgress(this IEnumerable<Transaction> transactions) => transactions.Any(x => x.Status == TransactionStatus.Attempt);
        public static bool AllFinished(this IEnumerable<Transaction> transactions) => transactions.Any(x => x.Status == TransactionStatus.Sucess);
    }
}
