using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;
using VetClinic.Services;
using VetClinic.ViewModels;

namespace VetClinic.Controllers;

[Authorize(Roles = "Администратор")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public AdminController(AppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    // GET: Admin/Index (Панель управления и статистика)
    public async Task<IActionResult> Index()
    {
        var totalUsers = await _context.Users.CountAsync();
        var totalClients = await _context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Клиент");
        var totalVets = await _context.Users.CountAsync(u => u.Role != null && u.Role.Name == "Ветеринар");
        var totalPets = await _context.Pets.CountAsync();
        var totalVisits = await _context.Visits.CountAsync();
        var plannedVisits = await _context.Visits.CountAsync(v => v.Status == "Запланирован");
        var completedVisits = await _context.Visits.CountAsync(v => v.Status == "Завершён");
        var completedCosts = await _context.Visits
            .Where(v => v.Status == "Завершён")
            .Select(v => v.TotalCost)
            .ToListAsync();
        var totalRevenue = completedCosts.Sum();

        var recentVisits = await _context.Visits
            .Include(v => v.Pet)
            .Include(v => v.Client)
            .Include(v => v.Veterinarian)
            .Include(v => v.Service)
            .OrderByDescending(v => v.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentUsers = await _context.Users
            .Include(u => u.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentPets = await _context.Pets
            .Include(p => p.PetType)
            .Include(p => p.Owner)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync();

        var model = new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalClients = totalClients,
            TotalVets = totalVets,
            TotalPets = totalPets,
            TotalVisits = totalVisits,
            PlannedVisits = plannedVisits,
            CompletedVisits = completedVisits,
            TotalRevenue = totalRevenue,
            RecentVisits = recentVisits,
            RecentUsers = recentUsers,
            RecentPets = recentPets
        };

        return View(model);
    }

    // ==========================================
    // Управление пользователями
    // ==========================================

    // GET: Admin/Users
    public async Task<IActionResult> Users(string? search, int? roleId)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.Pets)
            .AsQueryable();

        if (roleId.HasValue && roleId.Value > 0)
        {
            query = query.Where(u => u.RoleId == roleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.Login.ToLower().Contains(s) ||
                                     u.FullName.ToLower().Contains(s) ||
                                     u.Email.ToLower().Contains(s) ||
                                     (u.Phone != null && u.Phone.ToLower().Contains(s)));
        }

        ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", roleId);
        ViewBag.Search = search;
        ViewBag.RoleId = roleId;

        var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return View(users);
    }

    // GET: Admin/UserEdit/5
    public async Task<IActionResult> UserEdit(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        var vm = new UserEditViewModel
        {
            Id = user.Id,
            Login = user.Login,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            RoleId = user.RoleId,
            Specialization = user.Specialization
        };

        ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", user.RoleId);
        return View(vm);
    }

    // POST: Admin/UserEdit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserEdit(int id, UserEditViewModel model)
    {
        if (id != model.Id) return NotFound();

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Проверка уникальности логина при смене
        var checkLogin = await _context.Users
            .AnyAsync(u => u.Id != id && u.Login.ToLower() == model.Login.Trim().ToLower());
        if (checkLogin)
        {
            ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
        }

        if (ModelState.IsValid)
        {
            user.Login = model.Login.Trim();
            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim();
            user.Phone = model.Phone?.Trim();
            user.RoleId = model.RoleId;
            user.Specialization = model.Specialization?.Trim();

            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.PasswordHash = _hasher.HashPassword(model.NewPassword);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Пользователь {user.FullName} успешно отредактирован.";
            return RedirectToAction(nameof(Users));
        }

        ViewBag.Roles = new SelectList(await _context.Roles.ToListAsync(), "Id", "Name", model.RoleId);
        return View(model);
    }

    // POST: Admin/UserDelete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserDelete(int id)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (id == currentUserId)
        {
            TempData["ErrorMessage"] = "Вы не можете удалить собственный аккаунт!";
            return RedirectToAction(nameof(Users));
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Пользователь {user.FullName} удалён.";
        return RedirectToAction(nameof(Users));
    }

    // ==========================================
    // Управление услугами клиники
    // ==========================================

    // GET: Admin/Services
    public async Task<IActionResult> Services()
    {
        var services = await _context.Services.OrderBy(s => s.Name).ToListAsync();
        return View(services);
    }

    // GET: Admin/ServiceCreate
    public IActionResult ServiceCreate()
    {
        return View(new Service { DurationMinutes = 30, IsActive = true });
    }

    // POST: Admin/ServiceCreate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ServiceCreate(Service service)
    {
        if (ModelState.IsValid)
        {
            _context.Services.Add(service);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Услуга «{service.Name}» добавлена в прейскурант.";
            return RedirectToAction(nameof(Services));
        }
        return View(service);
    }

    // GET: Admin/ServiceEdit/5
    public async Task<IActionResult> ServiceEdit(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();
        return View(service);
    }

    // POST: Admin/ServiceEdit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ServiceEdit(int id, Service service)
    {
        if (id != service.Id) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(service);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Услуга «{service.Name}» успешно обновлена.";
            return RedirectToAction(nameof(Services));
        }
        return View(service);
    }

    // POST: Admin/ServiceDelete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ServiceDelete(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();

        var hasVisits = await _context.Visits.AnyAsync(v => v.ServiceId == id);
        if (hasVisits)
        {
            // Мягкое отключение вместо жесткого удаления, если есть связанные приёмы
            service.IsActive = false;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Услуга «{service.Name}» деактивирована, так как с ней связаны приёмы.";
            return RedirectToAction(nameof(Services));
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Услуга «{service.Name}» удалена.";
        return RedirectToAction(nameof(Services));
    }

    // ==========================================
    // Обозреватель базы данных (СУБД SQLite)
    // ==========================================

    // GET: Admin/Database
    public async Task<IActionResult> Database(string? table, string? sqlQuery)
    {
        var vm = new DatabaseViewerViewModel();

        var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "vetclinic.db");
        if (System.IO.File.Exists(dbPath))
        {
            var fileInfo = new FileInfo(dbPath);
            vm.DbPath = fileInfo.FullName;
            vm.DbFileName = fileInfo.Name;
            vm.FileSizeBytes = fileInfo.Length;
            vm.LastModified = fileInfo.LastWriteTime;
        }
        else
        {
            vm.DbPath = dbPath;
            vm.DbFileName = "vetclinic.db";
        }

        var tableNames = new (string Name, string Display)[]
        {
            ("Pets", "Питомцы (Pets)"),
            ("Visits", "Приёмы (Visits)"),
            ("Users", "Пользователи (Users)"),
            ("Roles", "Роли (Roles)"),
            ("Services", "Услуги (Services)"),
            ("PetTypes", "Виды животных (PetTypes)")
        };

        var conn = _context.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            foreach (var t in tableNames)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT COUNT(*) FROM \"{t.Name}\"";
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                vm.Tables.Add(new TableSummary { Name = t.Name, DisplayName = t.Display, RowCount = count });
            }

            if (!string.IsNullOrWhiteSpace(sqlQuery))
            {
                vm.CustomSql = sqlQuery;
                var trimmed = sqlQuery.Trim();
                if (!trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    vm.SqlError = "В целях безопасности разрешены только запросы на чтение (SELECT ...).";
                }
                else
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = trimmed;
                    using var reader = await cmd.ExecuteReaderAsync();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        vm.ColumnNames.Add(reader.GetName(i));
                    }
                    while (await reader.ReadAsync())
                    {
                        var row = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "");
                        }
                        vm.Rows.Add(row);
                    }
                    vm.TotalRows = vm.Rows.Count;
                    vm.SelectedTable = "Результат SQL";
                }
            }
            else
            {
                var targetTable = string.IsNullOrWhiteSpace(table) ? "Pets" : table;
                if (!tableNames.Any(t => t.Name.Equals(targetTable, StringComparison.OrdinalIgnoreCase)))
                {
                    targetTable = "Pets";
                }
                vm.SelectedTable = targetTable;

                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"SELECT * FROM \"{targetTable}\" LIMIT 100";
                using var reader = await cmd.ExecuteReaderAsync();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    vm.ColumnNames.Add(reader.GetName(i));
                }
                while (await reader.ReadAsync())
                {
                    var row = new List<string>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader.IsDBNull(i) ? "NULL" : reader.GetValue(i)?.ToString() ?? "");
                    }
                    vm.Rows.Add(row);
                }
                vm.TotalRows = vm.Rows.Count;
            }
        }
        catch (Exception ex)
        {
            vm.SqlError = $"Ошибка выполнения SQL: {ex.Message}";
        }
        finally
        {
            await conn.CloseAsync();
        }

        return View(vm);
    }
}
