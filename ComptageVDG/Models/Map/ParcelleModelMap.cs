using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Models.Map
{
    public class ParcelleModelMap : ClassMap<ParcelleModel>
    {
        public ParcelleModelMap()
        {
            Map(m => m.id_parcelle).Name("ParcelleId");
            Map(m => m.surface).Name("Surface");
            Map(m => m.ut).Name("UtId");
            Map(m => m.nameParcelle).Name("Designation");
            Map(m => m.nameParcelle2).Name("Designation2");
            Map(m => m.cptGlomerule).Name("Glomerule");
            Map(m => m.cptPerforation1).Name("Perforation");
            Map(m => m.cptPerforation2).Name("Perforation2");
            Map(m => m.inCampagne).Name("InCampagne");
            Map(m => m.qualite).Name("Qualite");
        }
    }
}
