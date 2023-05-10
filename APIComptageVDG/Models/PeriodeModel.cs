namespace APIComptageVDG.Models
{
    public class PeriodeModel
    {
        public int Id_periode { get; set; }

        public string Name { get; set; }

        public int Year { get; set; }

        public DateTime? DateDebut { get; set; }

        public DateTime? DateFin { get; set; }
    }
}
