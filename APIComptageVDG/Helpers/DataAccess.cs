using System.Data.SqlClient;
using System.Data;
using APIComptageVDG.Helpers.Interfaces;

namespace APIComptageVDG.Helpers
{
    public class DataAccess : IDataAccess
    {

        private IDbConnection dbConnect = new SqlConnection();
        public IDbConnection DbConnect { get => dbConnect; set => dbConnect = value; }

        public bool IsConnected
        {
            get
            {
                try
                {
                    if (dbConnect.State == ConnectionState.Open || dbConnect.State == ConnectionState.Connecting)
                        return true;
                    else
                    {
                        dbConnect.Open();
                        if (dbConnect.State == ConnectionState.Open || dbConnect.State == ConnectionState.Connecting)
                            return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Gestion.Erreur(ex.Message);
                    return false;
                }
                finally
                {
                    // dbConnect.Close();
                }
            }
        }

        public void SetConnexion(string connexionString)
        {
            if (string.IsNullOrEmpty(connexionString))
                throw new ArgumentNullException("La chaine de connexion est null.");

            if (DbConnect == null)
                throw new ArgumentNullException("La connexion est null.");

            if (DbConnect.State != ConnectionState.Open)
                DbConnect.ConnectionString = connexionString;

        }

        public void SetConnexion(IDbConnection connexion)
        {
            if (DbConnect == null)
                throw new ArgumentNullException("L'objet connexion est null.");

            this.DbConnect = connexion;
        }
    }
}
