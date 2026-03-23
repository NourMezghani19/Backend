using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cours;

// ════ CRÉER UN COURS (Super Admin) ══════════════════
public record CreateCoursDto(
    [Required(ErrorMessage = "Nom obligatoire")]
    [MinLength(3), MaxLength(100)]
    string       Nom,

    [Range(1, 30, ErrorMessage = "Capacite entre 1 et 30")]
    int           CapaciteMax,

    GenreCours Genre,

    string? Description
);

// ════ MODIFIER UN COURS (Super Admin) ══════════════
public record UpdateCoursDto(
    string? Nom,
    int? CapaciteMax,
    GenreCours? Genre,
    string? Description,
    bool? Actif
);

// ════ RÉPONSE API COURS ══════════════════════════════
public record CoursResponseDto(
    int Id,
    string Nom,
    string? Description,
    int CapaciteMax,
    string Genre,
    string GenreLabel,
    bool Actif,
    int NbSessions,
    DateTime DateCreation
);