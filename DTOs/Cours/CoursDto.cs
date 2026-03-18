using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cours;




// ═══ CRÉER UN COURS ══════════════════════════════════


public record CreateCoursDto(


    [Required(ErrorMessage = "Nom obligatoire")]


    [MinLength(3), MaxLength(100)]


    string       Nom,




    [Range(1, 30, ErrorMessage = "Capacite entre 1 et 30")]


    int           CapaciteMax,




    GenreCours Genre,


        // Mixte=0 | Homme=1 | Femme=2



    string? Description



);




// ═══ MODIFIER UN COURS (PATCH) ══════════════════════


public record UpdateCoursDto(


    string? Nom,


    int? CapaciteMax,


    GenreCours? Genre,


    string? Description,


    bool? Actif


);




// ═══ RÉPONSE API COURS ══════════════════════════════


public record CoursResponseDto(


    int Id,


    string Nom,


    string? Description,


    int CapaciteMax,


    string Genre,


    // ex: "Homme"


    string GenreLabel,


    // ex: "Hommes uniquement"


    bool Actif,


    int NbSessions,


    DateTime DateCreation


);

