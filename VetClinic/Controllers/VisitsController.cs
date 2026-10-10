using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;
using VetClinic.ViewModels;

namespace VetClinic.Controllers;

[Authorize]
public class VisitsController : Controller
{
    private readonly AppDbContext _context;

    public VisitsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Visits
    public async Task<IActionResult> Index(string? status, DateTime? dateFrom, DateTime? dateTo, int? vetId, string? search)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isClient = User.IsInRole("Клиент");
        var isVet = User.IsInRole("Ветеринар");
        var isAdmin = User.IsInRole("Администратор");

        var query = _context.Visits
            .Include(v => v.Pet)
            .Include(v => v.Client)
            .Include(v => v.Veterinarian)
            .Include(v => v.Service)
            .AsQueryable();

        // Разграничение по ролям
        if (isClient)
        {
            query = query.Where(v => v.ClientId == currentUserId);
        }
        else if (isVet && !isAdmin)
        {
            // Ветеринар видит приёмы, назначенные к нему
            query = query.Where(v => v.VeterinarianId == currentUserId);
        }

        // Фильтрация по статусу
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(v => v.Status == status);
        }

        // Фильтрация по датам
        if (dateFrom.HasValue)
        {
            query = query.Where(v => v.VisitDate.Date >= dateFrom.Value.Date);
        }
        if (dateTo.HasValue)
        {
            query = query.Where(v => v.VisitDate.Date <= dateTo.Value.Date);
        }

        // Фильтрация по ветеринару (для админа)
        if (isAdmin && vetId.HasValue && vetId.Value > 0)
        {
            query = query.Where(v => v.VeterinarianId == vetId.Value);
        }

        // Поиск по кличке питомца, диагнозу или ФИО клиента/врача
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(v => (v.Pet != null && v.Pet.Name.ToLower().Contains(s)) ||
                                     (v.Diagnosis != null && v.Diagnosis.ToLower().Contains(s)) ||
                                     (v.Client != null && v.Client.FullName.ToLower().Contains(s)) ||
                                     (v.Veterinarian != null && v.Veterinarian.FullName.ToLower().Contains(s)) ||
                                     (v.Service != null && v.Service.Name.ToLower().Contains(s)));
        }

        var vets = await _context.Users
            .Where(u => u.Role != null && u.Role.Name == "Ветеринар")
            .ToListAsync();

        ViewBag.Vets = new SelectList(vets, "Id", "FullName", vetId);
        ViewBag.Status = status;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
        ViewBag.VetId = vetId;
        ViewBag.Search = search;

        var visits = await query.OrderByDescending(v => v.VisitDate).ToListAsync();
        return View(visits);
    }

    // GET: Visits/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isClient = User.IsInRole("Клиент");

        var visit = await _context.Visits
            .Include(v => v.Pet)
                .ThenInclude(p => p!.PetType)
            .Include(v => v.Client)
            .Include(v => v.Veterinarian)
            .Include(v => v.Service)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (visit == null) return NotFound();

        if (isClient && visit.ClientId != currentUserId)
        {
            return Forbid();
        }

        return View(visit);
    }

    // GET: Visits/Create (Запись на приём)
    public async Task<IActionResult> Create(int? petId, int? serviceId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isClient = User.IsInRole("Клиент");

        var model = new VisitCreateViewModel
        {
            VisitDate = DateTime.Today.AddDays(1).AddHours(10),
            PetId = petId ?? 0,
            ServiceId = serviceId ?? 0
        };

        await PopulateBookingDropdowns(isClient ? currentUserId : null);
        return View(model);
    }

    // POST: Visits/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VisitCreateViewModel model)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isClient = User.IsInRole("Клиент");

        // Валидация даты: нельзя записываться в прошлое
        if (model.VisitDate < DateTime.Now.AddMinutes(-10))
        {
            ModelState.AddModelError("VisitDate", "Дата и время визита не могут быть в прошлом");
        }

        var pet = await _context.Pets.FindAsync(model.PetId);
        if (pet == null)
        {
            ModelState.AddModelError("PetId", "Выбранный питомец не найден");
        }
        else if (isClient && pet.OwnerId != currentUserId)
        {
            return Forbid();
        }

        var service = await _context.Services.FindAsync(model.ServiceId);
        if (service == null)
        {
            ModelState.AddModelError("ServiceId", "Выбранная услуга не найдена");
        }

        var vet = await _context.Users.FindAsync(model.VeterinarianId);
        if (vet == null)
        {
            ModelState.AddModelError("VeterinarianId", "Выбранный врач не найден");
        }

        if (ModelState.IsValid && pet != null && service != null && vet != null)
        {
            var visit = new Visit
            {
                PetId = pet.Id,
                ClientId = isClient ? currentUserId : pet.OwnerId,
                VeterinarianId = vet.Id,
                ServiceId = service.Id,
                VisitDate = model.VisitDate,
                Status = "Запланирован",
                Symptoms = model.Symptoms?.Trim(),
                TotalCost = service.Price,
                CreatedAt = DateTime.Now
            };

            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Запись на приём успешно оформлена!";
            return RedirectToAction(nameof(Details), new { id = visit.Id });
        }

        await PopulateBookingDropdowns(isClient ? currentUserId : null);
        return View(model);
    }

    // GET: Visits/Conduct/5 (Проведение приёма ветеринаром)
    [Authorize(Roles = "Ветеринар,Администратор")]
    public async Task<IActionResult> Conduct(int id)
    {
        var visit = await _context.Visits
            .Include(v => v.Pet)
                .ThenInclude(p => p!.PetType)
            .Include(v => v.Client)
            .Include(v => v.Service)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (visit == null) return NotFound();

        var vm = new VisitConductViewModel
        {
            VisitId = visit.Id,
            PetName = visit.Pet?.Name ?? "Не указано",
            PetSpeciesAndBreed = $"{visit.Pet?.PetType?.Name ?? ""}, {visit.Pet?.Breed ?? ""}",
            OwnerName = visit.Client?.FullName ?? "Не указан",
            ServiceName = visit.Service?.Name ?? "Не указана",
            VisitDate = visit.VisitDate,
            Status = visit.Status == "Запланирован" ? "Завершён" : visit.Status,
            Symptoms = visit.Symptoms,
            Diagnosis = visit.Diagnosis ?? string.Empty,
            Treatment = visit.Treatment ?? string.Empty,
            DoctorNotes = visit.DoctorNotes,
            TotalCost = visit.TotalCost > 0 ? visit.TotalCost : (visit.Service?.Price ?? 0)
        };

        return View(vm);
    }

    // POST: Visits/Conduct/5
    [HttpPost]
    [Authorize(Roles = "Ветеринар,Администратор")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Conduct(VisitConductViewModel model)
    {
        var visit = await _context.Visits
            .Include(v => v.Pet)
            .Include(v => v.Client)
            .Include(v => v.Service)
            .FirstOrDefaultAsync(v => v.Id == model.VisitId);

        if (visit == null) return NotFound();

        if (ModelState.IsValid)
        {
            visit.Status = model.Status;
            visit.Symptoms = model.Symptoms?.Trim();
            visit.Diagnosis = model.Diagnosis.Trim();
            visit.Treatment = model.Treatment.Trim();
            visit.DoctorNotes = model.DoctorNotes?.Trim();
            visit.TotalCost = model.TotalCost;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Приём проведён! Медицинская карта пациента обновлена.";
            return RedirectToAction(nameof(Details), new { id = visit.Id });
        }

        return View(model);
    }

    // POST: Visits/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isClient = User.IsInRole("Клиент");

        var visit = await _context.Visits.FindAsync(id);
        if (visit == null) return NotFound();

        if (isClient && visit.ClientId != currentUserId)
        {
            return Forbid();
        }

        visit.Status = "Отменён";
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Запись на приём была отменена.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Visits/Delete/5 (Только Администратор)
    [HttpPost]
    [Authorize(Roles = "Администратор")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var visit = await _context.Visits.FindAsync(id);
        if (visit == null) return NotFound();

        _context.Visits.Remove(visit);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Запись приёма удалена.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateBookingDropdowns(int? clientId)
    {
        var petsQuery = _context.Pets.Include(p => p.PetType).AsQueryable();
        if (clientId.HasValue)
        {
            petsQuery = petsQuery.Where(p => p.OwnerId == clientId.Value);
        }

        var pets = await petsQuery.Select(p => new
        {
            p.Id,
            DisplayName = $"{p.Name} ({(p.PetType != null ? p.PetType.Name : "Животное")}, {p.Breed})"
        }).ToListAsync();

        var vets = await _context.Users
            .Where(u => u.Role != null && u.Role.Name == "Ветеринар")
            .Select(u => new
            {
                u.Id,
                DisplayName = $"{u.FullName} ({u.Specialization ?? "Ветеринар"})"
            }).ToListAsync();

        var services = await _context.Services
            .Where(s => s.IsActive)
            .Select(s => new
            {
                s.Id,
                DisplayName = $"{s.Name} — {s.Price:N0} руб. ({s.DurationMinutes} мин)"
            }).ToListAsync();

        ViewBag.PetId = new SelectList(pets, "Id", "DisplayName");
        ViewBag.VeterinarianId = new SelectList(vets, "Id", "DisplayName");
        ViewBag.ServiceId = new SelectList(services, "Id", "DisplayName");
    }
}
