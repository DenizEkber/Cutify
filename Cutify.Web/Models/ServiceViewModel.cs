using System.ComponentModel.DataAnnotations;

namespace Cutify.Web.Models
{
    public class ServiceViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Servis adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Servis adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Süre zorunludur.")]
        [Range(1, 480, ErrorMessage = "Süre 1 ile 480 dakika arasında olmalıdır.")]
        public int DurationMinutes { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0, 10000, ErrorMessage = "Fiyat 0 ile 10.000 arasında olmalıdır.")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public int BarberId { get; set; }

        // Optional BarberName if needed for display purposes
        public string? BarberName { get; set; }
    }
}
