using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.Models.Map
{
    public class PeriodeModelMap : ClassMap<PeriodeModel>
    {
        public PeriodeModelMap()
        {
            string format = "dd/MM/yyyy";
            var msMY = CultureInfo.GetCultureInfo("fr-FR");

            Map(m => m.Name).Name("name");
            Map(m => m.DateDebut).Name("datedeb");
            Map(m => m.DateFin).TypeConverterOption.Format(format)
              .TypeConverterOption.CultureInfo(msMY).Name("datefin");
            Map(m => m.Year).Name("year");
            Map(m => m.Id_periode).Name("id");
            Map(m => m.DateDebut).TypeConverterOption.Format(format)
              .TypeConverterOption.CultureInfo(msMY).Name("datedeb");
        }
    }
}
