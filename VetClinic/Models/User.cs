using System.ComponentModel.DataAnnotations;

namespace VetClinic.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    [Display(Name = "Логин")]
    public string Login { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    [Display(Name = "Электронная почта")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(25)]
    [Display(Name = "Номер телефона")]
    public string? Phone { get; set; }

    [Display(Name = "Специализация (для ветеринара)")]
    [StringLength(100)]
    public string? Specialization { get; set; }

    [Display(Name = "Роль")]
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    [Display(Name = "Дата регистрации")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Питомцы, принадлежащие пользователю (если клиент)
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();

    // Приёмы, где пользователь является клиентом
    public ICollection<Visit> ClientVisits { get; set; } = new List<Visit>();

    // Приёмы, которые ведёт данный врач-ветеринар
    public ICollection<Visit> VetVisits { get; set; } = new List<Visit>();
}
