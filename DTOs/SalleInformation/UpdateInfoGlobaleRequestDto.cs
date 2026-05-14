using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.SalleInformation
{
    public class UpdateInfoGlobaleRequestDto
    {
        [MaxLength(100)]
        public string? NomSalle { get; set; }

        [MaxLength(200)]
        public string? Adresse { get; set; }

        [MaxLength(20)]
        public string? Telephone { get; set; }

        [MaxLength(200)]
        public string? LienFacebook { get; set; }

        [MaxLength(200)]
        public string? LienInstagram { get; set; }


        public bool? Actif { get; set; }
    }
}
