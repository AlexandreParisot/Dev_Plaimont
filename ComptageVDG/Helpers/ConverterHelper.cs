using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Helpers
{
    public static class ConverterHelper
    {
        public static DataTable ConvertDataRowsToDataTable(IEnumerable<DataRow> dataRows)
        {
            DataTable dataTable = dataRows.FirstOrDefault()?.Table.Clone();
            foreach (var row in dataRows)
            {
                dataTable.Rows.Add(row.ItemArray);
            }
            return dataTable;
        }

        //public static DataTable ConvertDataRowsToDataTable(IEnumerable<object> dataRows, string tableName = "INCONNU")
        //{
        //    DataTable dataTable = new DataTable();
        //    dataTable.Columns.Clear();  
        //    dataTable.TableName = tableName;    
        //    dataTable.Rows.Clear();
           
        //    foreach (Array[] row in dataRows)
        //    {
        //        bool first = true;
        //        string Colname = string.Empty;
        //        DataRow dr = null;
        //        foreach (var item in row)
        //        {
                    
        //            if (first)
        //            {
        //                if (!dataTable.Columns.Contains(item?.ToString()))
        //                dataTable.Columns.Add(item.ToString());
        //                first = false;
        //                Colname = item?.ToString();
        //            }
        //            else
        //            {
        //                dr = dataTable.NewRow();
        //                dr[Colname] = item;
        //            }
        //            dataTable.Rows.Add(dr);
        //        }                
        //        dataTable.Rows.Add();
        //    }
        //    return dataTable;
        //}

    }
}
