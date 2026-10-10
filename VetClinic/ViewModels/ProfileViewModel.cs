using System.ComponentModel.DataAnnotations;

namespace VetClinic.ViewModels;

public class ProfileViewModel
{
    public int Id { get; set; }

    [Display(Name = "Логин")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите ФИО")]
    [StringLength(100)]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите Email")]
    [EmailAddress]
    [StringLength(100)]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(25)]
    [Display(Name = "Номер телефона")]
    public string? Phone { get; set; }

    [Display(Name = "Роль")]
    public string RoleName { get; set; } = string.Empty;

    [Display(Name = "Специализация")]
    public string? Specialization { get; set; }

    [Display(Name = "Дата регистрации")]
    public DateTime CreatedAt { get; set; }

    // Для смены пароля
    [DataType(DataType.Password)]
    [Display(Name = "Текущий пароль")]
    public string? CurrentPassword { get; set; }

    [StringLength(100, MinimumLength = 4)]
    [DataType(DataType.Password)]
    [Display(Name = "Новый пароль (оставьте пустым, если не меняете)")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение нового пароля")]
    public string? ConfirmNewPassword { get; set; }
}
