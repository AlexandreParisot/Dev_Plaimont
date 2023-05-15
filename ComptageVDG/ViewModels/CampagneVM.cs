using ComptageVDG.Helpers;
using ComptageVDG.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ComptageVDG.ViewModels
{
    public class CampagneVM:BaseViewModel
    {



        //private ObservableCollection<ParcelleModel> parcelleModels;
        //public ObservableCollection<ParcelleModel> ParcelleModels { get => parcelleModels; set => SetProperty(ref parcelleModels , value); }

        
        public string LastSynchro { get => _lastSynchro; set=> SetProperty(ref _lastSynchro,value); }


       public RelayCommand RefreshCommand { get; set; }



        public  CampagneVM()
        {
            Message.Notify += Message_Notify;
            RefreshCommand = new RelayCommand(async() => {
                ShowLoading($"Chargement de la campagne {DateCampagne}");               
                await asyncLoadParcelles(DateCampagne).ContinueWith((x) => ClearLoading());
                await asyncGetLastSynchro();                
            });
        }

        public void CloseVM()
        {
            Message.Notify -= Message_Notify;
        }


        private void Message_Notify(object? sender, MessageEventArgs e)
        {
            
            if (e.Sender == "CHANGEDATE")
            {
                if ((e.Data is string dateCp) && !string.IsNullOrEmpty(dateCp))
                {
                    ShowLoading($"Chargement de la campagne {dateCp}");
                    LoadParcelles(dateCp);
                    asyncGetLastSynchro().GetAwaiter();
                    ClearLoading();
                }
            }
        }



        private async Task asyncLoadParcelles(string dateCp)
        {
            if (string.IsNullOrEmpty(dateCp))
                return;

            ParcelleModelsinCampagne = await ServiceCampagne.asyncLoadYearInCampagne(dateCp);
            
        }

        private async Task asyncGetLastSynchro()
        {
            LastSynchro = await ServiceCampagne.getTimeSynchroCampagne();
        }


        private  void LoadParcelles(string dateCp)
        {            
            if (string.IsNullOrEmpty(dateCp))
                return;

          Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => {
              ParcelleModelsinCampagne = ServiceCampagne.asyncLoadYearInCampagne(DateCampagne).GetAwaiter().GetResult();
               asyncGetLastSynchro().GetAwaiter();
          }));
               return;
        }



    }
}
