using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Salle
{
    public record CreateSalleDto(
     [Required, MaxLength(80)] string Nom,
     [Range(1, 500)] int Capacite,
     string? Description
 );
}
