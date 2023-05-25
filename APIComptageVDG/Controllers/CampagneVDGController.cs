using APIComptageVDG.ControllerModel;
using APIComptageVDG.Helpers;
using APIComptageVDG.Models;
using APIComptageVDG.Models.JsonModel;
using APIComptageVDG.Provider;
using APIComptageVDG.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
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
        private InstagrappeServcie _instaService;
        private LavilogService _service;

        private IConfiguration _config;
        public CampagneVDGController(IConfiguration config)
        {
            _config = config;
            //_service = service;
            if (_service == null)
                _service = new LavilogService();

            if (!string.IsNullOrEmpty(_config.GetConnectionString("SqlConnexion")))            
                _service.SetConnexion(_config.GetConnectionString("SqlConnexion")!);
            

            var url_token = _config["Instagrappe:URL_Token"];
            var url_api = _config["Instagrappe:URL_API"];
            var user = _config["Instagrappe:ID"];
            var pwd = _config["Instagrappe:MDP"];

            if(_instaService == null)
                _instaService = new InstagrappeServcie();

            if (!string.IsNullOrEmpty(url_token) && !string.IsNullOrEmpty(url_api) && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
                _provider = new InstagrappeApiProvider(url_token, url_api, user, pwd);
                _instaService.SetApiProviderInstagrappe(_provider);
            }
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
            if (_instaService?.SuccessConfig ==true)
            {
                var token = await _instaService.getToken();
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


            if (_instaService.SuccessConfig )
            {
                var result = await _instaService.GetEngagementInstagrappe(code_activite,annee);
                return result.success ? Ok(result.result) : BadRequest(result.result);
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
            {
                Gestion.Erreur($"le fichier {Path} n'existe pas.");
                return BadRequest(false);
            }

            var hostSftp = _config["Instagrappe:ServeurSFTP"];
            var userSftp = _config["Instagrappe:IdSftp"];
            var pathSftp = "import";
            var pwdSftp = _config["Instagrappe:MdpSftp"];
            sFtpHelper.SetInstance(hostSftp, userSftp, pwdSftp, pathSftp);

           if( await sFtpHelper.AsyncUploadFile(Path))
            {
                if (_instaService.SuccessConfig)
                {

                    var result = await _instaService.SetImportInstagrappe(Path);
                    try
                    {
                        //archive le fichier json
                        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Gestion.Current_Dir, "API", "Reussi"));
                        System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Gestion.Current_Dir, "API", "Echec"));
                        if (result.success)
                        {
                            System.IO.File.Copy(Path, System.IO.Path.Combine(Gestion.Current_Dir, "API", "Reussi", System.IO.Path.GetFileName(Path)), true);
                            await _service.AsyncSetLastSynchro();
                        }
                        else
                            System.IO.File.Copy(Path, System.IO.Path.Combine(Gestion.Current_Dir, "API", "Echec", System.IO.Path.GetFileName(Path)), true);

                        System.IO.File.Delete(Path);
                    }
                    catch (Exception ex)
                    {
                        Gestion.Erreur($"Erreur Archivage fichier : {System.IO.Path.GetFileName(Path)} - {ex.Message}");
                    }
                    if (result.success)
                        Gestion.Obligatoire(result.result.ToString());
                    else
                        Gestion.Erreur(result.result.ToString());

                    return result.success ? Ok(true) : BadRequest(false);
                }
                else
                    Gestion.Erreur("Il manque des informaitons pour le parametrage de l'appel de l'api instagrappe.");
                    return BadRequest(false);
            }
            Gestion.Erreur("Impossible d'envoyer le fichier sur le serveur Sftp.");
            return BadRequest(false);

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

            int annee = 0;
            if (!int.TryParse(AnneeCampagne, out annee))
                return BadRequest("L'année de la campagne n'est pas correct.");

            if (_instaService.SuccessConfig)
            {
                var result = await _instaService.GetEngagementVersDeGrappe(annee);
                return result.success ? Ok(result?.result) : BadRequest(result?.result);

            }
            else
                return BadRequest("Il manque des informaitons pour le parametrage de l'appel de l'api instagrappe.");


        }


       
        /// <summary>
        /// Génére le fichier engagement cadre pour le vers de grappe.
        /// constitu le fichier json.
        /// </summary>
        /// <returns>retourne le nom du fichier json a lancer a l'import. sinon retourne rien.</returns>
        [HttpGet("Instagrappe/GenerateEngagementCardeVDG")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> GenerateEngagementCadreVDGInstagrappe()
        {
            string fileJson = string.Empty;

            var listParcelle = await _service.AsyncGetParcellesInCampagne(int.Parse(DateTime.Today.ToString("yyyy")));
            var listPeriode = await _service.AsyncGetPeriode(DateTime.Today.ToString("yyyy"));

            var result = await _instaService.GenerateEngagementCadreVDGInstagrappe(listParcelle, listPeriode, int.Parse(DateTime.Today.ToString("yyyy"))) ; 
            return result.success ? Ok(result?.result) : BadRequest(result?.result);    
            

        }

        /// <summary>
        /// Génére le fichier engagement pour le vers de grappe.
        /// constitu le fichier json.
        /// </summary>
        /// <returns>retourne le nom du fichier json a lancer a l'import. sinon retourne rien.</returns>
        //[HttpGet("Instagrappe/GenerateEngagementVDG")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<ActionResult<string>> GenerateEngagementVDGInstagrappe()
        //{
        //    string fileJson = string.Empty;

        //    var listParcelle = await _service.AsyncGetParcellesInCampagne(int.Parse(DateTime.Today.ToString("yyyy")));
          
        //    var result = await _instaService.GenerateEngagementVDGInstagrappe(listParcelle, int.Parse(DateTime.Today.ToString("yyyy")));
        //    return result.success ? Ok(result?.result) : BadRequest(result?.result);


        //}



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

            if(_instaService.SuccessConfig)
            {
               var result = await _instaService.GetEngagementCadreInstagrappe(code_activite, annee);
               return result.success ? Ok(result.result) : BadRequest(result.result);
            }
            else
              return BadRequest("Il manque des informaitons pour le parametrage de l'appel de l'api instagrappe.");

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
            int annee = 0;
            if (!int.TryParse(AnneeCampagne, out annee))
                return BadRequest("L'année de la campagne n'est pas correct.");

            if (_instaService.SuccessConfig)
            {
                var result = await _instaService.GetEngagementCadreVersDeGrappe(annee);
                return result.success? Ok(result?.result) : BadRequest(result?.result);  

            }else
                return BadRequest("Il manque des informaitons pour le parametrage de l'appel de l'api instagrappe.");
        }



        /// <summary>
        ///  permet d'envoyer la synchro des parcelle dans instagrappe pour l'ouverture au comptage.
        /// </summary>
        /// <param name="AnneeCampagne"></param>
        /// <returns></returns>
        [HttpGet("Instagrappe/SynchroDeclaration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> GetSynchroDeclaration(int year)
        {

            if (_instaService.SuccessConfig)
            {
                var result = await GenerateEngagementCadreVDGInstagrappe();
                var strjsonFile = ((ObjectResult)result.Result).Value;
                if (string.IsNullOrEmpty(strjsonFile.ToString()))
                {
                    Gestion.Erreur("Impossible de generer le fichier Engagement Cadre Vers de grappe.");
                    return Ok(false);
                }

                Gestion.Info($"Creation du fichier {strjsonFile.ToString()} éffectué.");
                var result2 =  await SetImportInstagrappe(strjsonFile.ToString());
                var boolResult = ((ObjectResult)result2.Result).Value;
                Gestion.Info($"Envoi instagrappe : {result2.ToString()}");
                return Ok(boolResult);
            }
            else
                Gestion.Erreur("Il manque des informaitons pour le parametrage de l'appel de l'api instagrappe.");
            return BadRequest(false); ;
        }

        /// <summary>
        ///  permet d'envoyer la synchro des parcelle dans instagrappe pour l'ouverture au comptage.
        /// </summary>
        /// <param name="AnneeCampagne"></param>
        /// <returns></returns>
        [HttpGet("Instagrappe/SynchroCompteur")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<bool>> GetSynchroCompteur(int year)
        {

            if (_instaService.SuccessConfig)
            {
                var result = await GetEngagementVersDeGrappe(year.ToString());
                if(((ObjectResult)result.Result).Value is Engagements Engagement)
                {
                    if (Engagement != null)
                       return Ok(await _service.AsyncSetCptParcellesCampagne(Engagement));                    
                    else
                    {
                        Gestion.Erreur($"Erreur dans la recuperation de l'engagement.");
                        return Ok(false);
                    }
                }
                else
                {
                    Gestion.Erreur(((ObjectResult)result.Result).Value.ToString());
                    return Ok(false);
                }
                
                
            }
            else
                Gestion.Erreur("Il manque des informaitons pour le parametrage de l'appel de l'api instagrappe.");
            return BadRequest(false); ;
        }


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
