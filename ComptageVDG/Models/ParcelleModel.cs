using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Models
{
    public class ParcelleModel
    {
        public bool inCampage { get; set; }
        public int id_parcelle { get; set; }
        public string ut { get; set; }
        public string nameParcelle { get; set; }
        public string nameParcelle2 { get; set; }   
        public decimal? surface { get; set; }
        public decimal? qualite  { get; set; }
        public int? cptGlomerule { get; set; }  
        public int? cptPerforation1 { get; set; }
        public int? cptPerforation2 { get; set; }
        
    }
}
