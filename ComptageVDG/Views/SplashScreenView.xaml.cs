using ComptageVDG.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ComptageVDG.Views
{
    /// <summary>
    /// Logique d'interaction pour SplashScreenView.xaml
    /// </summary>
    public partial class SplashScreenView : Window
    {
       
        public SplashScreenView()
        {
            InitializeComponent();
        }




        private async void Timer_Tick(object sender, EventArgs e)
        {
            DispatcherTimer timer = (DispatcherTimer)sender;
            timer.Stop();

            var splashVm = ((SplashScreenVM)DataContext);          
            
            if (await splashVm.loadApplication())
            {
                // Charge la fenêtre principale
                MainView mainWindow = new MainView();
                mainWindow.Show();
            }
            else if (splashVm.lastErreur == "Vous n'avez pas de fichier ini." || splashVm.lastErreur == "La connexion a échoué.")
            {
                // Charge la saisi url showmodal
                DialogParameter dialogParameter = new DialogParameter();
                dialogParameter.ShowDialog();

                if (await splashVm.loadApplication())
                {
                    // Charge la fenêtre principale
                    MainView mainWindow = new MainView();
                    mainWindow.Show();
                }
                else               
                    MessageBox.Show(splashVm.lastErreur);               
            }
            else
               MessageBox.Show(splashVm.lastErreur);
          
            // Fermer l'écran de démarrage
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Add a timer to show the main window after a certain amount of time
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Change to your desired interval
            timer.Tick += Timer_Tick;
            timer.Start();
            //var splashVm = ((SplashScreenVM)DataContext);

            //if (splashVm.loadApplication())
            //{
            //    // Charge la fenêtre principale
            //    MainView mainWindow = new MainView();
            //    mainWindow.Show();
            //}
            //else if (splashVm.lastErreur == "Vous n'avez pas de fichier ini.")
            //{
            //    // Charge la saisi url showmodal
            //    DialogParameter dialogParameter = new DialogParameter();
            //    dialogParameter.ShowDialog();

            //    if (splashVm.loadApplication())
            //    {
            //        // Charge la fenêtre principale
            //        MainView mainWindow = new MainView();
            //        mainWindow.Show();
            //    }
            //}
            //else
            //{
            //    MessageBox.Show(splashVm.lastErreur);
            //}

            //this.Close();
        }
    }
}
