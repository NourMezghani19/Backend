using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cours;

// ════ PLANIFIER SESSION (Super Admin) ══════════════
public record PlanifierSessionDto(
    [Required] int CoursId,
    [Required] int CoachId,
    [Required] DateTime DateHeure
);

// ════ MODIFIER HORAIRE (Administrateur) ════════════
public record ModifierHoraireDto(
    [Required] DateTime NouvelleDate
);

// ════ RÉPONSE SESSION ═══════════════════════════════
public record SessionResponseDto(
    int Id,
    int CoursId,
    string CoursNom,
    string CoursGenre,
    string CoursGenreLabel,
    int CoursCapaciteMax,
    int CoachId,
    string CoachNom,
    string CoachPrenom,
    string CoachNomComplet,
    string CoachSpecialite,
    DateTime DateHeure,
    int PlacesDisponibles,
    string Statut
);