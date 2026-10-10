using VetClinic.Models;

namespace VetClinic.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalClients { get; set; }
    public int TotalVets { get; set; }
    public int TotalPets { get; set; }
    public int TotalVisits { get; set; }
    public int PlannedVisits { get; set; }
    public int CompletedVisits { get; set; }
    public decimal TotalRevenue { get; set; }

    public List<Visit> RecentVisits { get; set; } = new();
    public List<User> RecentUsers { get; set; } = new();
    public List<Pet> RecentPets { get; set; } = new();
}
