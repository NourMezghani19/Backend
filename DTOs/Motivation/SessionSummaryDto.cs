namespace backend.DTOs.Motivation
{
    public class SessionSummaryDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int NbMessages { get; set; }
    }

    /// <summary>Envoyé par le front pour démarrer ou continuer une session</summary>
    public class ChatRequestDto
    {
        /// <summary>
        /// null = nouvelle session, sinon ID session existante
        /// </summary>
        public int? SessionId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>Réponse du POST /motivation/chat enrichie</summary>
    public class ChatResponseDto
    {
        public int SessionId { get; set; }
        public string Reply { get; set; } = string.Empty;
    }
}

