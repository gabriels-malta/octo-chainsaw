using System;

namespace TechCase.FundTransfer.Core.Domain
{
    public abstract class Trackable
    {
        public DateTime CreatedOn { get; protected set; }
        public DateTime? UpdatedOn { get; protected set; }

        protected void TimestampOnCreate() => CreatedOn = DateTime.UtcNow;
        protected void TimestampOnUpdate() => UpdatedOn = DateTime.UtcNow;
    }
}
