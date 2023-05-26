using APIComptageVDG.Helpers;
using APIComptageVDG.Models;
using APIComptageVDG.Models.JsonModel;
using APIComptageVDG.Provider;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ObjectiveC;

namespace APIComptageVDG.Services
{
    public class InstagrappeServcie
    {
        private InstagrappeApiProvider _provider;

        public void SetApiProviderInstagrappe(InstagrappeApiProvider providerInsta)
        {
            if (providerInsta == null)
                new NullReferenceException();

            this._provider = providerInsta!;
        }

        public bool SuccessConfig { get => (_provider == null ? false : _provider!.successConfig); }

        public async Task<string> getToken()
        {

            if (_provider?.successConfig == true)
            {
                var token = await _provider!.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    Gestion.Info($"Recuperation Token : {token}");
                    return token;
                }
                Gestion.Erreur("Une erreur s'est produit lors de la recupération du token.");
            }
            else
                Gestion.Erreur($"Il manque des informations dans Appsetting.json pour contacter l'api Instagrappe. ");

            return string.Empty;
        }

        public async Task<ReponseApi> GetEngagementCadreInstagrappe(string code_activite, int AnneeCampagne)
        {

            if (string.IsNullOrEmpty(code_activite))
                return new("code modele est vide.");



            var cCommandeGet = string.Empty;
            cCommandeGet += "code_activite=" + code_activite;
            cCommandeGet += "&code_campagne=" + AnneeCampagne;
            //cCommandeGet += "&indicateur_modele=v_code_modele";
            //cCommandeGet += "&indicateur_contrat=v_code_modele";
            //cCommandeGet += "&valeur_contrat=MODELE_" + code_activite;


            if (await getToken() is string token && !string.IsNullOrEmpty(token))
            {
                var result = await _provider.CallApi("export/engagementCadre", HttpMethod.Get, entetedata: cCommandeGet);
                if (result.IsSuccessStatusCode)
                {
                    var str = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var EngagementCadre = JsonSerialize.Deserialize<EngagementCadreModel>(str);
                        if (EngagementCadre != null)
                        {
                            return new(EngagementCadre, true);
                        }

                        return new(str, true);
                    }
                    else
                        return new ReponseApi(str);
                }
                else
                    return new ReponseApi(await result.Content.ReadAsStringAsync());
            }
            else
                return new ReponseApi("Probléme de connexion a l'api Instagrappe.");
        }


        public async Task<ReponseApi> GetEngagementCadreVersDeGrappe(int annee)
        {

            var cCommandeGet = string.Empty;
            cCommandeGet += "code_activite=PARCELLE";
            cCommandeGet += "&code_campagne=" + annee;

            if (await getToken() is string token && !string.IsNullOrEmpty(token))
            {
                var result = await _provider.CallApi("export/engagementCadre", HttpMethod.Get, entetedata: cCommandeGet);
                if (result.IsSuccessStatusCode)
                {
                    var str = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var EngagementCadre = JsonSerialize.Deserialize<EngagementCadreModel>(str);
                        return new(EngagementCadre, true);
                    }
                    else
                        return new(str);
                }
                else
                    return new(await result.Content.ReadAsStringAsync());
            }
            else
                return new("Probléme de connexion a l'api Instagrappe.");
        }



        public async Task<ReponseApi> GetEngagementInstagrappe(string code_activite, int AnneeCampagne)
        {

            if (string.IsNullOrEmpty(code_activite))
                return new ReponseApi("code modele est vide.");


            var cCommandeGet = string.Empty;
            cCommandeGet += "code_activite=" + code_activite;
            cCommandeGet += "&code_campagne=" + AnneeCampagne;
            cCommandeGet += "&indicateur_modele=v_code_modele";
            cCommandeGet += "&indicateur_contrat=v_code_modele";
            cCommandeGet += "&valeur_contrat=MODELE_" + code_activite;


            if (await getToken() is string token && !string.IsNullOrEmpty(token))
            {
                var result = await _provider.CallApi("export/engagement", HttpMethod.Get, entetedata: cCommandeGet);
                if (result.IsSuccessStatusCode)
                {
                    var str = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var Engagement = JsonSerialize.Deserialize<Engagements>(str);
                        if (Engagement != null)
                            return new(Engagement, true);
                        else
                            return new(str, true);
                    }
                    else
                        return new(str);
                }
                else
                    return new(await result.Content.ReadAsStringAsync());
            }
            else
                return new("Probléme de connexion a l'api Instagrappe.");


        }


        public async Task<ReponseApi> GetEngagementVersDeGrappe(int annee)
        {
            var cCommandeGet = string.Empty;
            cCommandeGet += "code_activite=PARCELLE";
            cCommandeGet += "&code_campagne=" + annee;
            cCommandeGet += "&indicateur_modele=v_code_modele";
            cCommandeGet += "&indicateur_contrat=statut_inscription";
            cCommandeGet += "&valeur_contrat=OUVERTE";

            if (await getToken() is string token && !string.IsNullOrEmpty(token))
            {
                var result = await _provider.CallApi("export/engagement", HttpMethod.Get, entetedata: cCommandeGet);
                if (result.IsSuccessStatusCode)
                {
                    var str = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var Engagement = JsonSerialize.Deserialize<Engagements>(str);
                        if (Engagement != null)
                            return new(Engagement, true);
                        else
                            return new(str, true);
                    }
                    else
                        return new(str);
                }
                else
                    return new(await result.Content.ReadAsStringAsync());
            }
            else
                return new("Probléme de connexion a l'api Instagrappe.");
        }



        public async Task<ReponseApi> SetImportInstagrappe(string fileJson)
        {
            var cCommandeGet = string.Empty;
            cCommandeGet += "file=" + System.IO.Path.GetFileName(fileJson);

            if (await getToken() is string token && !string.IsNullOrEmpty(token))
            {
                var result = await _provider.CallApi($"import/apport", HttpMethod.Post, entetedata: cCommandeGet);
                if (result.IsSuccessStatusCode)
                {
                    var str = await result.Content.ReadAsStringAsync();
                    return new ReponseApi(str, true);
                }
                else
                {
                    var erreur = await result.Content.ReadAsStringAsync();
                    Gestion.Erreur(erreur);
                    return new(erreur);
                }

            }
            else
                return new("Probléme de connexion à l'api Instagrappe.");

        }


        public async Task<ReponseApi> GenerateEngagementCadreVDGInstagrappe(IEnumerable<ParcelleModel> parcelles, IEnumerable<PeriodeModel> periodes, int annee)
        {
            if (parcelles == null)
                parcelles = new List<ParcelleModel>();

            if(periodes == null)
                periodes = new List<PeriodeModel>();

            var result = await GetEngagementCadreVersDeGrappe(annee);
            if (result.success && result.result is EngagementCadreModel engagements)
            {
                EngagementCadreModel ListSend = new EngagementCadreModel();
                var ParcelleOuverteInstagrappe = mappingParcellesToEngagementCadre(parcelles.ToList(), periodes.ToList(), annee);
                if( ParcelleOuverteInstagrappe.engagement_cadre.Count > 0 && engagements.engagement_cadre.Count > 0 )
                {
                    var lstInstOuvert = new List<Engagement_cadre>();
                    foreach(Engagement_cadre instaEngagementCardre in engagements.engagement_cadre)
                    {
                        var trouve = false;
                        if (instaEngagementCardre.complements != null)
                            instaEngagementCardre.complements.vers_grappe_authorize = "non";

                        foreach (Engagement_cadre ouvertureParcelle in ParcelleOuverteInstagrappe.engagement_cadre)
                        {
                            if(instaEngagementCardre.v_num_engagement_cadre == ouvertureParcelle.v_num_engagement_cadre)
                            {
                                trouve = true;
                                break;
                            }
                        } 
                        if(!trouve)
                            lstInstOuvert.Add(instaEngagementCardre);
                    }
                    if(lstInstOuvert.Count > 0)
                    {
                       var engagementCadre = ParcelleOuverteInstagrappe.engagement_cadre.ToList();
                        engagementCadre.AddRange(lstInstOuvert);
                        ParcelleOuverteInstagrappe.engagement_cadre = engagementCadre;
                    }

                    ListSend = ParcelleOuverteInstagrappe;
                        //engagements.engagements_cadre.Where(engagements => ParcelleOuverteInstagrappe.engagements_cadre.ToList().ForEach(e => e.v_num_engagement_cadre != engagements.v_num_engagement_cadre));
                }
                else if (engagements.engagement_cadre.Count > 0)
                {
                   foreach(Engagement_cadre instaEngagementCardre in engagements.engagement_cadre)
                    {
                        if (instaEngagementCardre.complements != null)
                            instaEngagementCardre.complements.vers_grappe_authorize = "non";
                    }
                    ListSend = engagements;
                }
                else if (ParcelleOuverteInstagrappe.engagement_cadre.Count > 0)
                {
                    ListSend = ParcelleOuverteInstagrappe;
                }

                if(ListSend.engagement_cadre != null && ListSend.engagement_cadre.Count > 0)
                {
                    var nameFileJson = $"apport_{DateTime.Now.ToString("yyyyMMddHHmmss")}.json";
                    var strContentFile = @$"{{
                                                ""incremental"": {{
                                                    ""upsert"": 
                                                            {JsonSerialize.Serialize(ListSend).Replace("engagements_cadre", "engagement_cadre")}                                                              
                                                    }}
                                            }}";

                    System.IO.File.WriteAllText(nameFileJson, strContentFile);

                    return new(nameFileJson,true);
                }
            }
            else
                Gestion.Erreur($"Etat : {result.success} - reponse : {result.result}");

            return new(string.Empty);
        }



        public async Task<ReponseApi> GenerateEngagementVDGInstagrappe(IEnumerable<ParcelleModel> parcelles, int annee)
        {
            if (parcelles == null)
                parcelles = new List<ParcelleModel>();

          
            var result = await GetEngagementVersDeGrappe(annee);
            if (result.success && result.result is Engagements engagements)
            {
                Engagements ListSend = new Engagements();
                var ParcelleOuverteInstagrappe = mappingParcellesToEngagement(parcelles.ToList(), annee);
                if (ParcelleOuverteInstagrappe.engagements.Count > 0 && engagements.engagements.Count > 0)
                {
                    var lstInstOuvert = new List<EngagementModel>();
                    foreach (EngagementModel instaEngagementCardre in engagements.engagements)
                    {
                        var trouve = false;
                        //if (instaEngagementCardre.complements != null)
                        //    instaEngagementCardre.complements. = "non";

                        foreach (EngagementModel ouvertureParcelle in ParcelleOuverteInstagrappe.engagements)
                        {
                            if (instaEngagementCardre.v_num_engagement == ouvertureParcelle.v_num_engagement)
                            {
                                trouve = true;
                            }
                        }
                        if (!trouve)
                            lstInstOuvert.Add(instaEngagementCardre);
                    }
                    if (lstInstOuvert.Count > 0)
                    {
                        ParcelleOuverteInstagrappe.engagements.ToList().AddRange(lstInstOuvert);
                    }

                    ListSend = ParcelleOuverteInstagrappe;
                    //engagements.engagements_cadre.Where(engagements => ParcelleOuverteInstagrappe.engagements_cadre.ToList().ForEach(e => e.v_num_engagement_cadre != engagements.v_num_engagement_cadre));
                }
                else if (engagements.engagements.Count > 0)
                {
                    //foreach (EngagementModel instaEngagementCardre in engagements.engagements)
                    //{
                    //    if (instaEngagementCardre.complements != null)
                    //        instaEngagementCardre.complements.vers_grappe_authorize = "non";
                    //}
                    ListSend = engagements;
                }
                else if (ParcelleOuverteInstagrappe.engagements.Count > 0)
                {
                    ListSend = ParcelleOuverteInstagrappe;
                }

                if (ListSend.engagements != null && ListSend.engagements.Count > 0)
                {
                    var nameFileJson = $"apport_{DateTime.Now.ToString("yyyyMMddHHmmss")}.json";
                    var strContentFile = @$"{{
                                                ""incremental"": {{
                                                    ""upsert"": 
                                                            {JsonSerialize.Serialize(ListSend)}                                                              
                                                    }}
                                            }}";

                    System.IO.File.WriteAllText(nameFileJson, strContentFile);

                    return new(nameFileJson, true);
                }
            }
            else
                Gestion.Erreur($"Etat : {result.success} - reponse : {result.result}");

            return new(string.Empty);
        }



        private EngagementCadreModel mappingParcellesToEngagementCadre(List<ParcelleModel> parcelles, List<PeriodeModel> periodes, int annee)
        {
            var engagements = new EngagementCadreModel();
            engagements.engagement_cadre = new List<Engagement_cadre>();
            bool glomeruleRead = true;
            var glomerule = periodes.FirstOrDefault(x => x.Name.ToLower() == "glomerule");
            if(glomerule != null)
            {
                if(DateTime.Today >= glomerule.DateDebut && DateTime.Today <= glomerule.DateFin)
                    glomeruleRead = false;
            }
            bool perforationRead = true;
            var perforation = periodes.FirstOrDefault(x => x.Name.ToLower() == "perforation");
            if (perforation != null)
            {
                if (DateTime.Today >= perforation.DateDebut && DateTime.Today <= perforation.DateFin)
                    perforationRead = false;
            }
            bool perforation2Read = true;
            var perforation2 = periodes.FirstOrDefault(x => x.Name.ToLower() == "perforation2");
            if (perforation2 != null)
            {
                if (DateTime.Today >= perforation2.DateDebut && DateTime.Today <= perforation2.DateFin)
                    perforation2Read = false;
            }

            foreach (ParcelleModel parcelle in parcelles)
            {
                try
                {
                    Engagement_cadre engagements_cadre = new Engagement_cadre();
                    engagements_cadre.v_person_typ = "ADHERENT";
                    engagements_cadre.v_num_engagement_cadre = $"{annee}_{parcelle.id_parcelle}";
                    engagements_cadre.v_code_campagne = $"{annee}";
                    engagements_cadre.i_person_num = parcelle.id_propriete;
                    engagements_cadre.v_code_activite = "PARCELLE";
                    engagements_cadre.complements = new ComplementsEngagements_cadre();
                    var complement = engagements_cadre.complements;

                    complement.vers_grappe_authorize = parcelle.inCampagne ? "oui" : null;
                    double doubleTmp;
                  
                    complement.nom_parcelle = parcelle.nameParcelle2;
                    complement.nom_prestataire = parcelle.prestataire;
                    complement.num_parcelle = parcelle.ut;
                    complement.cepage = parcelle.cepage;
                    complement.qualite = parcelle.qualite;
                    if (double.TryParse(parcelle.surface.ToString(), out doubleTmp))
                        complement.surface = doubleTmp;
                    if (double.TryParse(parcelle.superficie_vendangee.ToString(), out doubleTmp))
                        complement.surface_vendangee = doubleTmp;
                    complement.appellation = parcelle.appellation;
                    //complement.glomerule_readonly = glomeruleRead?"oui":null;
                    complement.perforation_1_readonly = perforationRead ? "oui" : null;
                    complement.perforation_2_readonly = perforation2Read ? "oui" : null;
                    complement.glomerules_readonly = glomeruleRead ? "oui" : null;
                    engagements.engagement_cadre.Add(engagements_cadre);
                }
                catch(Exception ex)
                {
                    Gestion.Erreur($"Parcelle : {parcelle.id_parcelle} - {parcelle.ut} / {ex.Message} ");
                }  
            }
            return engagements;

        }

        private Engagements mappingParcellesToEngagement(List<ParcelleModel> parcelles, int annee)
        {
            var engagements = new Engagements();
            engagements.engagements = new List<EngagementModel>();
            
            foreach (ParcelleModel parcelle in parcelles)
            {
                try
                {
                    EngagementModel engagement = new EngagementModel();
                    engagement.v_person_typ = "ADHERENT";
                    engagement.v_num_engagement_cadre = $"{parcelle.id_propriete}__{annee}_MODEL_VERS_DE_GRAPPE_{annee}_{parcelle.id_parcelle}";
                    engagement.v_code_campagne = $"{annee}";
                    engagement.i_person_num = parcelle.id_propriete;
                    engagement.v_code_activite = "PARCELLE";
                    engagement.v_code_commercialisation = "VERS_DE_GRAPPE";
                    engagement.complements = new Complements();
                    var complement = engagement.complements;

                    complement.statut_inscription = "OUVERTE";
                    complement.cont_date = $"{DateAndTime.Today.ToString("yyyy-MM-dd")}";
                    complement.v_code_modele = "MODELE_VERS_DE_GRAPPE";
                    complement.i_person_num = parcelle.id_propriete.ToString();
                    complement.v_person_typ = "ADHERENT";
                }
                catch (Exception ex)
                {
                    Gestion.Erreur($"Parcelle : {parcelle.id_parcelle} - {parcelle.ut} / {ex.Message} ");
                }
            }
            return engagements;

        }

    }


        public class ReponseApi
    {
        public bool success;
        public Type reponseType;

        public object result;

        
        public ReponseApi(object result, bool success = false)
        {
            this.success = success;
            this.reponseType =  result.GetType();
            this.result = result;
        }
    }
}
