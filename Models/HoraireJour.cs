using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class HoraireJour
    {
        [Required]
        public string Jour { get; set; } = string.Empty; // "lundi", "mardi", etc.

        [Required, RegularExpression(@"^\d{2}:\d{2}$")]
        public string HeureOuverture { get; set; } = "06:00";

        [Required, RegularExpression(@"^\d{2}:\d{2}$")]
        public string HeureFermeture { get; set; } = "22:00";

        public bool EstOuvert { get; set; } = true;
        public static List<HoraireJour> Default() =>
    [
        new() { Jour = "lundi",    HeureOuverture = "06:00", HeureFermeture = "22:00", EstOuvert = true  },
        new() { Jour = "mardi",    HeureOuverture = "06:00", HeureFermeture = "22:00", EstOuvert = true  },
        new() { Jour = "mercredi", HeureOuverture = "06:00", HeureFermeture = "22:00", EstOuvert = true  },
        new() { Jour = "jeudi",    HeureOuverture = "06:00", HeureFermeture = "22:00", EstOuvert = true  },
        new() { Jour = "vendredi", HeureOuverture = "06:00", HeureFermeture = "22:00", EstOuvert = true  },
        new() { Jour = "samedi",   HeureOuverture = "08:00", HeureFermeture = "20:00", EstOuvert = true  },
        new() { Jour = "dimanche", HeureOuverture = "08:00", HeureFermeture = "14:00", EstOuvert = false },
    ];
    }

}

