using System.ComponentModel.DataAnnotations.Schema;

namespace Cutify.Domain.Entities;

public class Customer : User
{
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    [NotMapped]
    public const string Role = "Customer";
} 