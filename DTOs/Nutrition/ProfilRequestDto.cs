namespace backend.DTOs.Nutrition
{
    public class ProfilRequestDto
    {
        public float poids_kg { get; set; }
        public float taille_cm { get; set; }
        public int age { get; set; }
        public string sexe { get; set; }      // "homme" / "femme"
        public string activite { get; set; }  // "sedentaire", "leger", etc.
    }

    public class PlanRequestDto
    {
        public int calories_cible { get; set; }
        public string objectif { get; set; }  // "perte_de_poids", "prise_de_masse", "maintien"
        public List<string> allergies { get; set; } = new();
    }
}