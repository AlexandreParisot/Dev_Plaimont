using ComptageVDG.Models;
using ComptageVDG.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.IServices
{
     public interface IServiceCampagne
    {
       public Task<ObservableCollection<ParcelleModel>> asyncLoadYearCampagne(string DateCampagne);
        public ObservableCollection<ParcelleModel> LoadYearCampagne(string DateCampagne);
        public Task<Dictionary<string, string>> asyncDicoCampagne();
        public Dictionary<string, string> DicoCampagne();

        public ObservableCollection<PeriodeModel> GetPeriodeCampagne(string DateCampagne);

        public Task<ObservableCollection<PeriodeModel>> asyncGetPeriodeCampagne(string DateCampagne);

        public bool SetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne);

        public Task<bool> asyncSetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne);

        public Task<bool> asyncOpenParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year);
        public Task<bool> asyncOpenParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel);       
        public Task<bool> asyncOpenParcelleCampagne(ParcelleModel parcelle,int Year);
        public Task<bool> asyncOpenParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel);
        public bool OpenParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year);
        public bool OpenParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel);
        public bool OpenParcelleCampagne(ParcelleModel parcelle, int Year);
        public bool OpenParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel);

        public Task<bool> asyncCloseParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year);
        public Task<bool> asyncCloseParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel);
        public Task<bool> asyncCloseParcelleCampagne(ParcelleModel parcelle, int Year);
        public Task<bool> asyncCloseParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel);
        public bool CloseParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year);
        public bool CloseParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel);
        public bool CloseParcelleCampagne(ParcelleModel parcelle, int Year);
        public bool CloseParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel);
    }
}
