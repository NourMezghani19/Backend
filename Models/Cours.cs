using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Cours
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [MaxLength(100)]
    public string Nom { get; set; } = "";

    public string? Description { get; set; }

    [Range(1, 30, ErrorMessage = "Capacite entre 1 et 30")]
    public int CapaciteMax { get; set; } = 20;

    public GenreCours Genre { get; set; } = GenreCours.Mixte;

    public bool Actif { get; set; } = true;

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Navigation — sessions de ce cours
    public ICollection<Session_Cours> Sessions = new List<Session_Cours>();
}