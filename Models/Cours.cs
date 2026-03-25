using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Cours
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom est obligatoire")]
    [MaxLength(100)]
    public string Nom { get; set; } = "";

    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La capacité doit être d'au moins 1 personne")]
    public int CapaciteMax { get; set; } = 20;

    public GenreCours Genre { get; set; } = GenreCours.Mixte;

    public bool Actif { get; set; } = true;

    [Required]
    public string JourSemaine { get; set; } = "Lundi"; // Lundi, Mardi, Mercredi...
    [Required]

    public TimeSpan HeureDebut { get; set; } // Utilise TimeSpan pour les calculs
    [Required]

    public TimeSpan HeureFin { get; set; }
    // Navigation — sessions de ce cours
    public ICollection<Session_Cours> Sessions = new List<Session_Cours>();
}