namespace backend.Models
{
    public class SuperAdministrateur : Utilisateur
    {
        public override void SeConnecter() { }
        public override void SeDeconnecter() { }
        public override void ModifierProfil() { }
        public void CreerAdministrateur() { }
        public void SupprimerAdministrateur() { }
    }
}
