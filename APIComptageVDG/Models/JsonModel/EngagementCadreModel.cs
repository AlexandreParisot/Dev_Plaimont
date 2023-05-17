using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace APIComptageVDG.Models.JsonModel
{

   
    public class ComplementsEngagements_cadre
    {
        public double? tonnage_vendange { get; set; }
        public string nom_parcelle { get; set; }
        public string cepage { get; set; }
        public double? surface { get; set; }
        public string? nom_prestataire { get; set; }
        public double? tonnage_estime { get; set; }
        public object? surface_non_vendangee { get; set; }
        public string qualite { get; set; }
        public string appellation { get; set; }
        public string num_parcelle { get; set; }
        public double? surface_vendangee { get; set; }
        public double? tonnage_a_vendanger { get; set; }
        public string? vers_grappe_authorize { get; set; }
        public string? perforation_1_readonly { get; set; }
        public string? perforation_2_readonly { get; set; }
        public string? glomerule_readonly { get; set; }
    }

    public class Engagement_cadre
    {
        public string v_num_engagement_cadre { get; set; }
        public string v_person_typ { get; set; }
        public int i_person_num { get; set; }
        public string v_code_activite { get; set; }
        public string v_code_campagne { get; set; }

        [JsonPropertyName("complements")]
        public ComplementsEngagements_cadre complements { get; set; }

    }
    public class EngagementCadreModel
    {
        [JsonProperty("engagements_cadre")]
        public IList<Engagement_cadre> engagement_cadre { get; set; }

    }
}
