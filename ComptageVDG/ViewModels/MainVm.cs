using ComptageVDG.Converters;
using ComptageVDG.Helpers;
using ComptageVDG.Models;
using ComptageVDG.Models.Map;
using ComptageVDG.Views;
using Infragistics.Windows.DataPresenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ComptageVDG.ViewModels
{
    internal class MainVm:BaseViewModel
    {
        private string currentView = string.Empty;

        public RelayCommand SynchroInstagrappeCommand { get; set; } 
        public RelayCommand OpenDialogConnexionCommand { get; set; }
        public RelayCommand PeriodeCommand { get; set; }
        public RelayCommand DeclarationCommand { get; set; }


        public event EventHandler LoadUC;

        public  MainVm()
        {

            Message.Notify += Message_Notify;

            OpenDialogConnexionCommand = new RelayCommand( ()=>{               
                DialogParameter dialogParameter = new DialogParameter();
                dialogParameter.ShowDialog();
            });

            SynchroInstagrappeCommand = new RelayCommand(SynchroPeriodeInstaGrappeCommandExecute);

            DeclarationCommand = new RelayCommand(() => { toggleView("Parcelle"); });
            PeriodeCommand = new RelayCommand(() => { toggleView("Periode"); });    
            
        }

        public void CloseVM()
        {
            Message.Notify -= Message_Notify;
        }


        private async void SynchroPeriodeInstaGrappeCommandExecute()
        {

            if (string.IsNullOrEmpty(DateCampagne) || DateCampagne != DateTime.Today.ToString("yyyy"))
            {
                WarningNotif("On ne peut pas faire de Synchro Instagrappe en dehors d'une campagne en cours.");
                return;
            }
               
            if(currentView == "Periode")
                Message.Notification("SYNCHRO",String.Empty);
            else
            {
                ShowLoading($"Synchronisation Instagrappe pour la campagne {DateCampagne}.");
                //il faut la liste de parcelle autorisé + la periode
                //await ServiceCampagne.OpenParcelle(null).with;
                await Task.Delay(TimeSpan.FromSeconds(3));
                ClearLoading();
            }
        }

        public async Task<bool> LoadDicoCampagne(bool firstLoad = false)
        {            
            var dico = await ServiceCampagne.asyncDicoCampagne();

            if (dico != null && dico.Count > 0)
            {
                ListeCampagne = dico;
                DateCampagne = dico.First().Key;
                return true;
            }
            else
            {
                ListeCampagne = new Dictionary< string, string>();
                DateCampagne = String.Empty;
                return false;
            }             
        }


        public async void LoadMainView()
        {
            ShowLoading("Chargement informations campagne ...");
            if (await LoadDicoCampagne(true))
            {
                ShowLoading($"Chargement parcelle {DateCampagne} ...");
                 await ServiceCampagne.asyncLoadYearCampagne(DateCampagne).ContinueWith((x) => {
                    if (x.IsCompleted)
                         ParcelleModels = x.Result;
                      ClearLoading();
                }) ;

                toggleView("Campagne");                
            }
            else
            {
                ClearLoading();
                toggleView("Periode");
            }
               
            

        }

        private  void Message_Notify(object? sender, MessageEventArgs e)
        {
            if( e.Data is string retour && retour == "RETOUR")
            {
                toggleView("Campagne");
            }

            if(e.Data is string load && load == "CHARGEVIEW")
            {
                LoadMainView();
            }
        }

        private void toggleView(string View)
        {

            switch (View)
            {
                case "Campagne":
                    UcLoad(typeof(CampagneView).FullName);                   
                    break;
                case "Periode":
                    UcLoad(typeof(PeriodeView).FullName);
                    break;
                case "Parcelle":
                    UcLoad(typeof(ParcelleView).FullName);
                    break;
            }

            currentView = View;
        }


        private void UcLoad(string? nameUc)
        {
            Dispatcher.CurrentDispatcher.BeginInvoke(() => LoadUC?.Invoke(nameUc, EventArgs.Empty) );
        }
    }
}
