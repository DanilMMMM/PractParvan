using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var services = await _context.Services
            .Where(s => s.IsActive)
            .Take(6)
            .ToListAsync();

        var doctors = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.Role != null && u.Role.Name == "Ветеринар")
            .ToListAsync();

        ViewBag.Doctors = doctors;
        ViewBag.TotalPetsCount = await _context.Pets.CountAsync();
        ViewBag.TotalVisitsCount = await _context.Visits.CountAsync();
        ViewBag.TotalClientsCount = await _context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Клиент");

        return View(services);
    }

    public async Task<IActionResult> Services(string? search, decimal? maxPrice)
    {
        var query = _context.Services.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.ToLower().Contains(search.Trim().ToLower()) ||
                                     s.Description.ToLower().Contains(search.Trim().ToLower()));
        }

        if (maxPrice.HasValue && maxPrice.Value > 0)
        {
            query = query.Where(s => s.Price <= maxPrice.Value);
        }

        ViewBag.Search = search;
        ViewBag.MaxPrice = maxPrice;

        var list = await query.ToListAsync();
        var services = list.OrderBy(s => s.Price).ToList();
        return View(services);
    }

    public IActionResult Contacts()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
