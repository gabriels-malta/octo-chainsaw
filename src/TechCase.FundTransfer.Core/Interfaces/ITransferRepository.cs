using System;
using TechCase.FundTransfer.Core.Domain;

namespace TechCase.FundTransfer.Core.Interfaces
{
    public interface ITransferRepository : IRepository<TransferRequest>
    {
        (bool, Guid) HasSameTransferOngoing(TransferRequest transferRequest);
    }
}
