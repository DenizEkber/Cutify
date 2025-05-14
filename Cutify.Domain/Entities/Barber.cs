using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cutify.Domain.Entities;

public class Barber : User
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();


    [NotMapped]
    public const string Role = "Barber";
} 