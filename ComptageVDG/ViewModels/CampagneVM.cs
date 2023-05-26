using ComptageVDG.Helpers;
using ComptageVDG.Helpers.Interfaces;
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
   
        public string LastSynchro { get => _lastSynchro; set=> SetProperty(ref _lastSynchro,value); }

        private string _colorGlomerule;
        public string ColorGlomerule { get => _colorGlomerule; set => SetProperty(ref _colorGlomerule,value); }

        private string _colorPerforation;
        public string ColorPerforation { get => _colorPerforation; set => SetProperty(ref _colorPerforation, value); }

        private string _colorPerforation2;
        public string ColorPerforation2 { get => _colorPerforation2; set => SetProperty(ref _colorPerforation2, value); }

        public RelayCommand RefreshCommand { get; set; }

        //public delegate void RaiseNotif(Notification Me);
        
        public  CampagneVM()
        {
            
            RefreshCommand = new RelayCommand(async() => {
                ShowLoading($"Chargement de la campagne {DateCampagne}");               
                await asyncLoadParcelles(DateCampagne).ContinueWith(
                    async (x) => { 
                        ClearLoading();
                        await ColorCompteur(DateCampagne);
                        if (ParcelleModelsinCampagne != null && ParcelleModelsinCampagne.Count > 0)
                            InfoNotif($"Nombre de parcelle au comptage : {ParcelleModelsinCampagne.Count}");
                    });
                await asyncGetLastSynchro();                
            });

            MessageBrokerImpl.Instance.Subscribe<MessageEventArgs>(PayloadMessage);
        }

        private void PayloadMessage(MessagePayload<MessageEventArgs> obj)
        {
            if (obj.What.Sender == "CHANGEDATE")
               if ((obj.What.Data is string dateCp) && !string.IsNullOrEmpty(dateCp))
                    LoadParcelles(dateCp);
             
            if(obj.What.Sender == "REFRESH")
                if(obj.What.Data is string dateCp && !string.IsNullOrEmpty(dateCp))
                    LoadParcelles(dateCp);  
        }


        public void CloseVM()
        { 
            MessageBrokerImpl.Instance.Unsubscribe<MessageEventArgs>(PayloadMessage);
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

        public async Task ColorCompteur(string dateCp)
        {
            ColorGlomerule = string.Empty;
            ColorPerforation = string.Empty;
            ColorPerforation2 = string.Empty;
            if (string.IsNullOrEmpty(dateCp))
                return;

            var lstPeriode = await ServiceCampagne.asyncGetPeriodeCampagne(dateCp);
            if(lstPeriode != null && lstPeriode.Count >0)
            {
                var glomerule = lstPeriode.FirstOrDefault(x => x.Name.ToLower() == "glomerule");
                if(glomerule != null)
                {
                    if (glomerule.DateDebut <= DateTime.Today && glomerule.DateFin >= DateTime.Today)
                        ColorGlomerule = "#89f09f"; 
                    else
                        ColorGlomerule = "#f58e87";
                }
                var perfo = lstPeriode.FirstOrDefault(x => x.Name.ToLower() == "perforation");
                if (perfo != null)
                {
                    if (perfo.DateDebut <= DateTime.Today && perfo.DateFin >= DateTime.Today)
                        ColorPerforation = "#89f09f"; 
                    else
                        ColorPerforation = "#f58e87";
                }
                var perfo2 = lstPeriode.FirstOrDefault(x => x.Name.ToLower() == "perforation2");
                if (perfo2 != null)
                {
                    if (perfo2.DateDebut <= DateTime.Today && perfo2.DateFin >= DateTime.Today)
                        ColorPerforation2 = "#89f09f"; 
                    else
                        ColorPerforation2 = "#f58e87";
                }
            }
        }

        private   void LoadParcelles(string dateCp)
        {            
            if (string.IsNullOrEmpty(dateCp))
                return;
           
              Dispatcher.CurrentDispatcher.BeginInvoke(new Action(async () => {
                  ShowLoading($"Chargement de la campagne {dateCp}");
                   await ServiceCampagne.asyncLoadYearInCampagne(dateCp).ContinueWith(
                    async (x) => {
                        ClearLoading();
                        ParcelleModelsinCampagne = x.Result;
                        await ColorCompteur(dateCp);                      
                            
                         if (ParcelleModelsinCampagne != null && ParcelleModelsinCampagne.Count > 0) { }
                            InfoNotif($"Nombre de parcelle au comptage : {ParcelleModelsinCampagne.Count}");
                    });
                  await asyncGetLastSynchro();
              }));
           
        }



    }
}
