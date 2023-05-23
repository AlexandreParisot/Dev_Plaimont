using ComptageVDG.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComptageVDG.Views
{
    /// <summary>
    /// Logique d'interaction pour CampagneView.xaml
    /// </summary>
    public partial class CampagneView : UserControl
    {
        public CampagneView()
        {
            InitializeComponent();
            this.DataContext = new CampagneVM(); //(CampagneVM) this.Resources["viewModel"];
            if (((CampagneVM)this.DataContext).ParcelleModelsinCampagne == null)
                ((CampagneVM)this.DataContext).RefreshCommand.Execute(this);
            else
                ((CampagneVM)this.DataContext).ColorCompteur(((CampagneVM)this.DataContext).DateCampagne);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CampagneVM Campage)
            {
                Campage.CloseVM();
                DataContext = null;
            }
        }
    }
}
