using ComptageVDG.Helpers;
using ComptageVDG.Helpers.Interfaces;
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
                if(datedebGlomerule > datefinGlomerule && datedebGlomerule.HasValue && !datefinGlomerule.HasValue)
                    DatefinGlomerule = datedebGlomerule!.Value.AddDays(7);
            } }
        public DateTime? DatefinGlomerule { get => datefinGlomerule; set
            {
                SetProperty(ref datefinGlomerule, value);
                controleDate(datefinGlomerule, Glomerule, false);
                if (datedebGlomerule > datefinGlomerule && datedebGlomerule.HasValue && !datefinGlomerule.HasValue)
                    DatefinGlomerule = DateTime.Today;
            }
        }
        public DateTime? DatedebPerforation { get => datedebPerforation; set
            {
                SetProperty(ref datedebPerforation, value);
                controleDate(datedebPerforation, Perforation, true);
                if (DatedebPerforation > DatefinPerforation && datedebPerforation.HasValue && !DatefinPerforation.HasValue)
                    DatefinPerforation = datedebPerforation!.Value.AddDays(7);
            }
        }
        public DateTime? DatefinPerforation { get => datefinPerforation; set
            {
                SetProperty(ref datefinPerforation, value);
                controleDate(datefinPerforation!, Perforation, false);
                if (DatedebPerforation > DatefinPerforation && datedebPerforation.HasValue && !DatefinPerforation.HasValue)
                    DatefinGlomerule = DateTime.Today;
            }
        }
        public DateTime? DatedebPerforation2
        {
            get => datedebPerforation2; set
            {
                SetProperty(ref datedebPerforation2, value);
                controleDate(datedebPerforation2, Perforation, true);
                if (DatedebPerforation2 > DatefinPerforation2 && datedebPerforation2.HasValue && !DatefinPerforation2.HasValue)
                    DatefinPerforation2 = DatedebPerforation2!.Value.AddDays(7);

            }
        }
        public DateTime? DatefinPerforation2 { get => datefinPerforation2; set
            {
                SetProperty(ref datefinPerforation2, value);
                controleDate(datefinPerforation2,Perforation2,false);
                if (DatedebPerforation2 > DatefinPerforation2 && datedebPerforation2.HasValue && !DatefinPerforation2.HasValue)
                    DatefinPerforation2 = DateTime.Today;
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
        //public RelayCommand AjoutCommand { get; set; }
        public RelayCommand<string> SynchroPeriodeInstaGrappeCommand { get; set; }

        public PeriodeVM()
        {

            MessageBrokerImpl.Instance.Subscribe<MessageEventArgs>(Payload);
            isDirty = false;

            RetourCommand = new RelayCommand(() => {
                if (isDirty)
                    WarningNotif($"Attention la période de campagne {Year} a été modifiée.{Environment.NewLine}Enregistrer la modification ou actualiser la periode en cours.");
                else if(string.IsNullOrEmpty(DateCampagne))
                    WarningNotif($"Attention aucune période de campagne n'a été créée.{Environment.NewLine}Veuillez en créer une.");
                else
                    MessageBrokerImpl.Instance.Publish(this, MessageBrokerImpl.Notification("PeriodeVM", "RETOUR"));
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

            //AjoutCommand = new RelayCommand( () =>
            //{
            //    isDirty = true;
            //    Year = DateTime.Today.ToString("yyyy");
            //    //DatedebGlomerule = DatedebPerforation = DatedebPerforation2 = DateTime.Today;
            //    //DatefinGlomerule = DatefinPerforation = DatefinPerforation2 = DateTime.Today;                
            //    IsReadGlomerule = IsReadPerforation = IsReadPerforation2 = false;
            //    PeriodeModels = new ObservableCollection<PeriodeModel>();
            //    if (!ListeCampagne.ContainsKey(DateTime.Today.ToString("yyyy")))
            //    {

            //        SaveCommand.Execute(null);
            //        ListeCampagne  = ServiceCampagne.asyncDicoCampagne().GetAwaiter().GetResult();
            //        if(ListeCampagne != null && ListeCampagne.ContainsKey(Year))
            //        {
            //            DateCampagne = Year;
            //        }
            //    }
            //}, () => ListeCampagne != null && !ListeCampagne.ContainsKey(DateTime.Today.ToString("yyyy")));


            SynchroPeriodeInstaGrappeCommand = new RelayCommand<string>(SynchroPeriodeInstaGrappeCommandExecute, SynchroPeriodeInstaGrappeCommandCanExecute);
            ShowLoading($"Chargement de la période {DateCampagne}");
            asyncLoadPeriode(DateCampagne).GetAwaiter().OnCompleted(() => { Year = DateCampagne; ClearLoading(); });
           
        }

        private void Payload(MessagePayload<MessageEventArgs> obj)
        {
            if (obj.What.Sender == "CHANGEDATE")
            {
                if ((obj.What.Data is string dateCp) && !string.IsNullOrEmpty(dateCp))
                {
                    ShowLoading($"Chargement de la période {dateCp}");
                    asyncLoadPeriode(dateCp).GetAwaiter().OnCompleted(() => {
                        ClearLoading();
                        SynchroPeriodeInstaGrappeCommand.RaiseCanExecuteChanged();
                    });
                }
            }

            if (obj.What.Sender == "SYNCHRO")
            {
                SynchroPeriodeInstaGrappeCommandExecute(String.Empty);
            }
        }

        public void CloseVM()
        {
            MessageBrokerImpl.Instance.Unsubscribe<MessageEventArgs>(Payload);
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
            if (isDirty)
            {
                WarningNotif("Une modification est en cours vous devez enregistrer ou actualiser pour annuler les modification afin de lancer une synchro instagrappe.");
                return;
            }

            ShowLoading($"Mise à jour période campagne {Year}{(!string.IsNullOrEmpty(obj)? $" pour les {obj.ToLower()}" : "")}.");
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
                        if (glome == null)
                        {
                            ErrorNotif($"La période pour les glomérules n'existe pas en base. Voir avec votre administrateur systéme.");
                            return;
                        }
                        else
                        {
                           
                            if (glome?.DateDebut != DatedebGlomerule && DatedebGlomerule > DateTime.Today)
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
                        
                    }
                    else if( DatedebGlomerule == null || DatefinGlomerule == null)
                    {
                        var glome = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Glomerule.ToLower());
                        if(glome != null)
                        {
                            glome.DateDebut = DatedebGlomerule;
                            glome.DateFin = DatefinGlomerule;
                            isDirty = true;
                        }
                            
                    }
                    else
                        message += "La déclaration de période pour le comptage des glomérules n'est pas conforme." + Environment.NewLine;



                    if (DatedebPerforation != null && DatefinPerforation != null)
                    {
                        var perfo = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Perforation.ToLower());
                        if (perfo == null)
                        {
                            ErrorNotif($"La période pour le comptage des perforations n'existe pas en base. Voir avec votre administrateur systéme.");
                            return;
                        }
                        else {
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
                        
                    }
                    else if (DatedebPerforation == null || DatefinPerforation == null)
                    {
                        var perfo = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Perforation.ToLower());
                        if (perfo != null)
                        {
                            perfo.DateDebut = DatedebPerforation;
                            perfo.DateFin = DatefinPerforation;
                            isDirty = true;
                        }

                    }
                    else
                        message += "La déclaration de période pour le premier comptage des perforations n'est pas conforme." + Environment.NewLine;

                    if (DatedebPerforation2 != null && DatefinPerforation2 != null)
                    {
                        var perfo2= PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Perforation2.ToLower());
                        if (perfo2 == null)
                        {
                            ErrorNotif($"La période pour le comptage des perforations 2 n'existe pas en base. Voir avec votre administrateur systéme.");
                            return;
                        }
                        else
                        {
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
                            
                    }
                    else if (DatedebPerforation2 == null || DatefinPerforation2 == null)
                    {
                        var perfo2 = PeriodeModels.FirstOrDefault((x) => x.Name.ToLower() == Perforation2.ToLower());
                        if (perfo2 != null)
                        {
                            perfo2.DateDebut = DatedebPerforation2;
                            perfo2.DateFin = DatefinPerforation2;
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
                    if ((DatedebGlomerule == null && DatefinGlomerule == null) ||( DatedebGlomerule != null && DatefinGlomerule != null && DatedebGlomerule < DatefinGlomerule))
                    {
                        Glome = new PeriodeModel() { DateDebut = DatedebGlomerule, DateFin = DatefinGlomerule, Name = Glomerule, Year = int.Parse(this.Year) };
                        PeriodeModels!.Add(Glome);
                        isDirty = true; 
                    } else
                        message += "La déclaration de période pour le comptage des glomérules n'est pas conforme." + Environment.NewLine;
                    if((DatedebPerforation == null && DatefinPerforation == null) || (DatedebPerforation != null && DatefinPerforation != null))
                    {
                        Perfo = new PeriodeModel() { Year = int.Parse(this.Year), DateDebut = DatedebPerforation, DateFin=DatefinPerforation, Name = Perforation };
                        PeriodeModels!.Add(Perfo);
                        isDirty = true;
                    }
                    else
                        message += "La déclaration de période pour le premier comptage des perforations n'est pas conforme." + Environment.NewLine;
                    if ((DatedebPerforation2 == null && DatefinPerforation2 == null)||(DatedebPerforation2 != null && DatefinPerforation2 != null))
                    {
                        Perfo2 = new PeriodeModel() { Name = Perforation2, DateDebut=DatedebPerforation2, DateFin=DatefinPerforation2, Year=int.Parse(Year) };
                        PeriodeModels!.Add(Perfo2);
                        isDirty = true;
                    }
                    else 
                        message += "La déclaration de période pour le deuxiéme comptage des perforations n'est pas conforme." + Environment.NewLine;
                }

                if (string.IsNullOrEmpty(message) && PeriodeModels!.Count == 3 && isDirty)
                {

                    if (DatedebGlomerule > DatefinGlomerule || (datedebGlomerule.HasValue && !datefinGlomerule.HasValue))
                    {
                        WarningNotif($"La date de fin de comptage des glomérules ne peut être inferieure à la date de début.");
                        return;
                    }

                    if (DatedebPerforation > DatefinPerforation || (datedebPerforation.HasValue && !DatefinPerforation.HasValue))
                    {
                        WarningNotif($"La date de fin du premier comptage des perforations ne peut être inferieure à la date de début.");
                        return;
                    }

                    if (DatedebPerforation > DatefinPerforation || (datedebPerforation2.HasValue && !datefinPerforation2.HasValue))
                    {
                        WarningNotif($"La date de fin du second comptage des perforations ne peut être inferieure à la date de début.");
                        return;
                    }


                    var success = await ServiceCampagne.asyncSetPeriodeCampagne(PeriodeModels, Year);
                    if (!success)
                    {
                        message = "Une erreure s'est produite lors de la sauvegarde.";    
                        if(PeriodeModels?.FirstOrDefault()!.Id_periode > 0)
                        {
                            ShowLoading($"Chargement de la période {year}");
                            await  asyncLoadPeriode(Year).ContinueWith((x)=>ClearLoading());
                        }
                        else
                            PeriodeModels = new ObservableCollection<PeriodeModel>();

                        ErrorNotif(message);
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
                            ShowLoading($"Chargement de la période {year}");
                            await asyncLoadPeriode(Year).ContinueWith((x) => ClearLoading());
                        }
                        InfoNotif($"Sauvegarde période {Year} effectuée.");
                    }
                }
                else
                {
                    WarningNotif(message);  
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
            //AjoutCommand.RaiseCanExecuteChanged();
            isDirty = false;

            if (int.Parse(dateCp) < DateAndTime.Year(DateTime.Today))
                IsReadGlomerule = IsReadPerforation = IsReadPerforation2 = true;
            else
                IsReadGlomerule = IsReadPerforation = IsReadPerforation2 = false;
           
        }
    }
}
