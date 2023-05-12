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
        public Task<bool> asyncCloseParcelleCampagne(ParcelleModel parcelle, int Year)
        {
            throw new NotImplementedException();
        }

        public Task<bool> asyncCloseParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> asyncCloseParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year)
        {
            throw new NotImplementedException();
        }

        public Task<bool> asyncCloseParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel)
        {
            throw new NotImplementedException();
        }

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

        public Task<bool> asyncOpenParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel)
        {
            throw new NotImplementedException();
        }

        public Task<bool> asyncOpenParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year)
        {
            throw new NotImplementedException();
        }

        public Task<bool> asyncOpenParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> asyncSetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne)
        {
            return await Task.Run(() => SetPeriodeCampagne(periodeCampagne, DateCampagne));
        }

        public bool CloseParcelleCampagne(ParcelleModel parcelle, int Year)
        {
            throw new NotImplementedException();
        }

        public bool CloseParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel)
        {
            throw new NotImplementedException();
        }

        public bool CloseParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year)
        {
            throw new NotImplementedException();
        }

        public bool CloseParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> DicoCampagne()
        {
            var dico = new Dictionary<string, string>();
            //connexionService.Instance.ExecuteQuery<KeyValuePair<int, string>>("CampagneVDG/Lavilog/GetCampagnes");
            var Listkvp = ((DataAccessApi)connexionService.Instance).ExecuteQueryofT<Dictionary<int, string>>("CampagneVDG/Lavilog/GetCampagnes"); 
            if (Listkvp.Count() > 0)
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

        public bool OpenParcelleCampagne(ParcelleModel parcelle, int Year)
        {
            throw new NotImplementedException();
        }

        public bool OpenParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel)
        {
            throw new NotImplementedException();
        }

        public bool OpenParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year)
        {
            throw new NotImplementedException();
        }

        public bool OpenParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel)
        {
            throw new NotImplementedException();
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
