using System;

namespace TechCase.Services.Api.FundTransfer.Interfaces
{
    public interface IFundTransferService
    {
        Guid InitializeTransferProcess(string origin, string destination, double value);
        (string, string) GetProcessStatus(Guid transferProcessId);
    }
}
