
using APIComptageVDG.Interfaces;
using System.IO;
using System.Management;
using System.ServiceProcess;

namespace APIComptageVDG.Helpers
{
    public class ServicesHelper:IGestionService

    {
        private  string _status { get; set; }
        public   string Status { get => _status; }

        public  bool StartService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                
                var status = service.Status;
                if (service.Status != ServiceControllerStatus.Running)
                {
                    service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }
                return true;
            }
            catch
            {
                
                return false;
            }
            finally
            {
                _status = service.Status.ToString();
            }
            
        }

        public  bool StopService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                if (service.Status != ServiceControllerStatus.Stopped)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                _status = service.Status.ToString();
            }
            
        }

        public  bool RestartService(string serviceName)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running);
                return true;
            }
            catch
            {
                return false;

            }
            finally
            {
                _status = service.Status.ToString();
            }
           
        }

        public  string GetServiceName(string ExecutablePath)
        {
            string serviceName = string.Empty;
            using (ManagementObjectSearcher searcher = new  ManagementObjectSearcher("SELECT * FROM Win32_Service"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    if (obj["PathName"] != null && obj["PathName"].ToString().Contains(ExecutablePath))
                    {
                        serviceName = obj["Name"].ToString();
                        break;
                    }
                }
            }
            return serviceName;
        }


        public  string GetServiceStatus(string serviceName)
        {
            // Créer une instance de la classe ServiceController avec le nom du service
            ServiceController service = new ServiceController(serviceName);

            // Vérifier si le service existe
            if (!ServiceController.GetServices().Any(s => s.ServiceName == serviceName))
            {
                return "Le service n'existe pas.";
            }

            try
            {
                // Récupérer le statut actuel du service
                ServiceControllerStatus status = service.Status;
                _status = status.ToString();
                // Retourner le statut sous forme de chaîne de caractères
                switch (status)
                {
                    case ServiceControllerStatus.Running:
                        return "Le service est en cours d'exécution.";

                    case ServiceControllerStatus.Stopped:
                        return "Le service est arrêté.";

                    case ServiceControllerStatus.Paused:
                        return "Le service est en pause.";

                    case ServiceControllerStatus.StartPending:
                        return "Le service est en cours de démarrage.";

                    case ServiceControllerStatus.StopPending:
                        return "Le service est en cours d'arrêt.";

                    case ServiceControllerStatus.PausePending:
                        return "Le service est en cours de mise en pause.";

                    case ServiceControllerStatus.ContinuePending:
                        return "Le service est en cours de reprise.";

                    default:
                        return "Statut inconnu.";
                }
            }
            catch (Exception ex)
            {
                return "Erreur lors de la récupération du statut du service : " + ex.Message;
            }
        }
    }

}
