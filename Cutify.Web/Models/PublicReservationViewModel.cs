using System.ComponentModel.DataAnnotations;
using Cutify.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cutify.Web.Models;

public class PublicReservationViewModel
{
    [Required(ErrorMessage = "Please select a barber")]
    [Display(Name = "Barber")]
    public int BarberId { get; set; }

    [Required(ErrorMessage = "Please select a service")]
    [Display(Name = "Service")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "Please enter your name")]
    [Display(Name = "Your Name")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your phone number")]
    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    [StringLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a date")]
    [Display(Name = "Date")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Please select a time")]
    [Display(Name = "Time")]
    [DataType(DataType.Time)]
    public TimeSpan Time { get; set; } = DateTime.Now.TimeOfDay;

    // Navigation properties for dropdowns
    public IEnumerable<Barber> Barbers { get; set; } = new List<Barber>();
    public IEnumerable<Service> Services { get; set; } = new List<Service>();



    public IEnumerable<SelectListItem> AvailableTimes { get; set; } = new List<SelectListItem>();

}