using ComptageVDG.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Logique d'interaction pour LoadingView.xaml
    /// </summary>
    public partial class LoadingView : UserControl
    {

       
        public bool isLoading
        {
            get { return ((LoadingVM)this.DataContext).IsLoading; }
            set { ((LoadingVM)this.DataContext).IsLoading = value; }
        }

        public string MessageLoading
        {
            get { return ((LoadingVM)this.DataContext).MessageLoading; }
            set { ((LoadingVM)this.DataContext).MessageLoading = value; }
        }
               

        public LoadingView()
        {
            InitializeComponent();
            this.DataContext = new LoadingVM();
        }

       
    }
}
