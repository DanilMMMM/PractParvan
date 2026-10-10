using System.ComponentModel.DataAnnotations;

namespace VetClinic.ViewModels;

public class VisitConductViewModel
{
    public int VisitId { get; set; }

    public string PetName { get; set; } = string.Empty;
    public string PetSpeciesAndBreed { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime VisitDate { get; set; }

    [Required(ErrorMessage = "Выберите статус приёма")]
    [Display(Name = "Статус")]
    public string Status { get; set; } = "Завершён";

    [Display(Name = "Жалобы / Симптомы")]
    [StringLength(1000)]
    public string? Symptoms { get; set; }

    [Required(ErrorMessage = "Укажите диагноз")]
    [Display(Name = "Диагноз")]
    [StringLength(1000)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите назначенное лечение и препараты")]
    [Display(Name = "Лечение и рекомендации")]
    [StringLength(2000)]
    public string Treatment { get; set; } = string.Empty;

    [Display(Name = "Заметки врача")]
    [StringLength(1000)]
    public string? DoctorNotes { get; set; }

    [Required(ErrorMessage = "Укажите итоговую стоимость")]
    [Range(0, 1000000, ErrorMessage = "Стоимость не может быть отрицательной")]
    [Display(Name = "Итоговая стоимость (руб.)")]
    public decimal TotalCost { get; set; }
}
