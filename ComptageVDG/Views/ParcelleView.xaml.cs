﻿using ComptageVDG.Models;
using ComptageVDG.ViewModels;
using Infragistics.Documents.Excel;
using Infragistics.Windows.DataPresenter.ExcelExporter;
using Microsoft.Win32;
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
            DataContext = new ParcelleVM();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ParcelleVM parcelle)
            {
                parcelle.CloseVM();
                DataContext = null;                
            }
        }

        private void xamDataGridParcelle_DataValueChangedDirect(object sender, Infragistics.Windows.DataPresenter.Events.DataValueChangedEventArgs e)
        {
            //if (DataContext is ParcelleVM parcellevm)
            //{
            //    if (e.Record.DataItem is ParcelleModel parcelle)
            //    {

            //        parcellevm.ChangeStateCommand.Execute(e.Record.DataItem);
            //    }
            //}
        }

        private void EXPORT_Click(object sender, RoutedEventArgs e)
        {
            SaveExport();
        }

        private void SaveExport()
        {
            SaveFileDialog dialog;
            DataPresenterExcelExporter exporter = (DataPresenterExcelExporter)this.Resources["excelExporter1"];
            dialog = new SaveFileDialog { Filter = "Excel files|*.xlsx", DefaultExt = "xlsx" };


            if (dialog.ShowDialog() == true)
            {
                try
                {
                    exporter.Export(xamDataGridParcelle, dialog.FileName, WorkbookFormat.Excel2007);

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
