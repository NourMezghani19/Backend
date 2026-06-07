using backend.DTOs.Avatar;

namespace backend.Services.Avatar.Interfaces
{
    public interface IAvatarService
    {
        /// <summary>
        /// Calcule l'IMC à partir du poids (kg) et de la taille (cm).
        /// </summary>
        float CalculerImc(float poids, float taille);

        /// <summary>
        /// Retourne la catégorie IMC selon les seuils OMS.
        /// </summary>
        string GetCategorieImc(float imc);

        /// <summary>
        /// Compare la situation actuelle avec l'objectif et retourne une analyse complète.
        /// </summary>
        AvatarComparisonDto Comparer(float taille, float poidsActuel, float poidsObjectif);
    }
}