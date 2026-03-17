namespace backend.Models
{
    public class EmploiDuTemps
    {
        public int Id { get; set; }

        public int CoachId { get; set; }

        public Coach? Coach { get; set; }

        public int SessionId { get; set; }

        public Session_Cours? Session { get; set; }

        public DayOfWeek Jour { get; set; }

        public TimeSpan HeureDebut { get; set; }

        public TimeSpan HeureFin { get; set; }

        public bool Recurrent { get; set; } = true;
    }
}
