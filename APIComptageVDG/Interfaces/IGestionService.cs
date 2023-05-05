using System.Net.NetworkInformation;

namespace APIComptageVDG.Interfaces
{
    public interface IGestionService
    {
        string Status { get; }
        bool StartService(string serviceName);
        bool StopService(string serviceName);
        bool RestartService(string serviceName);
        string GetServiceName(string ExecutablePath);
        string GetServiceStatus(string serviceName);
    }
}
