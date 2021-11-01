using System.ComponentModel.DataAnnotations;

namespace TechCase.Services.Api.FundTransfer.Models
{
    public class FundTransferRequest
    {
        [Required(ErrorMessage = "Origin account is required")]
        [StringLength(20, ErrorMessage = "Origin account cannot be longer then 15")]
        [NotEqualTo(nameof(AccountDestination), ErrorMessage = "Cannot be the same as the destination account")]
        public string AccountOrigin { get; set; }

        [Required(ErrorMessage = "Destination account is required")]
        [StringLength(20, ErrorMessage = "Destination account cannot be longer then 15")]
        [NotEqualTo(nameof(AccountOrigin), ErrorMessage = "Cannot be the same as the origin account")]
        public string AccountDestination { get; set; }

        [Required]
        [GreaterThen(0, ErrorMessage = "Value should be greater then zero")]
        public double Value { get; set; }
    }
}
