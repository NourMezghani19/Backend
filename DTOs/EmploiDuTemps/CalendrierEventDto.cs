namespace backend.DTOs.EmploiDuTemps
{
    public class CalendrierEventDto
    {
        public string Id { get; set; } = "";   // "edt-{id}"
        public string Title { get; set; } = "";   // "Prénom NOM"
        public int EdtId { get; set; }
        public int CoachId { get; set; }
        public string Jour { get; set; } = "";
        public string HeureDebut { get; set; } = "";
        public string HeureFin { get; set; } = "";
        public string? Note { get; set; }
        public string Color { get; set; } = "#4f8ef7";
        public string? Photo { get; set; }
    }
}
