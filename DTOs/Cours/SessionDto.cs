using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cours;

////////////////////////////////////////////////////////////////////////  planifier une session dto
public record PlanifierSessionDto(
    [Required] int CoursId,
    [Required] int CoachId,
     int? SalleId, // 🟢 AJOUTE CETTE LIGNE ICI
    [Required] string JourSemaine, // Ex: "Lundi"
    [Required] string HeureDebut, // Reçu du <input type="time">
    [Required] string HeureFin ); 
// Reçu du <input type="time">

//////////////////////////////////////////////////////////////////////// Modifier l'Horaire d'une Dto
public record ModifierHoraireDto(
    [Required]  string NouveauJour,
    [Required]  string NouvelleHeureDebut, // AJOUTÉ
    [Required]  string NouvelleHeureFin    // AJOUTÉ
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
    string JourSemaine,
    string HeureDebut, // Remplace "Heure"
    string HeureFin ,   // Ajouté
    int PlacesDisponibles,
    string Statut,
    // 🟢 AJOUTE CES 3 PARAMÈTRES POUR CORRESPONDRE AU MAPPING :
    int? SalleId,
    string SalleNom,
    int? SalleCapacite
);