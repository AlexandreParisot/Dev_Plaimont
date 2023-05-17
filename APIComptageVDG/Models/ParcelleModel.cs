namespace APIComptageVDG.Models
{
    public class ParcelleModel
    {
        public bool inCampagne { get; set; }
        public int id_parcelle { get; set; }
        public int id_propriete { get; set; }
        public string? prestataire { get; set; }
        public string? type_vendange { get; set; }
        public string? totalement_vendangee { get; set; }
        public string? ut { get; set; }
        public string? nameParcelle { get; set; }
        public string? nameParcelle2 { get; set; }
        public string? propriete { get; set; }  
        public string? appellation { get; set; }
        public string? cepage { get; set; }
        public string? site_technique { get; set; }
        public string? site_vendange { get; set; }  
        public decimal? surface { get; set; }
        public decimal? superficie_vendangee { get; set; }
        public decimal? poids_vendanges { get; set; }
        public string? qualite { get; set; }
        public int? cptGlomerule { get; set; }
        public int? cptPerforation1 { get; set; }
        public int? cptPerforation2 { get; set; }
    }
}
