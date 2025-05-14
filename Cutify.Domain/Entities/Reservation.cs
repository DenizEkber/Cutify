using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cutify.Domain.Entities;

public class Reservation
{
    public int Id { get; set; }

    [Required]
    public int BarberId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = null!;

    [Required]
    [Phone]
    [StringLength(20)]
    public string CustomerPhone { get; set; } = null!;

    [Required]
    [Column(TypeName = "date")]
    public DateTime Date { get; set; }

    [Required]
    [Column(TypeName = "time")]
    public TimeSpan Time { get; set; }

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(BarberId))]
    public virtual Barber? Barber { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public virtual Service? Service { get; set; }
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled
} 