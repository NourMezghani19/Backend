namespace backend.Models
{
    public class Membre : Utilisateur
    {
        public float Taille { get; set; }
        public float Poids { get; set; }
        public string? PhotoProfile { get; set; }
        public DateTime DateInscription { get; set; } = DateTime.UtcNow;
        public override void SeConnecter() { }
        public override void SeDeconnecter() { }
        public override void ModifierProfil() { }
    }
}
