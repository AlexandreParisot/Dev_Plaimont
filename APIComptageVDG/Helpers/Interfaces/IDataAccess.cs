using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace APIComptageVDG.Helpers.Interfaces
{
    public interface IDataAccess
    {

        IDbConnection DbConnect { get; set; }
        bool IsConnected { get; }

        void SetConnexion(string connexionString);
        void SetConnexion(IDbConnection connexion);
    }

}
