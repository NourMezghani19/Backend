namespace backend.DTOs.Membre
{
    public class MembreResponseDto
    {
        public int Id { get; set; }
        public string IdSalleSport { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Prenom { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Telephone { get; set; }
        public float Taille { get; set; }
        public float Poids { get; set; }
        public float ObjectifPoids { get; set; }          // ✅ ajouté
        public string? PhotoProfile { get; set; }
        public string DateInscription { get; set; } = "";
    }
}
