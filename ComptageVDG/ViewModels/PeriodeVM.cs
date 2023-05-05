using ComptageVDG.Helpers;
using ComptageVDG.Models;
using Infragistics.Windows.Editors;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComptageVDG.ViewModels
{
    public class PeriodeVM:BaseViewModel
    {
        
        private const string Glomerule = "Glomerule";
        private const string Perforation = "Perforation";
        private const string Perforation2 = "Perforation2";


        private ObservableCollection<PeriodeModel> periodeModels;

        private DateTime? datedebGlomerule;
        private DateTime? datefinGlomerule;

        private DateTime? datedebPerforation;
        private DateTime? datefinPerforation;

        private DateTime? datedebPerforation2;
        private DateTime? datefinPerforation2;
        private string? year;
        private bool isReadGlomerule;
        private bool isReadPerforation;
        private bool isReadPerforation2;


        public bool isDirty = false;
        public ObservableCollection<PeriodeModel> PeriodeModels { get => periodeModels; set{ periodeModels = value; SynchroPeriodeInstaGrappeCommand.RaiseCanExecuteChanged(); } }

        public DateTime? DatedebGlomerule { get=> datedebGlomerule; set
            { 
                SetProperty(ref datedebGlomerule, value);
                controleDate(datedebGlomerule, Glomerule, true);
                if(DatedebGlomerule > DatefinGlomerule)
                    DatefinGlomerule = DatedebGlomerule.Value.AddDays(7);
            } }
        public DateTime? DatefinGlomerule { get => datefinGlomerule; set
            {
                SetProperty(ref datefinGlomerule, value);
                controleDate(datefinGlomerule, Glomerule, false);
            }
        }
        public DateTime? DatedebPerforation { get => datedebPerforation; set
            {
                SetProperty(ref datedebPerforation, value);
                controleDate(datedebPerforation, Perforation, true);
                if (DatedebPerforation > DatefinPerforation)
                    DatefinPerforation = DatedebPerforation!.Value.AddDays(7);
            }
        }
        public DateTime? DatefinPerforation { get => datefinPerforation; set
            {
                SetProperty(ref datefinPerforation, value);
                controleDate(datefinPerforation!, Perforation, false);  
            }
        }
        public DateTime? DatedebPerforation2
        {
            get => datedebPerforation2; set
            {
                SetProperty(ref datedebPerforation2, value);
                controleDate(datedebPerforation2, Perforation, true);
                if (DatedebPerforation2 > DatefinPerforation2)
                    DatefinPerforation2 = DatedebPerforation2!.Value.AddDays(7);

            }
        }
        public DateTime? DatefinPerforation2 { get => datefinPerforation2; set
            {
                SetProperty(ref datefinPerforation2, value);
                controleDate(datefinPerforation2,Perforation2,false);
            }

        }

        private void controleDate(DateTime? value, string Compteur, bool deb)
        {
            if (!value.HasValue)
            {
                isDirty = true;
                return;
            }

            if (PeriodeModels != null && PeriodeModels.Any())
            {
                var periode = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Compteur.ToLower());
                if (deb)
                {
                    if (periode?.DateDebut != value)
                    {
                        isDirty = true;
                    }
                }
                else
                {
                    if (periode?.DateFin != value)
                    {
                        isDirty = true;
                    }
                }               
            }else
                isDirty = true;
        }


        public string? Year { get => year; set { SetProperty(ref year, value); RefreshCommand.RaiseCanExecuteChanged(); SaveCommand.RaiseCanExecuteChanged(); } }
        public bool IsReadGlomerule { get => isReadGlomerule; set=> SetProperty(ref isReadGlomerule, value); }  
        public bool IsReadPerforation { get => isReadPerforation; set => SetProperty(ref isReadPerforation, value); }
        public bool IsReadPerforation2 { get => isReadPerforation2; set => SetProperty(ref isReadPerforation2, value);  }



        public RelayCommand RetourCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }
        public RelayCommand SaveCommand { get; set; }
        public RelayCommand AjoutCommand { get; set; }
        public RelayCommand<string> SynchroPeriodeInstaGrappeCommand { get; set; }

        public PeriodeVM()
        {

            Message.Notify += Message_Notify;
            isDirty = false;

            RetourCommand = new RelayCommand(() => {
                if (isDirty)
                    WarningNotif($"Attention la periode de campagne {Year} a été modifiée.{Environment.NewLine}Enregistrer la modification ou actualiser la periode en cours.");
                else
                    Message.Notification("PeriodeVM", "RETOUR"); 
            });

            RefreshCommand = new RelayCommand(()=>
            {
                ShowLoading($"Chargement de la période {DateCampagne}");
                 asyncLoadPeriode(DateCampagne).GetAwaiter().OnCompleted(() => { ClearLoading(); });
            },()=> !string.IsNullOrEmpty(Year));

            SaveCommand = new RelayCommand(async() =>
            {
                ShowLoading($"Sauvegarde de la période {Year} dans Lavilog.");
                await asyncSavePeriode().ContinueWith((x) => { ClearLoading(); }); 
            }, ()=> !string.IsNullOrEmpty(Year) && Year == DateTime.Today.ToString("yyyy"));

            AjoutCommand = new RelayCommand( () =>
            {
                isDirty = true;
                Year = DateTime.Today.ToString("yyyy");
                DatedebGlomerule = DatedebPerforation = DatedebPerforation2 = DateTime.Today;
                DatefinGlomerule = DatefinPerforation = DatefinPerforation2 = DateTime.Today;                
                IsReadGlomerule = IsReadPerforation = IsReadPerforation2 = false;
                PeriodeModels = new ObservableCollection<PeriodeModel>();                
            }, () => ListeCampagne != null && !ListeCampagne.ContainsKey(DateTime.Today.ToString("yyyy")));


            SynchroPeriodeInstaGrappeCommand = new RelayCommand<string>(SynchroPeriodeInstaGrappeCommandExecute, SynchroPeriodeInstaGrappeCommandCanExecute);

            asyncLoadPeriode(DateCampagne).GetAwaiter().OnCompleted(() => { Year = DateCampagne; });
           
        }
                

        public void CloseVM()
        {
            Message.Notify -= Message_Notify;
        }


        private void Message_Notify(object? sender, MessageEventArgs e)
        {
            if (e.Sender == "CHANGEDATE" )
            {
                if ((e.Data is string dateCp) && !string.IsNullOrEmpty(dateCp))
                {
                    ShowLoading($"Chargement de la période {dateCp}");
                    asyncLoadPeriode(dateCp).GetAwaiter().OnCompleted(() => {
                        ClearLoading();
                        SynchroPeriodeInstaGrappeCommand.RaiseCanExecuteChanged();
                    });                    
                }
            }
        }

        private bool SynchroPeriodeInstaGrappeCommandCanExecute(string obj)
        {
            try
            {
                if (PeriodeModels != null && PeriodeModels.Any())
                {
                    var periode = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == obj.ToLower());
                    if (periode?.DateDebut != null && periode?.DateFin != null && DateTime.Today.ToString("yyyy") == Year && DateTime.Now <= periode?.DateFin)
                        return true;
                }
                return false;
            }
            catch
            { return false; }
        }

        private async void SynchroPeriodeInstaGrappeCommandExecute(string obj)
        {
            ShowLoading($"Mise à jour période campagne {Year} pour les {obj.ToLower()} .");
            //il faut la liste de parcelle autorisé + la periode
            //await ServiceCampagne.OpenParcelle(null).with;
            await Task.Delay(TimeSpan.FromSeconds(3));
            ClearLoading();
        }
        private async Task asyncSavePeriode()
        {
            //lasauvegarde se fait que sur l'année en cours
            string message = String.Empty;
            isDirty = false;
            try
            {
                //update
                if (PeriodeModels!= null && PeriodeModels.Any())
                {
                    //Control des dates  
                    if (DatedebGlomerule != null && DatefinGlomerule != null && DatedebGlomerule < DatefinGlomerule)
                    {
                        var glome = PeriodeModels.FirstOrDefault((x)=> x.Name.ToLower() == Glomerule.ToLower());
                        if(glome?.DateDebut != DatedebGlomerule && DatedebGlomerule > DateTime.Today)
                        {
                            glome!.DateDebut = DatedebGlomerule.Value; 
                            isDirty = true;
                        }
                        if (glome?.DateFin != DatefinGlomerule && DatefinGlomerule > DateTime.Today)
                        {
                            glome!.DateFin = DatefinGlomerule.Value;
                            isDirty = true;
                        }
                    }
                    else
                        message += "La déclaration de période pour le comptage des glomérules n'est pas conforme." + Environment.NewLine;


                    if (DatedebPerforation != null && DatefinPerforation != null)
                    {
                        var perfo = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Perforation.ToLower());
                        if (perfo?.DateDebut != DatedebPerforation && DatedebPerforation > DateTime.Today)
                        {
                            perfo!.DateDebut = DatedebPerforation.Value;
                            isDirty = true;
                        }                           
                        if (perfo?.DateFin != DatefinPerforation && DatefinPerforation > DateTime.Today)
                        {
                            perfo!.DateFin = DatefinPerforation.Value;
                            isDirty = true;
                        }
                    }
                    else
                        message += "La déclaration de période pour le premier comptage des perforations n'est pas conforme." + Environment.NewLine;

                    if (DatedebPerforation2 != null && DatefinPerforation2 != null)
                    {
                        var perfo2= PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Perforation2.ToLower());
                        if (perfo2?.DateDebut != DatedebPerforation2 && DatedebPerforation2 > DateTime.Today)
                        {
                            perfo2!.DateDebut = DatedebPerforation2.Value;
                            isDirty = true;
                        }
                            
                        if (perfo2?.DateFin != DatefinPerforation2 && DatefinPerforation2 > DateTime.Today)
                        {
                            perfo2!.DateFin = DatefinPerforation2.Value;
                            isDirty = true;
                        }
                            
                    }
                    else
                        message += "La déclaration de période pour le deuxiéme comptage des perforations n'est pas conforme." + Environment.NewLine;
                    
                }
                //insert
                else
                {
                    PeriodeModel? Glome = null;
                    PeriodeModel? Perfo = null;
                    PeriodeModel? Perfo2 = null;
                    if (DatedebGlomerule != null && DatefinGlomerule != null && DatedebGlomerule < DatefinGlomerule)
                    {
                        Glome = new PeriodeModel() { DateDebut = DatedebGlomerule.Value!, DateFin = DatefinGlomerule.Value!, Name = Glomerule, Year = int.Parse(this.Year) };
                        PeriodeModels!.Add(Glome);
                        isDirty = true; 
                    } else
                        message += "La déclaration de période pour le comptage des glomérules n'est pas conforme." + Environment.NewLine;
                    if(DatedebPerforation != null && DatefinPerforation != null)
                    {
                        Perfo = new PeriodeModel() { Year = int.Parse(this.Year), DateDebut = DatedebPerforation.Value, DateFin=DatefinPerforation.Value, Name = Perforation };
                        PeriodeModels!.Add(Perfo);
                        isDirty = true;
                    }
                    else
                        message += "La déclaration de période pour le premier comptage des perforations n'est pas conforme." + Environment.NewLine;
                    if (DatedebPerforation2 != null && DatefinPerforation2 != null)
                    {
                        Perfo2 = new PeriodeModel() { Name = Perforation2, DateDebut=DatedebPerforation2.Value, DateFin=DatefinPerforation2.Value, Year=int.Parse(Year) };
                        PeriodeModels!.Add(Perfo2);
                        isDirty = true;
                    }
                    else 
                        message += "La déclaration de période pour le deuxiéme comptage des perforations n'est pas conforme." + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(message) && PeriodeModels!.Count == 3 && isDirty)
                {
                    var success = await ServiceCampagne.asyncSetPeriodeCampagne(PeriodeModels, Year);
                    if (!success)
                    {
                        message = "Une erreure s'est produite lors de la sauvegarde.";
                        //TODO Notification
                        PeriodeModels = new ObservableCollection<PeriodeModel>();
                        ErrorNotif(message);
                        //MessageBox.Show(message,"Vers de grappe",MessageBoxButton.OK,MessageBoxImage.Error);
                    }
                    else
                    {
                        isDirty = false;
                        if (PeriodeModels!.First().Id_periode == 0)
                        {
                            
                            if (!ListeCampagne.ContainsKey(Year))
                            {
                                ListeCampagne.Add(Year, $"Campagne {Year}");
                                DateCampagne = Year;
                            }

                        }

                        InfoNotif($"Sauvegarde période {Year} effectuée.");
                    }
                }
                else
                {
                    //TODO Notification
                    WarningNotif(message);  
                    //MessageBox.Show(message,"Vers de grappe",MessageBoxButton.OK,MessageBoxImage.Warning);
                }
                SynchroPeriodeInstaGrappeCommand.RaiseCanExecuteChanged();
            }
            catch(Exception ex)
            {
                ErrorNotif(ex.Message);
                // MessageBox.Show(ex.Message);
            }
        }


        private async Task asyncLoadPeriode(string dateCp)
        {
            if(string.IsNullOrEmpty(dateCp))
                return;

            PeriodeModels = await ServiceCampagne.asyncGetPeriodeCampagne(dateCp);

            if(PeriodeModels != null && PeriodeModels.Any())
            {
                var period = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Glomerule.ToLower());
                if(period != null)
                {
                    Year = period.Year.ToString();
                    DatedebGlomerule = period.DateDebut;
                    DatefinGlomerule = period.DateFin;
                }
                period = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Perforation.ToLower());
                if(period != null)
                {
                    Year = period.Year.ToString();
                    DatedebPerforation = period.DateDebut;
                    DatefinPerforation = period.DateFin;
                }
                period = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Perforation2.ToLower());
                if (period != null)
                {
                    Year = period.Year.ToString();
                    DatedebPerforation2 = period.DateDebut;
                    DatefinPerforation2 = period.DateFin;
                }               
            }
            else
            {                
                Year = null;
                DatedebGlomerule = DatedebPerforation = DatedebPerforation2 = DatefinGlomerule = DatefinPerforation = DatefinPerforation2 = null;                 
            }

            SynchroPeriodeInstaGrappeCommand.RaiseCanExecuteChanged();
            AjoutCommand.RaiseCanExecuteChanged();
            isDirty = false;

            if (int.Parse(dateCp) < DateAndTime.Year(DateTime.Today))
                IsReadGlomerule = IsReadPerforation = IsReadPerforation2 = true;
            else
                IsReadGlomerule = IsReadPerforation = IsReadPerforation2 = false;
           
        }
    }
}
