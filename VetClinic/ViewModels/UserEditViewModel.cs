using System.ComponentModel.DataAnnotations;

namespace VetClinic.ViewModels;

public class UserEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите логин")]
    [StringLength(50, MinimumLength = 3)]
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
    [Display(Name = "Телефон")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Выберите роль")]
    [Display(Name = "Роль")]
    public int RoleId { get; set; }

    [Display(Name = "Специализация")]
    public string? Specialization { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 4)]
    [Display(Name = "Новый пароль (оставьте пустым, если не меняется)")]
    public string? NewPassword { get; set; }
}
