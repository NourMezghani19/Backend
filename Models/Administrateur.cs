namespace backend.Models
{
    public class Administrateur : Utilisateur
    {
        public override void SeConnecter() { }
        public override void SeDeconnecter() { }
        public override void ModifierProfil() { }
        public void CreerCompteMembre() { }
        public void EnvoyerEmailInscription() { }
    }
}
