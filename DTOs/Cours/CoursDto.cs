using backend.Models;
using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Cours;
////////////////////////////////////////////////////////////////////////  create un cours     

public record CreateCoursDto(
    [Required(ErrorMessage = "Nom obligatoire")]
    [MinLength(3), MaxLength(100)]
    string       Nom,

    [Range(1, 30, ErrorMessage = "Capacite entre 1 et 30")]
    int           CapaciteMax,

    GenreCours Genre,

    string? Description
);

////////////////////////////////////////////////////////////////////////  Update un cours     
public record UpdateCoursDto(
    string? Nom,
    int? CapaciteMax,
    GenreCours? Genre,
    string? Description,
    bool? Actif
);

////////////////////////////////////////////////////////////////////////  reponse API cours : CoursResponseDto    
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

