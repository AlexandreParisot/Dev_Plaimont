using ComptageVDG.Helpers;
using ComptageVDG.IServices;
using ComptageVDG.Models;
using ComptageVDG.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ComptageVDG.ViewModels
{
    public class BaseViewModel:INotifyPropertyChanged
    {

        // mettre les constante du programme
        protected static string FileIni = $"{Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase)}.ini";
        protected static string PathProg = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        protected static string FullFileIni = Path.Combine(PathProg, FileIni);

        protected static string _lastSynchro;

        private static string _dateCampagne;
        public  string DateCampagne { get => _dateCampagne; set {
                var test = false;
                if (_dateCampagne != value)
                    test = true;

                SetProperty(ref _dateCampagne, value); 

                if(test)
                    Message.Notification("CHANGEDATE", value);
            } }

        private static Dictionary<string, string> listeCampagne;    
        public  Dictionary<string, string> ListeCampagne { get => listeCampagne; set => SetProperty(ref listeCampagne, value); }


        private static ObservableCollection<ParcelleModel> parcelleModels;
        public ObservableCollection<ParcelleModel> ParcelleModels { get => parcelleModels; set => SetProperty(ref parcelleModels, value); }

        private static ObservableCollection<ParcelleModel> parcelleModelinCampagne;
        public ObservableCollection<ParcelleModel> ParcelleModelsinCampagne { get => parcelleModelinCampagne; set => SetProperty(ref parcelleModelinCampagne, value); }

        protected static ConnexionService? Connexion = new ConnexionService();

        // mettre les services en static pour les rendre accessible des View models
        //protected static NavigationService navigationService = new NavigationService();

        protected static MessagingService Message = new MessagingService();
        protected static LoadService LoadApp = new LoadService(FullFileIni);
        protected static IServiceCampagne ServiceCampagne;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T oldValue, T newValue, [CallerMemberName] string name = null)
        {
            // Todo test egalité
            //ancienne valeur = nouvelle valeur
            oldValue = newValue;
            //PropertyChanged -> nom de la propriété
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected void RaisPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        protected virtual void ShowLoading(string MessageLoading)
        {
            Message.Notification("LOADING", MessageLoading);
        }

        protected virtual void ClearLoading()
        {
            Message.Notification("UNLOADING", string.Empty);
        }

        protected virtual void ErrorNotif(string message)
        {
            Message.Notification("ERRORNOTIF", message);
        }

        protected virtual void WarningNotif(string message)
        {
            Message.Notification("WARNINGNOTIF", message);
        }

        protected virtual void InfoNotif(string message)
        {
            Message.Notification("INFONOTIF", message);
        }

        protected virtual void SuccessNotif(string message)
        {
            Message.Notification("SUCCESSNOTIF", message);
        }
    }
}
