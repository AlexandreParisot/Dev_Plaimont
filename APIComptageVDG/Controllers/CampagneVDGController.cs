using APIComptageVDG.ControllerModel;
using APIComptageVDG.Helpers;
using APIComptageVDG.Models;
using APIComptageVDG.Models.JsonModel;
using APIComptageVDG.Provider;
using APIComptageVDG.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text.Json;

namespace APIComptageVDG.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CampagneVDGController : Controller
    {
        private InstagrappeApiProvider _provider;
        private LavilogService _service = new LavilogService();

        private IConfiguration _config;
        public CampagneVDGController(IConfiguration config)
        {
            _config = config;
            if (!string.IsNullOrEmpty(_config["SqlConnexion"]))
                _service.SetConnexion(_config["SqlConnexion"]!);
        }


        #region Instagrappe

        /// <summary>
        /// retour le token de l'api Instagrappe
        /// </summary>
        /// <returns></returns>
        [HttpGet("Instagrappe")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<String>> GetTokenInstagrappe()
        {
            var url_token = _config["Instagrappe:URL_Token"];
            var url_api = _config["Instagrappe:URL_API"];
            var user = _config["Instagrappe:ID"];
            var pwd = _config["Instagrappe:MDP"];
            if (!string.IsNullOrEmpty(url_token) && !string.IsNullOrEmpty(url_api) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
                var client = new InstagrappeApiProvider(url_token, url_api, user, pwd);

                var token = await client.GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    Gestion.Info($"Recuperation Token : {token}");
                    return Ok(token);
                }
                return BadRequest("Erreur d'authentification.");
            }
            else
            {
                Gestion.Erreur($"Il manque des informations dans Appsetting.json pour contacter l'api Instagrappe. ");
                return BadRequest("L'api n'a pas les informations néssecaire pour contatacter l'api Instagrappe.");
            }

        }

        /// <summary>
        ///  Retourne les engagnements 
        /// </summary>
        /// <param name="engagement"></param>
        /// <param name="AnneeCampagne"></param>
        /// <param name="code_modele"></param>
        /// <returns></returns>
        [HttpGet("Instagrappe/Engagement")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetEngagementInstagrappe(string code_activite, string AnneeCampagne)
        {

            if (string.IsNullOrEmpty(code_activite))
                return BadRequest("code modele est vide.");

            int annee = 0;
            if (!int.TryParse(AnneeCampagne, out annee))
                return BadRequest("L'année de la campagne n'est pas correct.");

            var cCommandeGet = string.Empty;
            cCommandeGet += "code_activite=" + code_activite;
            cCommandeGet += "&code_campagne=" + annee;
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
                        if (Engagement == null || Engagement.engagements?.Count > 0)
                            return Ok(Engagement);
                        else
                            return Ok(str);
                    }
                    else
                        return BadRequest(str);
                }
                else
                    return BadRequest(await result.Content.ReadAsStringAsync());
            }
            else
                return BadRequest("Probléme de connexion a l'api Instagrappe.");


        }


        /// <summary>
        ///  Envoi a instagrappe le fichier json des engagement cadre
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        [HttpPost("Instagrappe/Import")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> SetImportInstagrappe(string Path)
        {

            if (!System.IO.File.Exists(Path))
                return BadRequest("le fichier n'existe pas.");

            var hostSftp = _config["Instagrappe:ServeurSFTP"];
            var userSftp = _config["Instagrappe:IdSftp"];
            var pathSftp = "import";
            var pwdSftp = _config["Instagrappe:MdpSftp"];
            sFtpHelper.SetInstance(hostSftp, userSftp, pwdSftp, pathSftp);

           if( await sFtpHelper.AsyncUploadFile(Path))
            {
                var cCommandeGet = string.Empty;
                cCommandeGet += "file=" + System.IO.Path.GetFileName(Path);

                if (await getToken() is string token && !string.IsNullOrEmpty(token))
                {
                    var result = await _provider.CallApi($"import/apport", HttpMethod.Post, entetedata: cCommandeGet);
                    if (result.IsSuccessStatusCode)
                    {
                        var str = await result.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(str))                       
                            return Ok(str);                       
                        else
                            return BadRequest(str);
                    }
                    else
                    {
                        var erreur = await result.Content.ReadAsStringAsync();
                        Gestion.Erreur(erreur);
                        return BadRequest(erreur);
                    }
                      
                }
                else
                    return BadRequest("Probléme de connexion à l'api Instagrappe.");
            }

            return BadRequest("Impossible d'envoyer le fichier sur le serveur Sftp.");

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="AnneeCampagne"></param>
        /// <returns></returns>
        [HttpGet("Instagrappe/EngagementVersDeGrappe")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetEngagementVersDeGrappe(string AnneeCampagne)
        {

            //if (string.IsNullOrEmpty(code_activite))
            //    return BadRequest("code modele est vide.");

            int annee = 0;
            if (!int.TryParse(AnneeCampagne, out annee))
                return BadRequest("L'année de la campagne n'est pas correct.");

            var cCommandeGet = string.Empty;
            cCommandeGet += "code_activite=PARCELLE";// + code_activite;
            cCommandeGet += "&code_campagne=" + annee;
            cCommandeGet += "&indicateur_modele=v_code_modele";
            cCommandeGet += "&indicateur_contrat=statut_inscription";
            cCommandeGet += "&valeur_contrat=OUVERTE";


            if (await getToken() is string token && !string.IsNullOrEmpty(token))
            {
                var result = await _provider.CallApi("export/engagement", HttpMethod.Get,entetedata: cCommandeGet);
                if (result.IsSuccessStatusCode)
                {
                    var str = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var Engagement = JsonSerialize.Deserialize<Engagements>(str);
                        if (Engagement == null || Engagement.engagements?.Count > 0)
                            return Ok(Engagement);
                        else
                            return Ok(str);
                    }
                    else
                        return BadRequest(str);
                }
                else
                    return BadRequest(await result.Content.ReadAsStringAsync());
            }
            else
                return BadRequest("Probléme de connexion a l'api Instagrappe.");


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="AnneeCampagne"></param>
        /// <returns></returns>
        [HttpGet("Instagrappe/EngagementCadreVersDeGrappe")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetEngagementCadreVersDeGrappe(string AnneeCampagne)
        {

            //if (string.IsNullOrEmpty(code_activite))
            //    return BadRequest("code modele est vide.");

            int annee = 0;
            if (!int.TryParse(AnneeCampagne, out annee))
                return BadRequest("L'année de la campagne n'est pas correct.");



            var cCommandeGet = string.Empty;
            cCommandeGet += "code_activite=PARCELLE";
            cCommandeGet += "&code_campagne=" + annee;
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
                        if (EngagementCadre == null || EngagementCadre.engagements_cadre?.Count > 0)
                        {
                            var upsert = new IncrementalModel();
                            upsert.upsert = new Upsert() { Obj = EngagementCadre };
                            var StrUpsert = JsonSerialize.Serialize(upsert);

                            return Ok(EngagementCadre);
                        }

                        return Ok(str);
                    }
                    else
                        return BadRequest(str);
                }
                else
                    return BadRequest(await result.Content.ReadAsStringAsync());
            }
            else
                return BadRequest("Probléme de connexion a l'api Instagrappe.");

        }


        /// <summary>
        ///  Retourne les engagnements 
        /// </summary>
        /// <param name="engagement"></param>
        /// <param name="AnneeCampagne"></param>
        /// <param name="code_modele"></param>
        /// <returns></returns>
        [HttpGet("Instagrappe/EngagementCadre")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetEngagementCadreInstagrappe(string code_activite, string AnneeCampagne)
        {

            if (string.IsNullOrEmpty(code_activite))
                return BadRequest("code modele est vide.");

            int annee = 0;
            if (!int.TryParse(AnneeCampagne, out annee))
                return BadRequest("L'année de la campagne n'est pas correct.");



            var cCommandeGet = string.Empty;
            cCommandeGet += "code_activite=" + code_activite;
            cCommandeGet += "&code_campagne=" + annee;
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
                        if (EngagementCadre == null || EngagementCadre.engagements_cadre?.Count > 0)
                        {
                            var upsert = new IncrementalModel();
                            upsert.upsert = new Upsert() { Obj = EngagementCadre };
                            var StrUpsert = JsonSerialize.Serialize(upsert);

                            return Ok(EngagementCadre);
                        }

                        return Ok(str);
                    }
                    else
                        return BadRequest(str);
                }
                else
                    return BadRequest(await result.Content.ReadAsStringAsync());
            }
            else
                return BadRequest("Probléme de connexion a l'api Instagrappe.");


        }

        /// <summary>
        ///  Retourne les engagnements 
        /// </summary>
        /// <param name="engagement"></param>
        /// <param name="AnneeCampagne"></param>
        /// <param name="code_modele"></param>
        /// <returns></returns>
        [HttpGet("Instagrappe/export/cgu")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> cgu()
        {
            if (await getToken() is string token && !string.IsNullOrEmpty(token))
            {
                var result = await _provider.CallApi("export/cgu", HttpMethod.Get);
                if (result.IsSuccessStatusCode)
                {
                    var str = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var cgus = JsonSerialize.Deserialize<GgusModel>(str);
                        return Ok(cgus);
                    }

                    else
                        return BadRequest(str);
                }
                else
                    return BadRequest(await result.Content.ReadAsStringAsync());
            }
            else
                return BadRequest("Probléme de connexion a l'api Instagrappe.");

        }


        [HttpPost("Instagrappe/Config")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Config()
        {
            if (await getToken() is string token && !string.IsNullOrEmpty(token))
            {
                return Ok(_provider.CallApi("config", HttpMethod.Post));
            }
            else
                return BadRequest("Probléme de connexion a l'api Instagrappe.");

        }


        #region Private Methode 

        /// <summary>
        /// retourne le token instagrappe
        /// </summary>
        /// <returns></returns>
        private async Task<string> getToken()
        {
            var url_token = _config["Instagrappe:URL_Token"];
            var url_api = _config["Instagrappe:URL_API"];
            var user = _config["Instagrappe:ID"];
            var pwd = _config["Instagrappe:MDP"];
            if (!string.IsNullOrEmpty(url_token) && !string.IsNullOrEmpty(url_api) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
                _provider = new InstagrappeApiProvider(url_token, url_api, user, pwd);

                var token = await _provider.GetToken();
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

        #endregion
        #endregion

        #region Lavilog


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Lavilog/GetParcelles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ParcelleModel>>> GetParcelles(int year)
        {

            if (!_service.IsConnected)
                return BadRequest("Aucune Connexion au serveur de base");
            var result = await _service.AsyncGetParcelles( year);
            if(result != null)
                return Ok(result);
            return BadRequest("Une erreur s'est produite.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet("Lavilog/GetParcellesCampagne")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ParcelleModel>>> GetParcellesInCampagne(int year)
        {

            if (!_service.IsConnected)
                return BadRequest("Aucune Connexion au serveur de base");
            var result = await _service.AsyncGetParcellesInCampagne(year);
            return Ok(result);
        }
    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="parcelles"></param>
        /// <returns></returns>
        [HttpPost("Lavilog/SetParcellesCampagne")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<int>>> SetParcellesCampagne(int year, IEnumerable<ParcelleModel> parcelles)
        {

            if (!_service.IsConnected)
                return BadRequest("Aucune Connexion au serveur de base");

            var result = await _service.AsyncSetParcellesCampagne(parcelles, year);
            if(result!= null && result.Count == parcelles.Count())
                return Ok(result);

            return BadRequest(result);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Lavilog/GetCampagnes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Dictionary<int, string>>> GetCampagnes()
        {
           
            if (!_service.IsConnected)
                return BadRequest("Aucune Connexion au serveur de base");          

            return Ok(await _service.AsyncGetCampagnes());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpGet("Lavilog/GetPeriode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<PeriodeModel>>> GetPeriode(int year)
        {

            if (!_service.IsConnected)
                return BadRequest("Aucune Connexion au serveur de base");

            return Ok(await _service.AsyncGetPeriode(year.ToString()));
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="periodes"></param>
       /// <returns></returns>
        [HttpPost("Lavilog/SetPeriodes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<int>>> SetPeriodes(List<PeriodeModel> periodes)
        {

            if (!_service.IsConnected)
                return BadRequest("Aucune Connexion au serveur de base");
            var enregistement = await _service.AsyncSetPeriodes(periodes);
            if (enregistement.Count != periodes.Count)
                return BadRequest("Une erreur s'est produite."); 
            else
                return Ok(enregistement);
        }

        [HttpGet("Lavilog/GetLastSynchro")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GetLastSynchro()
        {

            if (!_service.IsConnected)
                return BadRequest("Aucune Connexion au serveur de base");

                return Ok(await _service.AsyncGetLastSynchro());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost("Lavilog/SetLastSynchro")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> SetLastSynchro()
        {

            if (!_service.IsConnected)
                return BadRequest("Aucune Connexion au serveur de base");
            
            if ( !await _service.AsyncSetLastSynchro())
                return BadRequest(false);
            else
                return Ok(true);
        }


        #endregion
    }

}
