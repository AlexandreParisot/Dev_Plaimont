using ComptageVDG.Converters;
using ComptageVDG.Helpers;
using ComptageVDG.Helpers.Interfaces;
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
        

        public RelayCommand SynchroInstagrappeCommand { get; set; } 
        public RelayCommand OpenDialogConnexionCommand { get; set; }
        public RelayCommand PeriodeCommand { get; set; }
        public RelayCommand DeclarationCommand { get; set; }


        public event EventHandler LoadUC;

        public  MainVm()
        {
            MessageBrokerImpl.Instance.Subscribe<MessageEventArgs>(PayloadMessage);

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
            MessageBrokerImpl.Instance.Unsubscribe<MessageEventArgs>(PayloadMessage);
        }


        private async void SynchroPeriodeInstaGrappeCommandExecute()
        {

            if (string.IsNullOrEmpty(DateCampagne) || DateCampagne != DateTime.Today.ToString("yyyy"))
            {
                WarningNotif("On ne peut pas faire de Synchro Instagrappe en dehors d'une campagne en cours.");
                return;
            }
               
            if(currentView == "Periode")
                MessagingService.Sender?.Notification("SYNCHRO",String.Empty);
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
                if (DateCampagne != DateTime.Today.ToString("yyyy"))
                {
                    ShowLoading($"Création de la période {DateTime.Today.ToString("yyyy")}...");

                    await CreationPeriodeAnneeCourrante().ContinueWith((x) => {
                        if (x.IsCompleted && x.IsFaulted)
                            ErrorNotif($"Erreur lors de la creation de la période {DateTime.Today.ToString("yyyy")}");
                    });
                }                
                   await LoadParcelle(DateCampagne);
                    toggleView("Campagne");
            }
            else
            {
               ShowLoading($"Création de la période {DateTime.Today.ToString("yyyy")}...");
               await CreationPeriodeAnneeCourrante().ContinueWith((x) => {
                   if (x.IsCompleted && x.IsFaulted)
                       ErrorNotif($"Erreur lors de la creation de la période {DateTime.Today.ToString("yyyy")}");
               });


                await LoadParcelle(DateCampagne);
                toggleView("Campagne");
            }
        }

        private async Task LoadParcelle(string date)
        {

            ShowLoading($"Chargement parcelles {date} ...");
            ParcelleModels = await ServiceCampagne.asyncLoadYearCampagne(date);
            if (ParcelleModels == null)
                ErrorNotif($"Erreur lors du chargement des parcelles.");

            ClearLoading();
                   
        }

        private async Task<bool> CreationPeriodeAnneeCourrante()
        {
                        
            var listePeriodes = new List<PeriodeModel>() { 
                new PeriodeModel() { Year= int.Parse(DateTime.Today.ToString("yyyy")), Name = "Glomerule"}
                ,new PeriodeModel() { Year= int.Parse(DateTime.Today.ToString("yyyy")), Name = "Perforation"}
                ,new PeriodeModel() { Year= int.Parse(DateTime.Today.ToString("yyyy")), Name = "Perforation2"}};

            if (await ServiceCampagne.asyncSetPeriodeCampagne(listePeriodes, DateTime.Today.ToString("yyyy")))
            {
                ListeCampagne.Add(DateTime.Today.ToString("yyyy"), $"Campagne {DateTime.Today.ToString("yyyy")}");
                DateCampagne = ListeCampagne.Last().Key;
            } else
                return false;
            return true;
        }


        private void PayloadMessage(MessagePayload<MessageEventArgs> obj)
        {
            
            if (obj.What.Data is string retour && retour == "RETOUR")
            {
                toggleView("Campagne");
            }

            if(obj.Who is MainView view && obj.What.Sender == "TOGGLEVIEW" && obj.What.Data is string strview)
            {
                toggleView(strview);
            }


            if (obj.What.Data is string load && load == "CHARGEVIEW")
            {
                LoadMainView();
            }
        }

        //private  void Message_Notify(object? sender, MessageEventArgs e)
        //{
           

        //}

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
            Dispatcher.CurrentDispatcher.BeginInvoke(() => LoadUC?.Invoke(nameUc, EventArgs.Empty),DispatcherPriority.Loaded);
        }
    }
}
