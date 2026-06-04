// backend/Models/Salle.cs
namespace backend.Models;

public class Salle
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";   // "Salle A", "Salle Cardio"
    public int Capacite { get; set; }          // nb de places physiques
    public string? Description { get; set; }
    public bool Disponible { get; set; } = true;

    // Navigation
    public ICollection<Session_Cours> Sessions { get; set; }
        = new List<Session_Cours>();
}