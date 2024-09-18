using ComptageVDG.Helpers;
using ComptageVDG.Helpers.Interfaces;
using ComptageVDG.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.ViewModels
{
    public class ParcelleVM:BaseViewModel
    {

        private bool isDirty = false;

        private bool isRead;
        public bool IsRead { get => isRead; set => SetProperty(ref isRead, value); }
       // public RelayCommand OpenCampagneCommand { get; set; }
        public RelayCommand RetourCommand { get; set; }

        public RelayCommand RefreshCommand { get; set; }

        public RelayCommand<ParcelleModel> ChangeStateCommand { get; set; }

        public ParcelleVM()
        {
            MessageBrokerImpl.Instance.Subscribe<MessageEventArgs>(PayloadMessage);

            RetourCommand = new RelayCommand(() => {
                if (isDirty)
                    ParcelleModelsinCampagne = null;
                MessageBrokerImpl.Instance.Publish(this, MessageBrokerImpl.Notification("ParcelleVM", "RETOUR"));
                
            });
            RefreshCommand = new RelayCommand(async () =>
            {
                ShowLoading($"Chargement de la campagne {DateCampagne}");
                await asyncLoadParcelles(DateCampagne).ContinueWith((x) => { ClearLoading(); });
                IsRead = (DateCampagne == DateTime.Today.ToString("yyyy")) ? false : true;
            });

            ChangeStateCommand = new RelayCommand<ParcelleModel>(ChangeStateCommandExcute);

            if(ParcelleModels != null)
            {
                if(!string.IsNullOrEmpty(DateCampagne) && ParcelleModels.First().campagne.ToString() != DateCampagne)
                    MessageBrokerImpl.Instance?.Publish(this, MessageBrokerImpl.Notification("CHANGEDATE", DateCampagne));

            }

            //OpenCampagneCommand = new RelayCommand(async() => {
            //    ShowLoading($"Synchronisation Instagrappe des parcelles pour la campagne {DateCampagne}");
            //    await ServiceCampagne.asyncOpenParcellesCampagne(ParcelleModels.ToList(), int.Parse(DateCampagne)).ContinueWith((x) => { ClearLoading();});
            //});

        }

        private  void PayloadMessage(MessagePayload<MessageEventArgs> obj)
        {
            if (obj.What.Sender == "CHANGESTATE" && currentView == "Parcelle")
            {
                if ((obj.What.Data is ParcelleModel parcelle) && parcelle != null)
                {

                    ChangeStateCommandExcute(parcelle); //.ContinueWith((x) => ClearLoading());

                }
            }

            if (obj.What.Sender == "CHANGEDATE")
            {
                if ((obj.What.Data is string dateCp) && !string.IsNullOrEmpty(dateCp))
                {
                    ShowLoading($"Chargement de la campagne {dateCp}");
                    asyncLoadParcelles(dateCp).ContinueWith((x) => { ClearLoading(); });
                    IsRead = (dateCp == DateTime.Today.ToString("yyyy")) ? false : true;
                }
            }

        }

        private async void ChangeStateCommandExcute(ParcelleModel obj)
        {
            await asyncChangeStateParcelle(obj);

        }

        //private void Message_Notify(object? sender, MessageEventArgs e)
        //{
        //    if (e.Sender == "CHANGEDATE")
        //    {
        //        if ((e.Data is string dateCp) && !string.IsNullOrEmpty(dateCp))
        //        {
        //            ShowLoading($"Chargement de la campagne {dateCp}");
        //            asyncLoadParcelles(dateCp).ContinueWith((x) => { ClearLoading(); });
        //            IsRead = (dateCp == DateTime.Today.ToString("yyyy")) ? false : true;
        //        }
        //    }

        //    //if(e.Sender == "CHANGESTATE" && currentView == "Parcelle")
        //    //{
        //    //    if ((e.Data is ParcelleModel parcelle) && parcelle != null)
        //    //    {
        //    //        ShowLoading("Changement état parcelle ...");
        //    //        asyncChangeStateParcelle(parcelle).ContinueWith((x) => ClearLoading());
                    
        //    //    }
        //    //}
        //}
        public void CloseVM()
        {
            MessageBrokerImpl.Instance.Unsubscribe<MessageEventArgs>(PayloadMessage);
        }


        private async Task asyncChangeStateParcelle(ParcelleModel parcelle)
        {
            if (parcelle == null)
                return;
            if (!await ServiceCampagne.asyncOpenParcelleCampagne(parcelle, int.Parse(DateCampagne)))
            {
                parcelle.inCampagne = parcelle.inCampagne ? false : true;
                ErrorNotif($"Une erreur s'est produite, pour l'ouverture de la parcelle {parcelle.nameParcelle} au comptage.");
            }else
                isDirty = true;
               
        }

        private async Task asyncLoadParcelles(string dateCp)
        {
            ShowLoading($"Synchronisation Instagrappe des parcelles pour la campagne {DateCampagne}");

            ParcelleModels = await ServiceCampagne.asyncLoadYearCampagne(dateCp);

            ClearLoading(); 
        }
    }
}
