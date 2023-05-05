using APIComptageVDG.ControllerModel;
using APIComptageVDG.Helpers;
using APIComptageVDG.Interfaces;
using APIComptageVDG.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.PortableExecutable;

namespace APIComptageVDG.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PiloteController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private PiloteControllerModel _piloteControllerModel;
        private string pathSaveMachines = String.Empty;
        private string pathPilotes = String.Empty;
        

        public PiloteController(IConfiguration configuration)
        {
            _configuration = configuration;
            pathPilotes = _configuration["pathPilotes"];
            pathSaveMachines = _configuration["pathSaveMachines"];  
            _piloteControllerModel = new PiloteControllerModel(new ServicesHelper(), pathSaveMachines);

            Gestion.Info($"Fichier config - Repértoire pilotes : {pathPilotes}");
            Gestion.Info($"Fichier config - Fichier sauvegarde des machines : {pathSaveMachines}");
        }

        #region "Methodes Controller"


        [HttpPost("start/{machineName}")]
        public IActionResult StartService(string machineName)
        {
            Gestion.Info($"Machine : {machineName}");
            if (!_piloteControllerModel.ExistMachine(machineName))
            {
                Gestion.Warning($"Aucun service trouvé pour cette machine {machineName}");
                return BadRequest(_piloteControllerModel.StatusServiceMachine(machineName));
            }

            if (_piloteControllerModel.StartService(machineName))
            {
                Gestion.Info($"Start Status : {_piloteControllerModel.StatusService}");
                return Ok(_piloteControllerModel.StatusServiceMachine(machineName));
            }
            else
            {
                Gestion.Warning($"Start Status : {_piloteControllerModel.StatusService} - Machine : {machineName}");
                return NotFound(_piloteControllerModel.StatusServiceMachine(machineName));
            }
                
        }

        [HttpPost("stop/{machineName}")]
        public IActionResult StopService(string machineName)
        {
            Gestion.Info($"Machine : {machineName}");
            if (!_piloteControllerModel.ExistMachine(machineName))
            {
                Gestion.Warning($"Aucun service trouvé pour cette machine {machineName}");
                return BadRequest(_piloteControllerModel.StatusServiceMachine(machineName));
            }

            if (_piloteControllerModel.StopService(machineName))
            {
                Gestion.Info($"Stop Status : {_piloteControllerModel.StatusService}");
                return Ok(_piloteControllerModel.StatusServiceMachine(machineName));
            }
            else
            {
                Gestion.Warning($"Stop Status : {_piloteControllerModel.StatusService} - Machine : {machineName}");
                return NotFound(_piloteControllerModel.StatusServiceMachine(machineName));
            }
               
        }


        [HttpPost("restart/{machineName}")]
        public IActionResult RestartService(string machineName)
        {
            Gestion.Info($"Machine : {machineName}");
            if (!_piloteControllerModel.ExistMachine(machineName))
            {
                Gestion.Warning($"Aucun service trouvé pour cette machine {machineName}");
                return BadRequest(_piloteControllerModel.StatusServiceMachine(machineName));
            }
                
            
            if (_piloteControllerModel.RestartService(machineName))
            {
                Gestion.Info($"Restart Status : {_piloteControllerModel.StatusService}");
                return Ok(_piloteControllerModel.StatusServiceMachine(machineName));
            }
            else
            {
                Gestion.Warning($"Restart Status : {_piloteControllerModel.StatusService} - Machine : {machineName}");
                return NotFound(_piloteControllerModel.StatusServiceMachine(machineName));
            }
                
        }

        [HttpPost("refresh")]
        public IActionResult Refresh()
        {
            Gestion.Info($"Démarrage du refresh du dossier des pilotes : {pathPilotes}");
            if (_piloteControllerModel.RefreshPilotes(pathPilotes))
            {
                Gestion.Info($"Nombre de pilotes trouvé : {_piloteControllerModel.Count}");
                return Ok(_piloteControllerModel.GetJsonMachines());
            }
            else
            {
                Gestion.Warning($"Le dossier des pilotes n'est pas defini dans le fichier de configuration.");
                return BadRequest($"Le dossier des pilotes n'est pas defini dans le fichier de configuration.");
            }
                             

        }

        [HttpPost("add/{path}")]
        public IActionResult add(string path)
        {
            Gestion.Info($"Ajout pilote : {path}");
            if (_piloteControllerModel.AddPilote(path))
            {
                Gestion.Info($"Pilote ajouté : {_piloteControllerModel.getJsonLastMachine()}");
                return Ok(_piloteControllerModel.getJsonLastMachine());
            }
            else
            {
                Gestion.Warning($"Impossible d'ajouté le Pilote. Il n'y a pas de pilote sur le serveur pour ce chemin avec un service windows.");
                return BadRequest($"il n'y a pas de pilote sur le serveur pour ce chemin avec un service windows.");
            }
                
        }

        [HttpGet("machines")]
        public IActionResult machines()
        {
            Gestion.Info($"Envoi liste des pilotes. nombres de pilotes : {_piloteControllerModel.Count}");
            return Ok(_piloteControllerModel.GetJsonMachines());           
        }

        [HttpGet("status/{machine}")]
        public IActionResult status(string machine)
        {
            var etat = _piloteControllerModel.StatusServiceMachine(machine);
            Gestion.Info($"Etat demander pour la machine : {machine} status : {_piloteControllerModel.StatusService}");
            return Ok(etat);
        }

        #endregion


    }
}
