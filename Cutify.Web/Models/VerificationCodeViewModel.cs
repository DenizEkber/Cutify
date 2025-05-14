using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cutify.Web.Models
{
    public class VerificationCodeViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [NotMapped]
        [StringLength(4, MinimumLength = 4)]
        public string Code { get; set; }
    }
}
