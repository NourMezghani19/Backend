using System.ComponentModel.DataAnnotations;
using backend.Models;

namespace backend.DTOs.Programmes;

// ─── Créer un programme ───────────────────────────────────────────────────────
public record CreateProgrammeDto(
    [Required(ErrorMessage = "Nom obligatoire")]
    [MinLength(2), MaxLength(150)]
    string Nom,

    string? Description,

    // Liste des exercices à créer ET lier au programme
    [Required]
    List<CreateExerciceProgrammeDto> Exercices
);

// Un exercice dans le formulaire de création de programme
public record CreateExerciceProgrammeDto(
    [Required] string Nom,
    string? Description,

    [Range(1, 20)] int Series = 3,
    [Range(1, 200)] int Repetitions = 12,

    ModeExecution Mode = ModeExecution.Normal,

    // Exercice B si mode Superset
    string? SupersetNom = null,
    string? SupersetDescription = null,
    int SupersetSeries = 3,
    int SupersetRepetitions = 12
);

// ─── Réponse programme ────────────────────────────────────────────────────────
public record ProgrammeResponseDto(
    int Id,
    string Nom,
    string? Description,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool Actif,
    int MembreId,
    List<ProgrammeExerciceDto> Exercices
);

public record ProgrammeExerciceDto(
    int ExerciceId,
    string Nom,
    int Series,
    int Repetitions,
    string Mode,
    string? Description,
    int Ordre
);