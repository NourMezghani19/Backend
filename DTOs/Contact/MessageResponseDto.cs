namespace backend.DTOs.Contact
{
    public class MessageResponseDto
    {
        public int Id { get; set; }
        public string NomPrenom { get; set; } = "";
        public string Email { get; set; } = "";
        public string Sujet { get; set; } = "";
        public string Message { get; set; } = "";
        public bool Lu { get; set; }
        public DateTime EnvoyeLe { get; set; }
        public string? MembreNom { get; set; }
    }
}
