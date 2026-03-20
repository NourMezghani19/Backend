using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cours;




// ═══ PLANIFIER UNE SESSION ══════════════════════════


public record PlanifierSessionDto(


    [Required]


    int      CoursId,




    [Required]


    int      CoachId,




    [Required]


    DateTime DateHeure


);




// ═══ MODIFIER HORAIRE D'UNE SESSION ════════════════


public record ModifierHoraireDto(


    [Required]


    DateTime NouvelleDate


);




// ═══ RÉPONSE API SESSION ════════════════════════════


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

