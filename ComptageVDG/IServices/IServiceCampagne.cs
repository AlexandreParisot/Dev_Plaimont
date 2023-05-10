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
        Task<ObservableCollection<ParcelleModel>> asyncLoadYearCampagne(string DateCampagne);
        Task<ObservableCollection<ParcelleModel>> asyncLoadYearInCampagne(string DateCampagne);
        ObservableCollection<ParcelleModel> LoadYearCampagne(string DateCampagne);
        Task<Dictionary<string, string>> asyncDicoCampagne();
        Dictionary<string, string> DicoCampagne();
        
        ObservableCollection<PeriodeModel> GetPeriodeCampagne(string DateCampagne);
        
        Task<ObservableCollection<PeriodeModel>> asyncGetPeriodeCampagne(string DateCampagne);
        
        bool SetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne);
        
        Task<bool> asyncSetPeriodeCampagne(IEnumerable<PeriodeModel> periodeCampagne, string DateCampagne);
        
        Task<bool> asyncOpenParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year);
        Task<bool> asyncOpenParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel);       
        Task<bool> asyncOpenParcelleCampagne(ParcelleModel parcelle,int Year);
        Task<bool> asyncOpenParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel);
        bool OpenParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year);
        bool OpenParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel);
        bool OpenParcelleCampagne(ParcelleModel parcelle, int Year);
        bool OpenParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel);
        
        Task<bool> asyncCloseParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year);
        Task<bool> asyncCloseParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel);
        Task<bool> asyncCloseParcelleCampagne(ParcelleModel parcelle, int Year);
        Task<bool> asyncCloseParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel);
        bool CloseParcellesCampagne(IEnumerable<ParcelleModel> parcelle, int Year);
        bool CloseParcellesPeriode(IEnumerable<ParcelleModel> parcelle, PeriodeModel periodeModel);
        bool CloseParcelleCampagne(ParcelleModel parcelle, int Year);
        bool CloseParcellePeriode(ParcelleModel parcelle, PeriodeModel periodeModel);
        Task<string> getTimeSynchroCampagne();
    }
}
