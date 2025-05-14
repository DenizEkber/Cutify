using Cutify.Domain.Entities;

namespace Cutify.Web.Models;

public class DashboardViewModel
{
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int TodayReservations { get; set; }
    public IEnumerable<Reservation> UpcomingReservations { get; set; } = new List<Reservation>();
    public IEnumerable<Reservation> RecentReservations { get; set; } = new List<Reservation>();
} 