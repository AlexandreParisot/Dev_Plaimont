using ComptageVDG.DataAccess;
using ComptageVDG.IServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Services
{
    public  class ConnexionService
    {
        public IDataAccess? Instance = null;
        public IServiceCampagne? ServiceCampagne = null;

        public  bool CreateAccess(string connexionString)
        {
            Instance = null;

            if (Directory.Exists(connexionString))
            {
                Instance = new DataAccessFileCsv() { ConnectionString = connexionString };
                ServiceCampagne = new ServiceCampagneCsv(this);
                return true;
            }

            return false;
                       
        }

        public string GetUserWindows()
        {
            return WindowsIdentity.GetCurrent().Name; 
        }

        public List<string> GetCredentialWindows()
        {
            List<string> groupWindows = new List<string>();
            
            IntPtr logonToken = WindowsIdentity.GetCurrent().Token;

            WindowsIdentity wi = new WindowsIdentity(logonToken);

            foreach (IdentityReference group in wi.Groups)
            {
                try
                {
                    groupWindows.Add(group.Translate(typeof(NTAccount)).ToString());
                }
                catch (Exception ex) {
                    return null;
                }
            }
            groupWindows.Sort();

            return groupWindows;
        }

    }

}
