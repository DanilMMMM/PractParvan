using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;

namespace VetClinic.Controllers;

[Authorize]
public class PetsController : Controller
{
    private readonly AppDbContext _context;

    public PetsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Pets
    public async Task<IActionResult> Index(string? search, int? typeId)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isClient = User.IsInRole("Клиент");
        var isAdmin = User.IsInRole("Администратор");
        var isVet = User.IsInRole("Ветеринар");

        var query = _context.Pets
            .Include(p => p.PetType)
            .Include(p => p.Owner)
            .Include(p => p.Visits)
            .AsQueryable();

        // Клиент видит только своих питомцев
        if (isClient)
        {
            query = query.Where(p => p.OwnerId == currentUserId);
        }

        // Фильтрация по виду животного
        if (typeId.HasValue && typeId.Value > 0)
        {
            query = query.Where(p => p.PetTypeId == typeId.Value);
        }

        // Поиск по кличке, породе, владельцу
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s) ||
                                     (p.Breed != null && p.Breed.ToLower().Contains(s)) ||
                                     (p.Owner != null && p.Owner.FullName.ToLower().Contains(s)));
        }

        ViewBag.PetTypes = new SelectList(await _context.PetTypes.ToListAsync(), "Id", "Name", typeId);
        ViewBag.Search = search;
        ViewBag.TypeId = typeId;
        ViewBag.IsClient = isClient;

        var pets = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(pets);
    }

    // GET: Pets/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isClient = User.IsInRole("Клиент");

        var pet = await _context.Pets
            .Include(p => p.PetType)
            .Include(p => p.Owner)
            .Include(p => p.Visits)
                .ThenInclude(v => v.Veterinarian)
            .Include(p => p.Visits)
                .ThenInclude(v => v.Service)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet == null) return NotFound();

        // Защита: клиент не может смотреть чужого питомца
        if (isClient && pet.OwnerId != currentUserId)
        {
            return Forbid();
        }

        return View(pet);
    }

    // GET: Pets/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.PetTypeId = new SelectList(await _context.PetTypes.ToListAsync(), "Id", "Name");
        
        if (User.IsInRole("Администратор") || User.IsInRole("Ветеринар"))
        {
            var clients = await _context.Users
                .Where(u => u.Role != null && u.Role.Name == "Клиент")
                .ToListAsync();
            ViewBag.OwnerId = new SelectList(clients, "Id", "FullName");
        }

        return View(new Pet { BirthDate = DateTime.Today.AddYears(-1) });
    }

    // POST: Pets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Pet pet)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isClient = User.IsInRole("Клиент");

        if (isClient)
        {
            pet.OwnerId = currentUserId;
        }

        if (pet.BirthDate.HasValue && pet.BirthDate.Value > DateTime.Today)
        {
            ModelState.AddModelError("BirthDate", "Дата рождения не может быть в будущем");
        }

        if (ModelState.IsValid)
        {
            pet.CreatedAt = DateTime.Now;
            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Питомец «{pet.Name}» успешно добавлен!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.PetTypeId = new SelectList(await _context.PetTypes.ToListAsync(), "Id", "Name", pet.PetTypeId);
        if (!isClient)
        {
            var clients = await _context.Users
                .Where(u => u.Role != null && u.Role.Name == "Клиент")
                .ToListAsync();
            ViewBag.OwnerId = new SelectList(clients, "Id", "FullName", pet.OwnerId);
        }

        return View(pet);
    }

    // GET: Pets/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null) return NotFound();

        // Клиент может редактировать только своего
        if (User.IsInRole("Клиент") && pet.OwnerId != currentUserId)
        {
            return Forbid();
        }

        ViewBag.PetTypeId = new SelectList(await _context.PetTypes.ToListAsync(), "Id", "Name", pet.PetTypeId);
        if (User.IsInRole("Администратор"))
        {
            var clients = await _context.Users
                .Where(u => u.Role != null && u.Role.Name == "Клиент")
                .ToListAsync();
            ViewBag.OwnerId = new SelectList(clients, "Id", "FullName", pet.OwnerId);
        }

        return View(pet);
    }

    // POST: Pets/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Pet pet)
    {
        if (id != pet.Id) return NotFound();

        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var existingPet = await _context.Pets.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (existingPet == null) return NotFound();

        if (User.IsInRole("Клиент") && existingPet.OwnerId != currentUserId)
        {
            return Forbid();
        }

        if (User.IsInRole("Клиент"))
        {
            pet.OwnerId = currentUserId; // не даем клиенту сменить владельца
        }

        if (pet.BirthDate.HasValue && pet.BirthDate.Value > DateTime.Today)
        {
            ModelState.AddModelError("BirthDate", "Дата рождения не может быть в будущем");
        }

        if (ModelState.IsValid)
        {
            _context.Update(pet);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Данные питомца «{pet.Name}» успешно обновлены!";
            return RedirectToAction(nameof(Details), new { id = pet.Id });
        }

        ViewBag.PetTypeId = new SelectList(await _context.PetTypes.ToListAsync(), "Id", "Name", pet.PetTypeId);
        if (User.IsInRole("Администратор"))
        {
            var clients = await _context.Users
                .Where(u => u.Role != null && u.Role.Name == "Клиент")
                .ToListAsync();
            ViewBag.OwnerId = new SelectList(clients, "Id", "FullName", pet.OwnerId);
        }

        return View(pet);
    }

    // GET: Pets/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pet = await _context.Pets
            .Include(p => p.PetType)
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet == null) return NotFound();

        if (User.IsInRole("Клиент") && pet.OwnerId != currentUserId)
        {
            return Forbid();
        }

        return View(pet);
    }

    // POST: Pets/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null) return NotFound();

        if (User.IsInRole("Клиент") && pet.OwnerId != currentUserId)
        {
            return Forbid();
        }

        var petName = pet.Name;
        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Питомец «{petName}» удалён из базы.";
        return RedirectToAction(nameof(Index));
    }
}
