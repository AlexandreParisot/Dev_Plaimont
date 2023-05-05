using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Models
{
    public class PeriodeCampagneModel
    {
        public int DateCampagne { get; set; }   
        public PeriodeModel Glomerules  { get; set; }
        public PeriodeModel Perforation1 { get; set; }
        public PeriodeModel Perforation2 { get; set; }

    }
}
