using ComptageVDG.ViewModels;
using Infragistics.Windows.DataPresenter;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using Infragistics.Documents.Excel;

using System.Collections;
using Infragistics.Windows.DataPresenter.ExcelExporter;

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

        private void EXPORT_Click(object sender, RoutedEventArgs e)
        {

           
            this.SaveExport();
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
                    exporter.Export(xamDataGridCampagne , dialog.FileName, WorkbookFormat.Excel2007);
                   
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message);
                }
            }
        }
    }

    //public class FieldNameToBoolConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        if (value == null)
    //            return value;
    //        var name = ((Infragistics.Windows.DataPresenter.SummaryResult)value).SourceField.Name;
    //        return name == "cptGlomerule" ? true : false;
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}
    //public class MyConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return (value as SummaryResultEntry).SummaryResult.SourceField.Label.ToString();
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    //    {
    //        return value;
    //    }
    //}
}
