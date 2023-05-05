﻿using ComptageVDG.ViewModels;
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
    /// Logique d'interaction pour ParcelleView.xaml
    /// </summary>
    public partial class ParcelleView : UserControl
    {
        public ParcelleView()
        {
            InitializeComponent();
            DataContext = (ParcelleVM) this.Resources["viewModel"];
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ParcelleVM parcelle)
                parcelle.CloseVM();
        }
    }
}
