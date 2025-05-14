using System.ComponentModel.DataAnnotations;
using Cutify.Domain.Entities;

namespace Cutify.Web.Models;

public class BarberViewModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "First Name")]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [Display(Name = "Last Name")]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    [Phone]
    [Display(Name = "Phone Number")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [Display(Name = "Password")]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = null!;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
} 