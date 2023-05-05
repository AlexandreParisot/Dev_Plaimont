using System.Text.Json.Serialization;

namespace APIComptageVDG.Models.JsonModel
{
    public class CguModel
    {
        [JsonPropertyName("Id de la cgu")]
        public int Iddelacgu { get; set; }

        [JsonPropertyName("Identifiant du service")]
        public int Identifiantduservice { get; set; }

        [JsonPropertyName("Intitulé du service")]
        public string Intitulduservice { get; set; }
        public string Login { get; set; }
        public string Nom { get; set; }
        [JsonPropertyName("Prénom")]
        public string Prenom { get; set; }

        [JsonPropertyName("Type personne")]
        public string Typepersonne { get; set; }

        [JsonPropertyName("Numéro personne")]
        public int Numropersonne { get; set; }
    }

    public class GgusModel
    {
        public List<CguModel> cgus { get; set; }
    }
}
