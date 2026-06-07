namespace backend.DTOs.Membre
{
    public class MembresListResponseDto
    {
        public int Count { get; set; }
        public string Search { get; set; } = "";
        public List<MembreResponseDto> Membres { get; set; } = new();
    }
}
