namespace APIComptageVDG.Models
{
    public class ServiceMachinesModel
    {
        public string NameService { get; set; }
        public string ExecutablePathPilote { get; set; }
        public List<string> Machines { get; set; }

        public ServiceMachinesModel()
        {
            Machines = new List<string>();
        }

        public string GetMachines()
        {
            return string.Join(",", Machines);
        }
    }
}
