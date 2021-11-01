using System;
using System.Text.Json;
using TechCase.FundTransfer.Core.Domain;
using Xunit;

namespace TechCase.FundTransfer.Core.Test
{
    public record TestModel(Guid Id, string Value);
    public class EventTest
    {
        [Fact(DisplayName = "The event should keep the TransactionId from previous Event")]
        public void UseToSeedNewBasedOnPreviousEvent()
        {
            TestModel model = new(Guid.Parse("5dfad4fc-9163-4470-ad29-56b58ba8c2c3"), "test-model");

            var previousEvent = Event
                .UseToSeedNew(model, nameof(UseToSeedNewBasedOnPreviousEvent))
                .WithTransferRequestId(model.Id);


            TestModel otherModer = new(Guid.NewGuid(), "other value");
            var newEvent = Event.UseToSeedNew(otherModer, nameof(UseToSeedNewBasedOnPreviousEvent), previousEvent);

            Assert.Equal(model.Id.ToString(), newEvent.TransferRequestId);
        }

        [Fact(DisplayName ="Should be able to deserialize into a new Event")]
        public void DeserializeFromStringToEvent()
        {
            TestModel model = new (Guid.Parse("5dfad4fc-9163-4470-ad29-56b58ba8c2c3"), "test-model");
            var testModelEvent = Event.UseToSeedNew(model, nameof(DeserializeFromStringToEvent)).WithTransferRequestId(model.Id);
            string rawEvent = JsonSerializer.Serialize(testModelEvent);

            var deserializedEvent = JsonSerializer.Deserialize<Event>(rawEvent);
            Assert.Equal(testModelEvent.TransferRequestId, deserializedEvent.TransferRequestId);
        }

        [Fact(DisplayName ="Should read an Entity from the given Event")]
        public void GetEntityFromGivenEvent()
        {
            TestModel model = new (Guid.Parse("5dfad4fc-9163-4470-ad29-56b58ba8c2c3"), "test-model");
            var testModelEvent = Event.UseToSeedNew(model, nameof(DeserializeFromStringToEvent)).WithTransferRequestId(model.Id);

            var retrievedEntity = testModelEvent.GetEntity<TestModel>();
            Assert.Equal(retrievedEntity.Value, model.Value);
        }
    }
}
