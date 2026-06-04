namespace backend.DTOs.Salle
{
    public record SalleResponseDto(
     int Id,
     string Nom,
     int Capacite,
     string? Description,
     bool Disponible
 );

}
