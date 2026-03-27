namespace backend.DTOs.Reservation
{
    public class ReservationResponseDto
    {
        public int Id { get; set; }

        public int MembreId { get; set; }
        public string? MembreNom { get; set; }
        public string? MembrePrenom { get; set; }

        public int SessionId { get; set; }

        public string? NomCours { get; set; }

      
            // ... tes propriétés existantes ...
        public string? HeureDebut { get; set; } // Ajoute ceci
        public string? HeureFin { get; set; }   // Ajoute ceci
        
        public string Statut { get; set; } = string.Empty;

        public int PlacesRestantes { get; set; }
    }
}
