using System.ComponentModel.DataAnnotations;

namespace VetClinic.ViewModels;

public class VisitCreateViewModel
{
    [Required(ErrorMessage = "Выберите питомца")]
    [Display(Name = "Питомец")]
    public int PetId { get; set; }

    [Required(ErrorMessage = "Выберите ветеринара")]
    [Display(Name = "Ветеринар")]
    public int VeterinarianId { get; set; }

    [Required(ErrorMessage = "Выберите услугу")]
    [Display(Name = "Услуга")]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "Выберите дату и время визита")]
    [Display(Name = "Дата и время визита")]
    public DateTime VisitDate { get; set; } = DateTime.Today.AddDays(1).AddHours(10);

    [StringLength(1000)]
    [Display(Name = "Жалобы / Причина обращения")]
    public string? Symptoms { get; set; }

    // Если создаёт администратор или врач, можно выбрать владельца
    public int? ClientId { get; set; }
}
