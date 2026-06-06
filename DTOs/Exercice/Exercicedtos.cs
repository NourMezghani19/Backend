using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTOs.Exercices;

// ─── Response ────────────────────────────────────────────────────────────────
public record ExerciceResponseDto(
    int Id,
    string Nom,
    int Repetitions,
    int Series,
    string Mode,          // label lisible : "Dropset", "Normal", etc.
    string? Description,
    bool Actif,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    int MembreId
);

// ─── Create ───────────────────────────────────────────────────────────────────
public record CreateExerciceDto(
    [Required(ErrorMessage = "Nom obligatoire")]
    [MinLength(2), MaxLength(100)]
    string Nom,

    [Range(1, 200, ErrorMessage = "Répétitions entre 1 et 200")]
    int Repetitions,

    [Range(1, 20, ErrorMessage = "Séries entre 1 et 20")]
    int Series,

    ModeExecution Mode,

    string? Description
);

// ─── Update ───────────────────────────────────────────────────────────────────
public record UpdateExerciceDto(
    string? Nom,
    int? Repetitions,
    int? Series,
    ModeExecution? Mode,
    string? Description,
    bool? Actif
);

// ─── Programme généré par l'IA ────────────────────────────────────────────────
public record ProgrammeResponseDto(
    string Titre,
    string Description,
    List<JourProgrammeDto> Jours
);

public record JourProgrammeDto(
    string Jour,
    string Focus,
    List<ExerciceProgrammeDto> Exercices
);

public record ExerciceProgrammeDto(
    string Nom,
    int Series,
    int Repetitions,
    string Mode,
    string? Conseil
);