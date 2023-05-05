using ComptageVDG.IServices;
using ComptageVDG.Models;
using ComptageVDG.Models.Map;
using ComptageVDG.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Xml;

namespace ComptageVDG.Services
{
    public class ServiceCampagneCsv:IServiceCampagne
    {
        private ConnexionService connexionService;
        public ServiceCampagneCsv(ConnexionService? connexion)
        {
            connexionService = connexion;
        }

        public async Task<ObservableCollection<ParcelleModel>> asyncLoadYearCampagne(string DateCampagne)
        {
            return await Task.Run(() => LoadYearCampagne(DateCampagne));
        }
        public ObservableCollection<ParcelleModel> LoadYearCampagne(string DateCampagne)
        {
            //var LstParcelle = new ObservableCollection<ParcelleModel>();
            if (connexionService?.Instance == null)
                return new ObservableCollection<ParcelleModel>();

            var lstParcelle= connexionService.Instance.ExecuteQuery<ParcelleModel>($"Campagne{DateCampagne}.csv",new object[] {typeof(ParcelleModelMap).FullName});
            var lst = lstParcelle.ToList();
            return new ObservableCollection<ParcelleModel>(lst);
        }

        public async Task<Dictionary<string, string>> asyncDicoCampagne()
        {
            return await Task.Run(() => DicoCampagne());
        }

        public Dictionary<string, string> DicoCampagne()
        {
            if(connexionService.Instance == null)
                return new Dictionary<string, string>();

            var dic = new Dictionary<string, string>();
            var lstCampagne = connexionService.Instance.ExecuteQuery(Path.Combine(connexionService.Instance.ConnectionString,$"Campagnes.csv"));
            if (lstCampagne.Rows.Count > 0)
            {
                foreach (DataRow item in lstCampagne.Rows)
                {
                    dic.Add(item["Key"].ToString(), item["Value"].ToString());
                }
            }
            
            return dic;
        }

        public ObservableCollection<PeriodeModel> GetPeriodeCampagne(string DateCampagne)
        {
            //var LstParcelle = new ObservableCollection<ParcelleModel>();
            if (connexionService?.Instance == null)
                return new ObservableCollection<PeriodeModel>();

            var lstPeriode = connexionService.Instance.ExecuteQuery<PeriodeModel>($"Periode{DateCampagne}.csv", new object[] { typeof(PeriodeModelMap).FullName }); //
            var lst = lstPeriode.ToList();
            return new ObservableCollection<PeriodeModel>(lst);
        }

        public async Task<ObservableCollection<PeriodeModel>> asyncGetPeriodeCampagne(string DateCampagne)
        {
            return await Task.Run(() => GetPeriodeCampagne(DateCampagne));
        }

        public bool SetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne)
        {
            if (connexionService?.Instance == null)
                return false;

            var Dt = new DataTable($"Periode{DateCampagne}");
            Dt.Columns.Add("id");
            Dt.Columns.Add("year");
            Dt.Columns.Add("name");
            Dt.Columns.Add("datedeb");
            Dt.Columns.Add("datefin");
            foreach(var periode in periodeCampagne)
            {
                var dr =Dt.NewRow();
                dr.SetField("id", periode.Id_periode);
                
                dr.SetField("year", periode.Year);
                dr.SetField("name",periode.Name);
                dr.SetField("datedeb",periode.DateDebut.ToString("dd/MM/yyyy"));
                dr.SetField("datefin",periode.DateFin.ToString("dd/MM/yyyy")) ;
                Dt.Rows.Add(dr);    
            }
           var result = connexionService.Instance.ExecuteNoQuery($"Periode{DateCampagne}.csv", new object[] { Dt, typeof(DataTable).FullName}); //

            return (result >= 0 ? true : false);
        }

        public async Task<bool> asyncSetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne)
        {
            return await Task.Run(() => SetPeriodeCampagne(periodeCampagne, DateCampagne));
        }

        #region Gestion API Instagrappe
        #endregion

        public async Task<bool> asyncOpenParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year)
        {
            return await Task.Run(() => OpenParcellesCampagne(parcelle, Year));
        }

        public async Task<bool> asyncOpenParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel)
        {
            return await Task.Run(() => OpenParcellesPeriode(parcelle, periodeModel));
        }

        public async Task<bool> asyncOpenParcelleCampagne(ParcelleModel parcelle, int Year)
        {
            return await Task.Run(() => OpenParcelleCampagne(parcelle, Year));
        }

        public async Task<bool> asyncOpenParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel)
        {
            return await Task.Run(() => OpenParcellePeriode(parcelle, periodeModel));
        }

        public bool OpenParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year)
        {
            //throw new NotImplementedException();
            return false;
        }

        public bool OpenParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel)
        {
            //throw new NotImplementedException();
            return false;
        }

        public bool OpenParcelleCampagne(ParcelleModel parcelle, int Year)
        {
            //throw new NotImplementedException();
            return false;
        }

        public bool OpenParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel)
        {
            //throw new NotImplementedException();
            return false;
        }

        public async Task<bool> asyncCloseParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year)
        {
            return await Task.Run(() => CloseParcellesCampagne(parcelle, Year));
        }

        public async Task<bool> asyncCloseParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel)
        {
            return await Task.Run(() => CloseParcellesPeriode(parcelle, periodeModel));
        }

        public async Task<bool> asyncCloseParcelleCampagne(ParcelleModel parcelle, int Year)
        {
            return await Task.Run(() => CloseParcelleCampagne(parcelle, Year));
        }

        public async Task<bool> asyncCloseParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel)
        {
            return await Task.Run(() => CloseParcellePeriode(parcelle, periodeModel));
        }

        public bool CloseParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year)
        {
            //throw new NotImplementedException();
            return false;
        }

        public bool CloseParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel)
        {
            //throw new NotImplementedException();
            return false;
        }

        public bool CloseParcelleCampagne(ParcelleModel parcelle, int Year)
        {
            //throw new NotImplementedException();
            return false;
        }

        public bool CloseParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel)
        {
            //throw new NotImplementedException();
            return false;
        }
    }
}
