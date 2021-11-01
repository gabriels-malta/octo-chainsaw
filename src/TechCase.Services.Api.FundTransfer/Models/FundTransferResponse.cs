using System;
using System.Text.Json.Serialization;

namespace TechCase.Services.Api.FundTransfer.Models
{
    public class FundTransferResponse : ServiceApiResponse
    {
        public FundTransferResponse() { }
        private FundTransferResponse(Guid guid) => TransactionId = guid.ToString();
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string TransactionId { get; set; }

        public static implicit operator FundTransferResponse(Guid guid) => new(guid);
    }
}
