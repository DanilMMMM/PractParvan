using System.ComponentModel.DataAnnotations;

namespace VetClinic.Models;

public class Role
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Display(Name = "Название роли")]
    public string Name { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();
}
