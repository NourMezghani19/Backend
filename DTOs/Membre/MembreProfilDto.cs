namespace backend.DTOs.Membre
{
    public class MembreProfilDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string Genre { get; set; } = "";
      
        public float ObjectifPoids { get; set; }          // ✅ ajouté
        public string? PhotoProfile { get; set; }
        public string DateInscription { get; set; } = "";
        public string? DateNaissance { get; set; }        // ✅ ajouté



        public float Taille { get; set; }   // en cm
        public float Poids { get; set; }   // en kg

        public float IMC { get; set; }  
        public string CategorieIMC { get; set; } = string.Empty;
        public string MotDePasse { get; set; }

    }
}
