using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Cutify.Web.Models
{
    public class SetNewPasswordModel
    {
        public string Email { get; set; }

        public ResetPasswordInputModel Input { get; set; } = new ResetPasswordInputModel();
    }

    public class ResetPasswordInputModel
    {
        [Required(ErrorMessage = "Yeni şifrə tələb olunur.")]
        [StringLength(100, ErrorMessage = "{0} ən azı {2} simvol olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Təkrar şifrə tələb olunur.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Şifrə və təsdiqi uyğun gəlmir.")]
        public string ConfirmPassword { get; set; }
    }
}
