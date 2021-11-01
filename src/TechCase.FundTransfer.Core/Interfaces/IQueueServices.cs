using TechCase.FundTransfer.Core.Domain;

namespace TechCase.FundTransfer.Core.Interfaces
{
    public interface IPublisher
    {
        public void Publish(Event eventMessage);
    }
}
