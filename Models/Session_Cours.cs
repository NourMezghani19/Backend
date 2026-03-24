namespace backend.Models;

public class Session_Cours
{
    public int Id { get; set; }

    public int CoursId { get; set; }
    public Cours? Cours { get; set; }

    public int CoachId { get; set; }
    public Coach? Coach { get; set; }

    public DateTime DateHeure { get; set; }

    public int PlacesDisponibles { get; set; }

    public string Statut { get; set; } = "Planifie";
    // Planifie | Annule | Termine

    public ICollection<Reservation> Reservations = new List<Reservation>();
}