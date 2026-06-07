namespace backend.Models
{
    public class ConversationMessage
    {
        public int Id { get; set; }
        public int SessionId { get; set; }

        /// <summary>"user" ou "assistant"</summary>
        public string Role { get; set; } = "user";

        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ConversationSession Session { get; set; } = null!;
    }
}
