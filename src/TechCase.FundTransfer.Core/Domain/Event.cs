using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TechCase.FundTransfer.Core.Domain
{
    public record EventError(string Message);
    public class Event
    {
        [JsonConstructor]
        public Event(string subject, Guid correlation, string transferRequestId, DateTime createdOn, string message, int attemptCount)
        {
            Subject = subject;
            Correlation = correlation;
            TransferRequestId = transferRequestId;
            CreatedOn = createdOn;
            Message = message;
            AttemptCount = attemptCount;
        }

        protected Event() { }
        public string Subject { get; private set; }
        public Guid Correlation { get; private set; }
        public string TransferRequestId { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public string Message { get; private set; }
        public int AttemptCount { get; private set; }

        public static Event UseToSeedNew<Tdata>(Tdata data, string subject)
        {
            return new Event
            {
                Subject = subject,
                Correlation = Guid.NewGuid(),
                CreatedOn = DateTime.UtcNow,
                Message = JsonSerializer.Serialize(data)
            };
        }

        public static Event UseToSeedNew<Tdata>(Tdata data, string subject, Event basedOn)
        {
            var newEvent = UseToSeedNew(data, subject);
            newEvent.Correlation = basedOn.Correlation;

            if (!string.IsNullOrWhiteSpace(basedOn.TransferRequestId))
                newEvent.TransferRequestId = basedOn.TransferRequestId;

            return newEvent;
        }

        public bool ReachedAttemptLimitOf(int attempLimit) => AttemptCount + 1 == attempLimit;

        public Event WithTransferRequestId(Guid id)
        {
            TransferRequestId = id.ToString();
            return this;
        }

        public void MarkForRetry() => AttemptCount += 1;
    }
    public static class EventExtensions
    {
        public static Tdata GetEntity<Tdata>(this Event @event) => JsonSerializer.Deserialize<Tdata>(@event.Message);
    }
}
