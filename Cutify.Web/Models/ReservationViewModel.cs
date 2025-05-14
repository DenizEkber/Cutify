using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Cutify.Web.Models;
public class ReservationViewModel
{
    public int BarberId { get; set; }
    public string BarberName { get; set; }
    public string CustomerName { get; set; }
    public string CustomerPhone { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time {  get; set; }
    public string ServiceDescription { get; set; }

    public List<int> SelectedServiceIds { get; set; } = new();
    public List<ServiceViewModel> AvailableServices { get; set; } = new();

    public IEnumerable<SelectListItem> AvailableTimes { get; set; } = new List<SelectListItem>();
}
