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
        public RelayCommand OpenCampagneCommand { get; set; }
        public RelayCommand RetourCommand { get; set; }

        public RelayCommand RefreshCommand { get; set; }

        public ParcelleVM()
        {

            RetourCommand = new RelayCommand(() => { Message.Notification("ParcelleVM", "RETOUR"); });
            RefreshCommand = new RelayCommand(async () =>
            {
                ShowLoading($"Chargement de la campagne {DateCampagne}");
                await asyncLoadParcelles(DateCampagne).ContinueWith((x) => { ClearLoading(); });
            });

            OpenCampagneCommand = new RelayCommand(async() => {
                ShowLoading($"Synchronisation Instagraape des parcelles pour la campagne {DateCampagne}");
                await ServiceCampagne.asyncOpenParcellesCampagne(ParcelleModels.ToList(), int.Parse(DateCampagne)).ContinueWith((x) => { ClearLoading();});
            });
            Message.Notify += Message_Notify;
        }

        private void Message_Notify(object? sender, MessageEventArgs e)
        {
            if (e.Sender == "CHANGEDATE")
            {
                if ((e.Data is string dateCp) && !string.IsNullOrEmpty(dateCp))
                {
                    ShowLoading($"Chargement de la parcelle {dateCp}");
                    asyncLoadParcelles(dateCp).ContinueWith((x) => { ClearLoading(); });
                }
            }
        }

        public void CloseVM()
        {
            Message.Notify -= Message_Notify;
        }
        private async Task asyncLoadParcelles(string dateCp)
        {

            ParcelleModels = await ServiceCampagne.asyncLoadYearCampagne(dateCp);
        }
    }
}
