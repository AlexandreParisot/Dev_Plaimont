using ComptageVDG.DataAccess;
using ComptageVDG.Helpers;
using ComptageVDG.IServices;
using ComptageVDG.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Services
{
    internal class ServiceCampagneApi : IServiceCampagne
    {

        private ConnexionService connexionService;
        public ServiceCampagneApi(ConnexionService? connexion)
        {
            connexionService = connexion;
        }


        #region Implementation Interface
       
        public async Task<Dictionary<string, string>> asyncDicoCampagne()
        {
            return await Task.Run(() => DicoCampagne());
        }

        public async Task<ObservableCollection<PeriodeModel>> asyncGetPeriodeCampagne(string DateCampagne)
        {
            return await Task.Run(()=> GetPeriodeCampagne(DateCampagne));
        }

        public async Task<ObservableCollection<ParcelleModel>> asyncLoadYearCampagne(string DateCampagne)
        {
           return await Task.Run(()=> LoadYearCampagne(DateCampagne));
        }

        public async Task<ObservableCollection<ParcelleModel>> asyncLoadYearInCampagne(string DateCampagne)
        {
            var ListPeriode = new ObservableCollection<ParcelleModel>( await connexionService.Instance.AsyncExecuteQuery<ParcelleModel>("CampagneVDG/Lavilog/GetParcellesCampagne", new object[] { HttpMethod.Get, $"year={DateCampagne}" }));

            return ListPeriode;
        }

        public async Task<bool> asyncOpenParcelleCampagne(ParcelleModel parcelle, int Year)
        {
            var list = new List<ParcelleModel>();
            list.Add(parcelle);
            string cmd = JsonSerialize.Serialize(list);
            var retour = await connexionService.Instance.AsyncExecuteNoQuery($"CampagneVDG/Lavilog/SetParcellesCampagne?year={Year.ToString()}", new object[] { cmd });
            if (retour == 1)
                return true;
            return false;
        }  

        public async Task<bool> asyncSetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne)
        {
            return await Task.Run(() => SetPeriodeCampagne(periodeCampagne, DateCampagne));
        }

      
        

        public Dictionary<string, string> DicoCampagne()
        {
            var dico = new Dictionary<string, string>();
            //connexionService.Instance.ExecuteQuery<KeyValuePair<int, string>>("CampagneVDG/Lavilog/GetCampagnes");
            var Listkvp = ((DataAccessApi)connexionService.Instance).ExecuteQueryofT<Dictionary<int, string>>("CampagneVDG/Lavilog/GetCampagnes"); 
            if (Listkvp != null && Listkvp.Count() > 0)
            {
                foreach (var item in Listkvp)
                {
                    dico.Add(item.Key.ToString(), item.Value);
                }
            }
            return dico;
        }

        public ObservableCollection<PeriodeModel> GetPeriodeCampagne(string DateCampagne)
        {
            var ListPeriode = new ObservableCollection<PeriodeModel>(connexionService.Instance.ExecuteQuery<PeriodeModel>("CampagneVDG/Lavilog/GetPeriode", new object[] {HttpMethod.Get,$"year={DateCampagne}"}));

            return ListPeriode;

        }

        public async Task<string> getTimeSynchroCampagne()
        {
            return ((DataAccessApi)connexionService.Instance).ExecuteQueryofT<string>("CampagneVDG/Lavilog/GetLastSynchro");
        }

        public ObservableCollection<ParcelleModel> LoadYearCampagne(string DateCampagne)
        {
            var ListPeriode = connexionService.Instance.ExecuteQuery<ParcelleModel>("CampagneVDG/Lavilog/GetParcelles", new object[] { HttpMethod.Get, $"year={DateCampagne}" });
               

            return  ListPeriode == null? null : new ObservableCollection<ParcelleModel>(ListPeriode) ;
        }

        public async Task<bool> asyncSynchroInstagrappeCompteur(int Year)
        {
            return await ((DataAccessApi)connexionService.Instance).AsyncExecuteQueryofT<bool>($"CampagneVDG/Instagrappe/SynchroCompteur", new object[] { $"year={Year.ToString()}" });
        }

        public async Task<bool> asyncSynchroInstagrappeDeclaration(int Year)
        {
            return await ((DataAccessApi)connexionService.Instance).AsyncExecuteQueryofT<bool>($"CampagneVDG/Instagrappe/SynchroDeclaration", new object[] { $"year={Year.ToString()}" });
        }

        public bool SetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne)
        {
           int retour = connexionService.Instance.ExecuteNoQuery("CampagneVDG/Lavilog/SetPeriodes",new object[] { JsonSerialize.Serialize(periodeCampagne) });

            if(retour != periodeCampagne.Count())
                return false;

            return true;
        }

    # endregion
    }
}
