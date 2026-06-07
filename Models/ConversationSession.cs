namespace backend.Models
{
    public class ConversationSession
    {
        public int Id { get; set; }
        public int MembreId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Titre auto-généré depuis le 1er message du membre</summary>
        public string Titre { get; set; } = "Nouvelle conversation";

        // Navigation
        public Utilisateur Membre { get; set; } = null!;
        public List<ConversationMessage> Messages { get; set; } = new();
    }
}