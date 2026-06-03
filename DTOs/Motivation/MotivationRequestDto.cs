namespace backend.DTOs.Motivation
{
    public class MotivationRequestDto
    {
        public string Content { get; set; } = string.Empty;
        public List<MessageDto>? History { get; set; }
    }
}
