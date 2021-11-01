using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TechCase.FundTransfer.Core.Domain;
using TechCase.FundTransfer.Core.Domain.EnumAndKeys;
using TechCase.FundTransfer.Core.Interfaces;

namespace TechCase.FundTransfer.Infrastructure.Database
{
    internal class TransferRepository : Repository<TransferRequest>, ITransferRepository
    {
        public TransferRepository(FundTransferContext context)
            : base(context) { }

        private static readonly string[] OnGoingStatus = new string[] { TransferRequestStatus.InQueue, TransferRequestStatus.Processing };
        public (bool, Guid) HasSameTransferOngoing(TransferRequest transferRequest)
        {
            var transfer = _db.Transfers.OrderBy(x => x.CreatedOn).LastOrDefault(x => x.OriginAcc == transferRequest.OriginAcc
                                                                                                 && x.DestinationAcc == transferRequest.DestinationAcc
                                                                                                 && x.Value == transferRequest.Value
                                                                                                 && OnGoingStatus.Contains(x.Status));

            return transfer != null ? (true, transfer.Id) : (false, Guid.Empty);
        }
    }
}
