namespace backend.DTOs.Membre
{
    public class MembreProfilDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string NomComplet { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }

        public float Taille { get; set; }  
        public float Poids { get; set; }   
        public string? PhotoProfile { get; set; }  
        public DateTime DateInscription { get; set; }
        
        public float IMC { get; set; }  
        public string CategorieIMC { get; set; } = string.Empty;
        // "Insuffisance pondérale" | "Normal" | "Surpoids" | "Obésité"
    }
}
