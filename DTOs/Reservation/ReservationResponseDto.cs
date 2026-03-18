namespace backend.DTOs.Reservation
{
    public class ReservationResponseDto
    {
        public int Id { get; set; }

        public int MembreId { get; set; }

        public int SessionId { get; set; }

        public string? NomCours { get; set; }

        public DateTime DateReservation { get; set; }

        public string Statut { get; set; } = string.Empty;

        public int PlacesRestantes { get; set; }
    }
}
