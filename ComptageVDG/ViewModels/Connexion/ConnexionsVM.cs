using ComptageVDG.Helpers;
using Infragistics.Controls.Menus.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ComptageVDG.ViewModels.Connexion
{
    public class ConnexionsVM:BaseViewModel,IDisposable
    {
        private string _url=string.Empty;
        
        public event EventHandler Close;
        public string Url { get => _url; set => SetProperty(ref _url, value);}
        
        private string _username;
        public string Username { get => _username; set => SetProperty(ref _username, value); }

        public string GroupAccess { get; set ; }
        
        public ConnexionsVM()
        {
           
            if (!string.IsNullOrEmpty(LoadApp.IniModel?.UrlService))
            {
                Url = LoadApp.IniModel.UrlService;
                Username = LoadApp.IniModel.UserName;
                GroupAccess =(LoadApp.IniModel.Credentials !=null ? string.Join(Environment.NewLine, LoadApp.IniModel.Credentials) : string.Empty);
            }               
            
        }

        private void Message_Notify(object? sender, Helpers.MessageEventArgs e)
        {
            if(e.Sender == nameof(ConnexionsVM) && e.PropertyName == "Url")
            {
               
                //if (string.IsNullOrEmpty(e.Data?.ToString()))
                //{
                //     //MessageBox.Show("La chaine de connexion est vide.");
                //    return;
                //}
                
                

            }
        }

        private RelayCommand testConnexionCommand;
        public ICommand TestConnexionCommand => testConnexionCommand ??= new RelayCommand(TestConnexion);
        private void TestConnexion()
        {
            if(!saveSettings())
                MessageBox.Show("Impossible de sauvegarder la configuration de l'applicaiton.");

            if (controlAccessConnexion())
            {
                MessageBox.Show("Connexion Reussi.");
                raiseCloseEvent();
            }
               

        }

        public void Dispose()
        {
           
        }



        private bool controlAccessConnexion()
        {
            try
            {
                if (string.IsNullOrEmpty(LoadApp.IniModel?.UrlService))
                {
                    MessageBox.Show("Aucun accés aux données est paramétré.");
                    Connexion!.Instance = null;
                    return false;
                }
                else
                {
                    if (Connexion!.CreateAccess(LoadApp.IniModel.UrlService))
                        ServiceCampagne = Connexion!.ServiceCampagne!;
                }

                if (Connexion!.Instance == null || (!Connexion!.Instance!.Open() && !Connexion.Instance.IsConnected))
                {
                    MessageBox.Show("Impossible d'accéder aux données.");
                    return false;
                }

                return true;
            }
            catch
            {
                return false;   
            }
        }

        private bool saveSettings()
        {
            try
            {
                if (LoadApp.IniModel == null)
                    LoadApp.IniModel = new Models.FileIniModel();

                LoadApp.IniModel.UrlService = Url;

                LoadApp.WriteIni();
                return true;    
            }
            catch
            {
                return false;
            }
        }

        private void raiseCloseEvent()
        {
            Close.Invoke(this, new EventArgs());
        }
    }
}
