using System.ComponentModel.DataAnnotations;

namespace VetClinic.Models;

public class Visit
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Выберите дату и время приёма")]
    [Display(Name = "Дата и время приёма")]
    public DateTime VisitDate { get; set; }

    [Required(ErrorMessage = "Выберите питомца")]
    [Display(Name = "Питомец")]
    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    [Required]
    [Display(Name = "Владелец / Клиент")]
    public int ClientId { get; set; }
    public User? Client { get; set; }

    [Required(ErrorMessage = "Выберите ветеринара")]
    [Display(Name = "Ветеринарный врач")]
    public int VeterinarianId { get; set; }
    public User? Veterinarian { get; set; }

    [Required(ErrorMessage = "Выберите услугу")]
    [Display(Name = "Услуга")]
    public int ServiceId { get; set; }
    public Service? Service { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Статус")]
    // Запланирован, В процессе, Завершён, Отменён
    public string Status { get; set; } = "Запланирован";

    [StringLength(1000)]
    [Display(Name = "Жалобы / Симптомы")]
    public string? Symptoms { get; set; }

    [StringLength(1000)]
    [Display(Name = "Диагноз")]
    public string? Diagnosis { get; set; }

    [StringLength(2000)]
    [Display(Name = "Назначения и лечение")]
    public string? Treatment { get; set; }

    [StringLength(1000)]
    [Display(Name = "Заметки врача")]
    public string? DoctorNotes { get; set; }

    [Display(Name = "Итоговая стоимость (руб.)")]
    [Range(0, 1000000)]
    public decimal TotalCost { get; set; }

    [Display(Name = "Дата создания записи")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string StatusBadgeClass => Status switch
    {
        "Запланирован" => "bg-warning text-dark",
        "В процессе" => "bg-primary text-white",
        "Завершён" => "bg-success text-white",
        "Отменён" => "bg-danger text-white",
        _ => "bg-secondary text-white"
    };
}
