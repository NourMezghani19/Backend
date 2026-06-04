using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Salle
{
    public record AssignerSalleDto(
    [Required] int SalleId
);
}
