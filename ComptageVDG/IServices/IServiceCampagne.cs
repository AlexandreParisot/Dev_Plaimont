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

        Task<bool> asyncSynchroInstagrappeDeclaration(int Year);        
        Task<bool> asyncSynchroInstagrappeCompteur(int Year);

        Task<bool> asyncOpenParcelleCampagne(ParcelleModel parcelle,int Year);      
        Task<string> getTimeSynchroCampagne();
    }
}
