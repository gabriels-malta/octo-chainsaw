using System;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;

namespace TechCase.FundTransfer.Core.Domain
{
    public class TransferRequest : Trackable, IEquatable<TransferRequest>
    {
        public TransferRequest() { }
        public TransferRequest(string originAcc, string destinationAcc, double value)
        {
            OriginAcc = originAcc;
            DestinationAcc = destinationAcc;
            Value = value;
            Status = TransferRequestStatus.InQueue;
            TimestampOnCreate();
        }

        public Guid Id { get; set; }
        public string OriginAcc { get; set; }
        public string DestinationAcc { get; set; }
        public double Value { get; set; }
        public string Status { get; set; }
        public string Comments { get; set; }

        public bool Equals(TransferRequest other) => OriginAcc == other.OriginAcc && DestinationAcc == other.DestinationAcc && Value == other.Value;

        public TransferRequest UpdateStatus(string status)
        {
            Status = status;
            TimestampOnUpdate();
            return this;
        }

        public TransferRequest UpdateComments(string comments)
        {
            Comments = comments;
            TimestampOnUpdate();
            return this;
        }
    }
}
