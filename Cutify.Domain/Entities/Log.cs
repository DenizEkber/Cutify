using System.ComponentModel.DataAnnotations;

namespace Cutify.Domain.Entities;

public class Log
{
    public int Id { get; set; }

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(50)]
    public string Level { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Message { get; set; } = null!;

    [StringLength(4000)]
    public string? Exception { get; set; }

    [StringLength(255)]
    public string? Source { get; set; }

    [StringLength(255)]
    public string? UserName { get; set; }

    [StringLength(255)]
    public string? RequestPath { get; set; }

    [StringLength(50)]
    public string? RequestMethod { get; set; }
} 