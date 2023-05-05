using APIComptageVDG.Helpers;
using APIComptageVDG.Interfaces;
using APIComptageVDG.Models;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace APIComptageVDG.ControllerModel
{
    public class PiloteControllerModel
    {
        private IGestionService gestionService;
        private List<ServiceMachinesModel> _listeMachines;
        private string pathSaveMachines = String.Empty;        

        public PiloteControllerModel(IGestionService gestionService,string pathSaveMachines)
        {
            this.gestionService = gestionService;
            if (!string.IsNullOrEmpty(pathSaveMachines))
                this.pathSaveMachines = pathSaveMachines;
            else
                this.pathSaveMachines = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath)!, "ServiceMachines.json");

            _listeMachines = JsonSerialize.DeserializeFromFile<List<ServiceMachinesModel>>(this.pathSaveMachines);
        }

        /// <summary>
        /// return l'etat du service en cours de test.
        /// </summary>
        public virtual string StatusService { get => gestionService.Status; }

        /// <summary>
        /// retourne le nombre de service Monitoré par l'API.
        /// </summary>
        public int Count { get=> _listeMachines.Count; }

        #region "Methode Public"

        /// <summary>
        /// redemarre le service en relation avec le nom de la machine dans integraal
        /// </summary>
        /// <param name="machineName"> nom de la machine dans integraal</param>
        /// <returns> retourne le booleen si  ça c'est bien passé</returns>
        public bool StartService(string machineName)
        {
            var machService = getServiceMachines(machineName);
            if (machService == null)
                return false;
            return gestionService.StartService(machService.NameService);
        }

        /// <summary>
        /// stop le service en relation avec le nom de la machine dans integraal
        /// </summary>
        /// <param name="machineName">nom de la machine dans integraal</param>
        /// <returns> retourne le booleen si  ça c'est bien passé</returns>
        public bool StopService(string machineName)
        {
            var machService = getServiceMachines(machineName);
            if (machService == null)
                return false;

            return gestionService.StopService(machService.NameService);
        }

        /// <summary>
        /// redémare le service en relation avec le nom de la machine dans integraal
        /// </summary>
        /// <param name="machineName">nom de la machine dans integraal</param>
        /// <returns> retourne le booleen si  ça c'est bien passé</returns>
        public bool RestartService(string machineName)
        {
            var machService = getServiceMachines(machineName);
            if (machService == null)
                return false;

           return gestionService.RestartService(machService.NameService);
        }

        /// <summary>
        /// retourne si la machine integraal est monitoré par l'API
        /// </summary>
        /// <param name="machineName">nom de la machine dans integraal</param>
        /// <returns>retourne true ou false</returns>
        public bool ExistMachine(string machineName)
        {
            try
            {
                var machService = getServiceMachines(machineName);
                if (machService == null)
                    return false;

                return true;
            }
            catch(Exception ex)
            {
                Gestion.Erreur($"machineName : {machineName} - {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// rafraichi la liste des services monitorer a partir d'un chemin d'un répertoire sur le serveur.
        /// </summary>
        /// <param name="path">chemin d'un répertoire </param>
        /// <returns>retourne true ou false</returns>
        public bool RefreshPilotes(string path)
        {
            try
            {
                if ((!string.IsNullOrEmpty(path) && Directory.Exists(Path.GetDirectoryName(path))))
                {
                    refreshListeMachines(path);
                    JsonSerialize.SerializeToFile(pathSaveMachines, _listeMachines);
                    return true;
                }
                else
                    return false;
            }
            catch(Exception ex)
            {
                Gestion.Erreur($"path : {path} - {ex.Message}");
                return false;
            }
        }

       /// <summary>
       /// Ajoute le pilote dans la liste des pilotes controller par l'API
       /// </summary>
       /// <param name="path"> chemin de l'executable du pilote</param>
       /// <returns> true si le pilote est ajouté ou false sinon</returns>

        public bool AddPilote(string path)
        {
            try
            {
                if ((!string.IsNullOrEmpty(path) && Directory.Exists(Path.GetDirectoryName(path))))
                {
                    var count = _listeMachines.Count();
                    var retour = AddOnePilote(path);
                    
                    if (_listeMachines.Count() > count || retour)
                    {
                        JsonSerialize.SerializeToFile(pathSaveMachines, _listeMachines);
                        return true;

                    }                
                }
                return false;
            }
            catch(Exception ex) 
            {
                Gestion.Erreur($"path : {path} - {ex.Message}");
                return false;
            }
        }

        public ServiceMachinesModel getJsonLastMachine()
        {
            return _listeMachines.Last();
        }

        public List<ServiceMachinesModel> GetJsonMachines()
        {
           return _listeMachines;
        }

        public StatusServiceMachineModel StatusServiceMachine(string machine)
        {
            var machService = getServiceMachines(machine);
            StatusServiceMachineModel statusServiceMachineModel = new StatusServiceMachineModel();
            if (machService == null)
            {
                statusServiceMachineModel.Message=  "La machine n'est pas prise en compte dans l'API.";
                statusServiceMachineModel.StatusService = string.Empty;
                statusServiceMachineModel.MachineName = machine;
                return statusServiceMachineModel;

            }

            statusServiceMachineModel.Message= gestionService.GetServiceStatus(machService.NameService);
            statusServiceMachineModel.StatusService = StatusService;
            statusServiceMachineModel.MachineName = machine;

            return statusServiceMachineModel;
        }



        #endregion



        #region "Methode Private"

        public virtual ServiceMachinesModel? getServiceMachines(string machineName)
        {
            if (string.IsNullOrEmpty(machineName))
                return null;

            if (_listeMachines == null)
                return null;

            return _listeMachines.FirstOrDefault(x => x.GetMachines().ToUpper().Contains(machineName));

        }


        public virtual void refreshListeMachines(string pathPilotes)
        {
            
            if (!Directory.Exists(pathPilotes))
                return;

            try
            {
                AddOnePilote(pathPilotes);

                foreach (var directory in Directory.GetDirectories(pathPilotes))
                {
                    refreshListeMachines(directory);
                }
            }
            catch(Exception ex)
            {
                Gestion.Erreur($"pathPilotes: {pathPilotes} - {ex.Message}");
            }

           

        }


        public virtual bool AddOnePilote(string path)
        {
            if (!Directory.Exists(path))
                return false;

            try
            {
                //Recherche fichier ini
                var fileIni = getLastFile(path, "*.ini");

                if (fileIni == null)
                    return false;

                //lecture du fichier pour trouver les machines 
                var lstMach = getMachinesFromIni(fileIni.FullName);

                //recuperation du fichier du pilote

                var fileExe = getLastFile(path, "*.exe", true);

                if (fileExe == null)
                    return false;
                //voir si il a un service associé
                var NameService = gestionService.GetServiceName(fileExe.FullName);
                if (string.IsNullOrEmpty(NameService))
                    return false;

                //creer l'objet ServiceMachines

                var pilote = new ServiceMachinesModel()
                {
                    NameService = NameService,
                    ExecutablePathPilote = fileExe.FullName,
                    Machines = lstMach
                };

                // verifie si on a pas deja le pilote dans la liste 

                var piloteExiste = _listeMachines.FirstOrDefault((x) => x.NameService == pilote.NameService);
                // supprime dans la liste si il existe deja.
                if (piloteExiste != null)
                    _listeMachines.Remove(piloteExiste);
                // l'ajouter a la liste.

                _listeMachines.Add(pilote);
                return true;
            }
            catch(Exception ex)
            {
                Gestion.Erreur($"path : {path} - {ex.Message}");
                return false;
            }
            
        }

        private FileInfo? getLastFile(string pathPilote, string patern, bool searchService = false)
        {
            if (!Directory.Exists(pathPilote))
                return null;

            try
            {
                var listFiles = Directory.GetFiles(pathPilote, patern);

                if (listFiles.Count() == 1)
                {
                    return new FileInfo(listFiles.First());
                }
                else if (listFiles.Any())
                {

                    FileInfo? lastFile = null;
                    foreach (var item in listFiles)
                    {
                        var fileInfo = new FileInfo(item);

                        if (searchService)
                        {
                            if (!string.IsNullOrEmpty(gestionService.GetServiceName(fileInfo.FullName)))
                            {
                                return fileInfo;
                            }
                        }

                        if (lastFile == null)
                        {
                            lastFile = fileInfo;
                        }
                        else if (fileInfo.LastWriteTime > lastFile.LastWriteTime)
                        {
                            lastFile = fileInfo;
                        }
                    }

                    return lastFile;
                }

                return null;
            }
            catch(Exception ex)
            {
                Gestion.Erreur($"Path : {pathPilote} - patern : {patern} - searchService : {searchService}  - {ex.Message}");
                return null;
            }

            
        }


        private List<string> getMachinesFromIni(string file)
        {
            if (string.IsNullOrEmpty(file))
                return new List<string>();

            try
            {
                var mach = string.Empty;

                var lignes = System.IO.File.ReadAllText(file);

                foreach (var line in lignes.Split(Environment.NewLine))
                {
                    var ligne = line.Trim();
                    if (ligne.ToUpper().Contains("MACHINE="))
                    {
                        mach = ligne.Split("=").ToArray()[1];
                        break;
                    }
                }

                if (mach.Contains("]"))
                {
                    mach = mach.Split("]").ToArray()[0];
                }
                mach = mach.Trim();
                return mach.Split(",").ToList();

            }
            catch(Exception ex)
            {
                Gestion.Erreur($"file:{file} - {ex.Message}");
                return new List<string>();
            }            
        }

        
        #endregion


    }

    public class StatusServiceMachineModel
    {
        public string StatusService { get; set; }
        public string MachineName { get; set; } 
        public string Message { get; set; }
    }
}
