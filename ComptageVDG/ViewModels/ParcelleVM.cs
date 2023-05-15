using ComptageVDG.Helpers;
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


        private ParcelleModel _currentParcelle;
        public ParcelleModel CurrentParcelle { get=> _currentParcelle; set{ 
                SetProperty(ref _currentParcelle, value);
                //if (_currentParcelle?.inCampagne)
                //    Message.Notification("CHANGESTATE", CurrentParcelle);
            } }

       // public RelayCommand OpenCampagneCommand { get; set; }
        public RelayCommand RetourCommand { get; set; }

        public RelayCommand RefreshCommand { get; set; }

        public RelayCommand<ParcelleModel> ChangeStateCommand { get; set; }

        public ParcelleVM()
        {

            RetourCommand = new RelayCommand(() => { Message.Notification("ParcelleVM", "RETOUR"); });
            RefreshCommand = new RelayCommand(async () =>
            {
                ShowLoading($"Chargement de la campagne {DateCampagne}");
                await asyncLoadParcelles(DateCampagne).ContinueWith((x) => { ClearLoading(); });
            });

            ChangeStateCommand = new RelayCommand<ParcelleModel>(ChangeStateCommandExcute);

            //OpenCampagneCommand = new RelayCommand(async() => {
            //    ShowLoading($"Synchronisation Instagrappe des parcelles pour la campagne {DateCampagne}");
            //    await ServiceCampagne.asyncOpenParcellesCampagne(ParcelleModels.ToList(), int.Parse(DateCampagne)).ContinueWith((x) => { ClearLoading();});
            //});
            Message.Notify += Message_Notify;
        }

        private async void ChangeStateCommandExcute(ParcelleModel obj)
        {
            await asyncChangeStateParcelle(obj);

        }

        private void Message_Notify(object? sender, MessageEventArgs e)
        {
            if (e.Sender == "CHANGEDATE")
            {
                if ((e.Data is string dateCp) && !string.IsNullOrEmpty(dateCp))
                {
                    ShowLoading($"Chargement de la campagne {dateCp}");
                    asyncLoadParcelles(dateCp).ContinueWith((x) => { ClearLoading(); });
                }
            }

            if(e.Sender == "CHANGESTATE")
            {
                if ((e.Data is ParcelleModel parcelle) && parcelle != null)
                {
                    ShowLoading("Changement état parcelle ...");
                    asyncChangeStateParcelle(parcelle).ContinueWith((x) => ClearLoading());
                }
            }
        }
        public void CloseVM()
        {
            Message.Notify -= Message_Notify;
        }


        private async Task asyncChangeStateParcelle(ParcelleModel parcelle)
        {
            if (parcelle == null)
                return;
            await ServiceCampagne.asyncOpenParcelleCampagne(parcelle, int.Parse(DateCampagne));
        }

        private async Task asyncLoadParcelles(string dateCp)
        {

            ParcelleModels = await ServiceCampagne.asyncLoadYearCampagne(dateCp);
        }
    }
}
