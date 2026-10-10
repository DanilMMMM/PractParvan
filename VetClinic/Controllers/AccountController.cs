using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetClinic.Data;
using VetClinic.Models;
using VetClinic.Services;
using VetClinic.ViewModels;

namespace VetClinic.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _hasher;

    public AccountController(AppDbContext context, IPasswordHasher hasher)
    {
        _context = context;
        _hasher = hasher;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Login.ToLower() == model.Login.Trim().ToLower());

        if (user == null || !_hasher.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim("FullName", user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "Клиент")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        TempData["SuccessMessage"] = $"Добро пожаловать, {user.FullName}!";

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        // Редирект в зависимости от роли для удобства
        if (user.Role?.Name == "Администратор")
            return RedirectToAction("Index", "Admin");
        if (user.Role?.Name == "Ветеринар")
            return RedirectToAction("Index", "Visits");

        return RedirectToAction("Index", "Pets");
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Roles = new SelectList(
            await _context.Roles.Where(r => r.Name != "Администратор").ToListAsync(),
            "Id", "Name");

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = new SelectList(
                await _context.Roles.Where(r => r.Name != "Администратор").ToListAsync(),
                "Id", "Name", model.RoleId);
            return View(model);
        }

        var normalizedLogin = model.Login.Trim().ToLower();
        if (await _context.Users.AnyAsync(u => u.Login.ToLower() == normalizedLogin))
        {
            ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
            ViewBag.Roles = new SelectList(
                await _context.Roles.Where(r => r.Name != "Администратор").ToListAsync(),
                "Id", "Name", model.RoleId);
            return View(model);
        }

        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower()))
        {
            ModelState.AddModelError("Email", "Пользователь с таким Email уже зарегистрирован");
            ViewBag.Roles = new SelectList(
                await _context.Roles.Where(r => r.Name != "Администратор").ToListAsync(),
                "Id", "Name", model.RoleId);
            return View(model);
        }

        var role = await _context.Roles.FindAsync(model.RoleId) 
            ?? await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Клиент");

        var newUser = new User
        {
            Login = model.Login.Trim(),
            PasswordHash = _hasher.HashPassword(model.Password),
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            Phone = model.Phone?.Trim(),
            RoleId = role!.Id,
            Specialization = role.Name == "Ветеринар" ? (model.Specialization ?? "Ветеринарный врач") : null,
            CreatedAt = DateTime.Now
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Регистрация успешно завершена! Теперь войдите в систему.";
        return RedirectToAction(nameof(Login));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["SuccessMessage"] = "Вы успешно вышли из системы.";
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (user == null) return NotFound();

        var vm = new ProfileViewModel
        {
            Id = user.Id,
            Login = user.Login,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.Phone,
            RoleName = user.Role?.Name ?? "Клиент",
            Specialization = user.Specialization,
            CreatedAt = user.CreatedAt
        };

        return View(vm);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.RoleName = user.Role?.Name ?? "Клиент";
            model.CreatedAt = user.CreatedAt;
            return View(model);
        }

        // Проверка смены пароля, если указан
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Введите текущий пароль для подтверждения смены");
                model.RoleName = user.Role?.Name ?? "Клиент";
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            if (!_hasher.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError("CurrentPassword", "Неверный текущий пароль");
                model.RoleName = user.Role?.Name ?? "Клиент";
                model.CreatedAt = user.CreatedAt;
                return View(model);
            }

            user.PasswordHash = _hasher.HashPassword(model.NewPassword);
        }

        user.FullName = model.FullName.Trim();
        user.Email = model.Email.Trim();
        user.Phone = model.Phone?.Trim();

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Профиль успешно обновлён!";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
