using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public class Programme
{
    public int Id { get; set; }

    [Required, MinLength(2), MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Auto-suppression après 5 semaines
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(35);

    public bool Actif { get; set; } = true;

    // FK vers le Membre
    public int MembreId { get; set; }

    [ForeignKey(nameof(MembreId))]
    public Utilisateur? Membre { get; set; }

    // Exercices liés à ce programme
    public List<ProgrammeExercice> ProgrammeExercices { get; set; } = [];
}

// Table de jointure Programme ↔ Exercice
public class ProgrammeExercice
{
    public int Id { get; set; }

    public int ProgrammeId { get; set; }
    [ForeignKey(nameof(ProgrammeId))]
    public Programme? Programme { get; set; }

    public int ExerciceId { get; set; }
    [ForeignKey(nameof(ExerciceId))]
    public Exercice? Exercice { get; set; }

    public int Ordre { get; set; } // position dans le programme
}