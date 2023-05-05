using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Converters
{
    public class ComboDictionary
    {
        private static Dictionary<string, string> _cbYears = null;   
        public static Dictionary<string, string> ComboYears
        {
            get { 
                 _cbYears = new Dictionary<string, string>() ;

                _cbYears.Add("2023", "Campagne 2023");
                _cbYears.Add("2022", "Campagne 2022");
                _cbYears.Add("2021", "Campagne 2021");

                return _cbYears; 
            }
        
        }


    }
}
