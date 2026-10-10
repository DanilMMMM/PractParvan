using System.ComponentModel.DataAnnotations;

namespace VetClinic.Models;

public class Service
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите название услуги")]
    [StringLength(120)]
    [Display(Name = "Название услуги")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите описание услуги")]
    [StringLength(1000)]
    [Display(Name = "Описание услуги")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите стоимость")]
    [Range(0, 1000000, ErrorMessage = "Стоимость не может быть отрицательной")]
    [Display(Name = "Стоимость (лей / MDL)")]
    public decimal Price { get; set; }

    [Required]
    [Range(5, 600, ErrorMessage = "Длительность от 5 до 600 минут")]
    [Display(Name = "Длительность (мин)")]
    public int DurationMinutes { get; set; } = 30;

    [Display(Name = "Активна для записи")]
    public bool IsActive { get; set; } = true;

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
