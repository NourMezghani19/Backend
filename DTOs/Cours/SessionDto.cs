using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cours;

////////////////////////////////////////////////////////////////////////  planifier une session dto
public record PlanifierSessionDto(
    [Required] int CoursId,
    [Required] int CoachId,
    [Required] DateTime DateHeure
);

//////////////////////////////////////////////////////////////////////// Modifier l'Horaire d'une Dto
public record ModifierHoraireDto(
    [Required] DateTime NouvelleDate
);
////////////////////////////////////////////////////////////////////////  reponse API sessions : SessionResponseDto    

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