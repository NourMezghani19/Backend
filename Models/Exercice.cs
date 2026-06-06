using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models;

public enum ModeExecution
{
    Normal,
    Dropset,
    Superset
    
}

public class Exercice
{
    public int Id { get; set; }

    [Required, MinLength(2), MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Range(1, 200)]
    public int Repetitions { get; set; }

    [Range(1, 20)]
    public int Series { get; set; }

    public ModeExecution Mode { get; set; } = ModeExecution.Normal;

    public string? Description { get; set; }

    public bool Actif { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Auto-suppression après 5-6 semaines
    public DateTime? ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(42);

    // FK vers le Membre
    public int MembreId { get; set; }

    [ForeignKey(nameof(MembreId))]
    public Utilisateur? Membre { get; set; }
}