namespace backend.DTOs.ReglementInterne
{
    public class ReglementInterneResponseDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "";
        public string Contenu { get; set; } = "";
        public DateTime DateModification { get; set; }
        public ReglementInterneResponseDto(
           int id, string titre, string contenu, DateTime dateModification)
        {
            Id = id;
            Titre = titre;
            Contenu = contenu;
            DateModification = dateModification;
        }
    }
}
