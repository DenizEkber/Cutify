using System.ComponentModel.DataAnnotations;

namespace Cutify.Domain.Entities;

public class Service
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = null!;

    [Required]
    [Range(1, 480)] // Maximum 8 hours in minutes
    public int DurationMinutes { get; set; }

    [Required]
    [Range(0, 10000)]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public int BarberId { get; set; }
    public virtual Barber Barber { get; set; } = null!;
}