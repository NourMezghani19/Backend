namespace backend.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int MembreId { get; set; }
        public Membre? Membre { get; set; }
        public int SessionCoursId { get; set; }
        public Session_Cours? SessionCours { get; set; }
        public DateTime DateReservation { get; set; } = DateTime.UtcNow;
        public StatutReservation Statut { get; set; } = StatutReservation.EnAttente;

    }
}
