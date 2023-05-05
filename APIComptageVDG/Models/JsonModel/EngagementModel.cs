namespace APIComptageVDG.Models.JsonModel
{
    public class Engagements
    {
        public List<EngagementModel> engagements { get; set; }
    }

    public class EngagementModel
    {
        public string v_num_engagement { get; set; }
        public string v_code_activite { get; set; }
        public string v_person_typ { get; set; }
        public int i_person_num { get; set; }
        public string v_code_campagne { get; set; }
        public string v_code_production { get; set; }
        public string v_code_commercialisation { get; set; }
        public string v_num_engagement_cadre { get; set; }
        public Complements complements { get; set; }
    }

    public class Complements
    {
        public string statut_inscription { get; set; }
        public string date_fin_vendange_prevue { get; set; }
        public string date_debut_vendange_prevue { get; set; }
        public decimal? tonnage_estime { get; set; }
        public decimal? tonnage_annonce { get; set; }
        public string date_vendange_vigneron { get; set; }
        public string cont_date { get; set; }
        public string v_code_modele { get; set; }
        public string commentaire_vigneron { get; set; }
        public string commentaire_cave { get; set; }
        public int? tonnage_recolte { get; set; }
        public string date_vendange_confirmee { get; set; }
        public string date_vendange_reelle { get; set; }
        public string i_person_num { get; set; }
        public string bon_accord_societaire { get; set; }
        public string signature_societaire { get; set; }
        public string v_nom { get; set; }
        public int? i_uid_connect { get; set; }
        public string v_num_engagement_cadre { get; set; }
        public string v_raison_sociale { get; set; }
        public int? i_uid_concern { get; set; }
        public string v_person_typ { get; set; }
        public string v_prenom { get; set; }
        public double? tonnage_a_vendanger { get; set; }
        public string nb_glomerules { get; set; }
        //public string cont_date { get; set; }
        public string nb_perforation_1 { get; set; }
        //public int i_uid_connect { get; set; }
        public string nb_perforation_2 { get; set; }
        //public string v_code_modele { get; set; }



    }

}
