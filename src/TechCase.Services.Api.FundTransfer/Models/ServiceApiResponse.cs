using System.Text.Json.Serialization;

namespace TechCase.Services.Api.FundTransfer.Models
{
    public abstract class ServiceApiResponse
    {
        public ServiceApiResponse()
        {
            Message = null;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string Message { get; set; }
    }
}
