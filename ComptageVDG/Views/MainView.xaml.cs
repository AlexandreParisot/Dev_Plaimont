using ComptageVDG.ViewModels;
using Infragistics.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComptageVDG.Views
{
    /// <summary>
    /// Logique d'interaction pour MainVM.xaml
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            if(DataContext != null && DataContext is MainVm mainVm)
            {
                mainVm.LoadUC += MainVm_LoadUC;
                mainVm.LoadMainView();
            }
                
            //ThemeManager.SetTheme(this, new MetroTheme());
        }

        private void MainVm_LoadUC(object? sender, EventArgs e)
        {
            if(sender is string uc)
            {
                LoadUserControl(uc);
            }
        }

        public void DisplayUserControl(UserControl uc)
        {
            // Add new user control to content area
            LayoutRoot.Children.Add(uc);
        }

        private void CloseUserControl()
        {
            // Remove current user control
            LayoutRoot.Children.Clear();
        }


        private bool ShouldLoadUserControl(string controlName)
        {
            bool ret = true;

            // Don't reload a control already loaded.
            if (LayoutRoot.Children.Count > 0)
            {
                if (((UserControl)LayoutRoot.Children[0]).GetType().FullName == controlName)
                {
                    ret = false;
                }
            }

            return ret;
        }

        private void LoadUserControl(string controlName)
        {
            if (ShouldLoadUserControl(controlName))
            {
                Type ucType = null;
                UserControl uc = null;

                // Create a Type from controlName parameter
                ucType = Type.GetType(controlName);
                if (ucType == null)
                {
                    MessageBox.Show("Ce module n'existe pas : " + controlName);
                }
                else
                {
                    // Close current user control in content area
                    CloseUserControl();

                    // Create an instance of this control
                    uc = (UserControl)Activator.CreateInstance(ucType);
                    if (uc != null)
                    {
                        // Display control in content area
                        DisplayUserControl(uc);
                    }
                }
            }           
        }




        private void ProcessMenuCommands(string command)
        {
            switch (command.ToLower())
            {
                case "exit":
                    this.Close();
                    break;
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (DataContext is MainVm main)
                main.CloseVM();
        }
    }
}
