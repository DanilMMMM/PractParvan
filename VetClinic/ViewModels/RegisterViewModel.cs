using System.ComponentModel.DataAnnotations;

namespace VetClinic.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите ФИО")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Длина ФИО должна быть от 3 до 100 символов")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите логин")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Логин должен быть от 3 до 50 символов")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Логин может содержать только латинские буквы, цифры, дефис и подчеркивание")]
    [Display(Name = "Логин")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите адрес электронной почты")]
    [EmailAddress(ErrorMessage = "Некорректный формат email")]
    [StringLength(100)]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Некорректный номер телефона")]
    [StringLength(25)]
    [Display(Name = "Номер телефона")]
    public string? Phone { get; set; }

    [Required(ErrorMessage = "Введите пароль")]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Пароль должен содержать минимум 4 символа")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль")]
    [Display(Name = "Желаемая роль")]
    public int RoleId { get; set; }

    [Display(Name = "Специализация (для врача)")]
    public string? Specialization { get; set; }
}
