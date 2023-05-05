using ComptageVDG.Converters;
using ComptageVDG.Helpers;
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

namespace ComptageVDG.ViewModels
{
    internal class MainVm:BaseViewModel
    {
        private bool _isCampagne;
        public bool isCampagne { get => _isCampagne; set=> SetProperty(ref _isCampagne, value); }

        private bool _isPeriode;
        public bool isPeriode { get => _isPeriode; set => SetProperty(ref _isPeriode, value); }

        private bool _isParcelle;
        public bool isParcelle { get => _isParcelle; set => SetProperty(ref _isParcelle, value); }


        private CampagneVM campagneVM;
        public CampagneVM CampagneVM { get => campagneVM; set=>SetProperty(ref campagneVM, value); }

        private PeriodeVM periodeVM;
        public  PeriodeVM PeriodeVM { get => periodeVM; set => SetProperty(ref periodeVM, value); }

        private ParcelleVM parcelleVM;
        public ParcelleVM ParcelleVM { get => parcelleVM; set => SetProperty(ref parcelleVM, value); }  



        public RelayCommand OpenDialogConnexionCommand { get; set; }
        public RelayCommand PeriodeCommand { get; set; }
        public RelayCommand DeclarationCommand { get; set; }

        public event EventHandler LoadUC;

        public MainVm()
        {

            Message.Notify += Message_Notify;

            //isCampagne = true;
            OpenDialogConnexionCommand = new RelayCommand( ()=>{               
                DialogParameter dialogParameter = new DialogParameter();
                dialogParameter.ShowDialog();
            });

            DeclarationCommand = new RelayCommand(() => { toggleView("Parcelle"); });
            PeriodeCommand = new RelayCommand(() => { toggleView("Periode"); });

            LoadDicoCampagne(true).GetAwaiter().OnCompleted(() => {
                //CampagneVM = new CampagneVM();
                //ParcelleVM = new ParcelleVM();
                //PeriodeVM = new PeriodeVM();
                toggleView("Campagne");
            });         

            Message.Notify += Message_Notify;

            
        }

        public void CloseVM()
        {
            Message.Notify -= Message_Notify;
        }


        public async Task LoadDicoCampagne(bool firstLoad = false)
        {            
            var dico = await ServiceCampagne.asyncDicoCampagne();

            if (dico != null && dico.Count > 0)
            {
                ListeCampagne = dico;
                DateCampagne = dico.First().Key;
            }
            else
            {
                ListeCampagne = ComboDictionary.ComboYears;
                DateCampagne = ComboDictionary.ComboYears.First().Key;
            }             
        }

        private void Message_Notify(object? sender, MessageEventArgs e)
        {
            if( e.Data is string retour && retour == "RETOUR")
            {
                toggleView("Campagne");
            }
        }

        private void toggleView(string View)
        {

            if (PeriodeVM !=null && PeriodeVM.isDirty)
            {
                MessageBox.Show($"Attention la periode de campagne {PeriodeVM.Year} a été modifiée.{Environment.NewLine}Enregistrer la modification ou actualiser la periode en cours.");
                return;
            }

            switch (View)
            {
                case "Campagne":
                    isCampagne = true;
                    isParcelle = false;
                    isPeriode = false;
                    
                    LoadUC?.Invoke(typeof(CampagneView).FullName, EventArgs.Empty );
                   // Message.Notification("CAMPAGNE", DateCampagne);
                    break;
                case "Periode":
                    isCampagne = false;
                    isParcelle = false;
                    isPeriode = true;
                    LoadUC?.Invoke(typeof(PeriodeView).FullName, EventArgs.Empty);
                    // Message.Notification("PERIODE", DateCampagne);
                    break;
                case "Parcelle":
                    isCampagne = false;
                    isParcelle = true;
                    isPeriode = false;
                    //Message.Notification("PARCELLE", DateCampagne);
                    LoadUC?.Invoke(typeof(ParcelleView).FullName,EventArgs.Empty);
                    break;
                default:
                    isCampagne = true;
                    isParcelle = false;
                    isPeriode = false;
                    break;
            }

            Message.Notification("DATECAMPAGNE", DateCampagne);
        }

    }
}
