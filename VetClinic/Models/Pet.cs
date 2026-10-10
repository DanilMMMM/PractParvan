using System.ComponentModel.DataAnnotations;

namespace VetClinic.Models;

public class Pet
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Введите кличку животного")]
    [StringLength(60, MinimumLength = 1)]
    [Display(Name = "Кличка")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите вид животного")]
    [Display(Name = "Вид животного")]
    public int PetTypeId { get; set; }
    public PetType? PetType { get; set; }

    [StringLength(80)]
    [Display(Name = "Порода")]
    public string? Breed { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Дата рождения")]
    public DateTime? BirthDate { get; set; }

    [StringLength(10)]
    [Display(Name = "Пол")]
    public string Gender { get; set; } = "Не указан"; // Самец, Самка, Не указан

    [Display(Name = "Вес (кг)")]
    [Range(0.01, 500.0, ErrorMessage = "Вес должен быть от 0.01 до 500 кг")]
    public double? Weight { get; set; }

    [Required(ErrorMessage = "Необходимо указать владельца")]
    [Display(Name = "Владелец")]
    public int OwnerId { get; set; }
    public User? Owner { get; set; }

    [StringLength(1000)]
    [Display(Name = "Особенности здоровья / Аллергии")]
    public string? MedicalNotes { get; set; }

    [Display(Name = "Дата добавления")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Visit> Visits { get; set; } = new List<Visit>();

    // Вычисляемый возраст
    public string AgeDisplay
    {
        get
        {
            if (!BirthDate.HasValue) return "Не указан";
            var now = DateTime.Today;
            int years = now.Year - BirthDate.Value.Year;
            if (now < BirthDate.Value.AddYears(years)) years--;
            
            if (years <= 0)
            {
                int months = (now.Year - BirthDate.Value.Year) * 12 + now.Month - BirthDate.Value.Month;
                if (now.Day < BirthDate.Value.Day) months--;
                if (months <= 0) return "Меньше месяца";
                return $"{months} мес.";
            }

            string suffix;
            int lastDigit = years % 10;
            int lastTwoDigits = years % 100;
            if (lastTwoDigits >= 11 && lastTwoDigits <= 14) suffix = "лет";
            else if (lastDigit == 1) suffix = "год";
            else if (lastDigit >= 2 && lastDigit <= 4) suffix = "года";
            else suffix = "лет";

            return $"{years} {suffix}";
        }
    }
}
