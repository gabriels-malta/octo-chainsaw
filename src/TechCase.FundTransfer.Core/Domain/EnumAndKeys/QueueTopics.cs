namespace TechCase.FundTransfer.Core.Domain.EnumAndKeys
{
    public class QueueTopics
    {
        private QueueTopics() { }

        public const string AccountUpdate = "queue-account-update";
        public const string AccountDiscovery = "queue-account-discovery";

        public const string FundTransferStarted = "queue-fund-transfer-started";
        public const string FundTransferTriggered = "queue-fund-transfer-triggered";
        public const string FundTransferFinished = "queue-fund-transfer-finished";
        public const string FundTransferFailed = "queue-fund-transfer-failed";
    }
}