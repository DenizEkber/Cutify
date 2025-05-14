using System.ComponentModel.DataAnnotations;

namespace Cutify.Domain.Entities;

public abstract class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public string WorkplaceAddress { get; set; }

    public string? ProfileImagePath { get; set; }

    public string FullName => $"{FirstName} {LastName}";
} 