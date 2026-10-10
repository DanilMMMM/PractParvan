using System.ComponentModel.DataAnnotations;

namespace VetClinic.Models;

public class PetType
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Вид животного")]
    public string Name { get; set; } = string.Empty;

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
