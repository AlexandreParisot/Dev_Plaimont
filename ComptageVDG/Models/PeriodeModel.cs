using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ComptageVDG.Models
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
