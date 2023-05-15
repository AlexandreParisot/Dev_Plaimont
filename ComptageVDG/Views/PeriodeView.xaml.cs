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
    /// Logique d'interaction pour PeriodeView.xaml
    /// </summary>
    public partial class PeriodeView : UserControl
    {
    
        public PeriodeView()
        {
            InitializeComponent();

            DataContext = new PeriodeVM(); // (PeriodeVM)this.Resources["viewModel"];;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PeriodeVM Periode)
            {
                if (Periode.isDirty)
                {
                    if (MessageBox.Show("La période a été modifiée. Voulez vous enregistrer?", "Vers de grappe", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        Periode.SaveCommand.Execute(null);
                }
                Periode.CloseVM();
                DataContext = null;
            }
                
        }
    }
}
