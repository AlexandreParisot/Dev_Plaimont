using ComptageVDG.Helpers;
using ComptageVDG.IServices;
using CsvHelper;
using CsvHelper.Configuration;
using Infragistics.Windows.DataPresenter;
using Microsoft.VisualBasic;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml.Linq;

namespace ComptageVDG.DataAccess
{
    internal class DataAccessFileCsv : IDataAccess
    {
        private string _csvFilePath = string.Empty;

        public string ConnectionString { get => _csvFilePath; set => _csvFilePath = value; }
        public bool IsConnected { get => Directory.Exists(_csvFilePath); }

        public async  Task<int> AsyncExecuteNoQuery(string query, object[]? paramsArray = null)
        {
            return await Task.Run(() => { return ExecuteNoQuery(query, paramsArray); });
        }

        public async Task<IEnumerable<T>> AsyncExecuteQuery<T>(string query, object[]? paramsArray = null)
        {
            return await Task.Run(() => { return ExecuteQuery<T>(query, paramsArray); });
        }

        public async Task<DataTable> AsyncExecuteQuery(string query, object[]? paramsArray = null)
        {
            return await Task.Run(() => ExecuteQuery(query, paramsArray));
        }

        public bool Close()
        {
            return true;
        }

        public int ExecuteNoQuery(string query, object[]? paramsArray = null)
        {

            try
            {
                var MapType = string.Empty;
                var ModelType = string.Empty;
                var DatatableType = string.Empty;
                Type objectType = null;

                var pathFile = Path.Combine(_csvFilePath, query);


                if (!File.Exists(query) && !File.Exists(pathFile))
                {
                    //creation du fichier
                    try
                    {
                        var file = File.Create(pathFile);
                        file.Close();   

                    }catch(Exception ex)
                    {
                        return -1;
                    }
                    
                }
                
                if(!File.Exists(pathFile))
                    pathFile = query;

                if (!File.Exists(pathFile))
                    return -1;


                if (paramsArray != null)
                {
                    DatatableType = paramsArray.OfType<String>().FirstOrDefault(s => s.EndsWith("Table"));
                    MapType = paramsArray.OfType<String>().FirstOrDefault(s => s.EndsWith("Map"));
                    objectType = Type.GetType($"{MapType}");
                    if (objectType == null)
                        MapType = string.Empty;

                    ModelType = paramsArray.OfType<String>().FirstOrDefault(s => s.EndsWith("Model"));
                    objectType = Type.GetType($"{ModelType}");
                    if (objectType == null)
                        ModelType = string.Empty;
                }


                if (!string.IsNullOrEmpty(DatatableType))
                {

                   DataTable rec = paramsArray.OfType<DataTable>().FirstOrDefault();
                    if( rec != null)
                    {
                        using (var writer = new StreamWriter(pathFile))
                        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                        {
                            // Write columns
                            foreach (DataColumn column in rec.Columns)
                            {
                                csv.WriteField(column.ColumnName);
                            }
                            csv.NextRecord();

                            // Write row values
                            foreach (DataRow row in rec.Rows)
                            {
                                for (var i = 0; i < rec.Columns.Count; i++)
                                {
                                    csv.WriteField(row[i]);
                                }
                                csv.NextRecord();
                            }
                        }
                        return 1;
                    }
                    else return 0;
                }
                else if (!string.IsNullOrEmpty(ModelType)){
                    var rec = paramsArray.FirstOrDefault(obj => obj.GetType().GetInterfaces()
                                               .Any(t => t.IsGenericType &&
                                                         t.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                                                         t.GetGenericArguments().First() == objectType));

                    using (var writer = new StreamWriter(pathFile))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        if (!string.IsNullOrEmpty(MapType))
                            csv.Context.RegisterClassMap((ClassMap)Activator.CreateInstance(Type.GetType($"{MapType}")));


                        Type itemType = Type.GetType(ModelType); // récupérer le type correspondant
                        Type enumerableType = typeof(IEnumerable<>).MakeGenericType(itemType);

                        csv.WriteRecords(DynamicCast(rec, enumerableType));
                    }
                    return 1;
                }


               

                return 0;
            }
            catch
            {
                return -1;   
            }
            
            
        }

        dynamic DynamicCast(object entity, Type to)
        {
            var openCast = this.GetType().GetMethod("Cast", BindingFlags.Static | BindingFlags.NonPublic);
            var closeCast = openCast.MakeGenericMethod(to);
            return closeCast.Invoke(entity, new[] { entity });
        }
        static T Cast<T>(object entity) where T : class
        {
            return entity as T;
        }

        public IEnumerable<T> ExecuteQuery<T>(string query, object[]? paramsArray = null)
        {
            try
            {
                //query est le nom du fichier
                var MapType = string.Empty;
                Type objectType = null;
                List<T> retour = new List<T>() ;
                if (paramsArray != null)
                {
                    MapType = paramsArray.OfType<string>().FirstOrDefault(s => s.EndsWith("Map"));
                    objectType = Type.GetType($"{MapType}");
                    if (objectType == null)
                        MapType = string.Empty;
                }
                var pathFile = Path.Combine(_csvFilePath, query);

                if (!File.Exists(pathFile))
                    pathFile = query;

                if (!File.Exists(pathFile))
                    return new List<T>();

                using (var reader = new StreamReader(pathFile))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    if (!string.IsNullOrEmpty(MapType))
                        csv.Context.RegisterClassMap((ClassMap)Activator.CreateInstance(objectType));
                      
                    retour = csv.GetRecords<T>().ToList();

                }
                return retour;
            }
            catch
            {
                return new List<T>();
            }
            
        }

        public DataTable ExecuteQuery(string query, object[]? paramsArray = null)
        {
            //query est le nom du fichier
            var dataTable = new DataTable();
            dataTable.TableName = "Generic Table";
            try
            {
                if(string.IsNullOrEmpty(query))
                    return dataTable;

                dataTable.TableName = Path.GetFileNameWithoutExtension(query);

                if (paramsArray != null)
                {

                }
                var pathFile = Path.Combine(_csvFilePath, query);
                
                if(!File.Exists(pathFile))
                    pathFile= query;

                if (!File.Exists(pathFile))
                    return dataTable;

               
                using (var reader = new StreamReader(pathFile))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {

                    using (var dr = new CsvDataReader(csv))
                    {
                        dataTable.Load(dr);
                    }

                    dataTable.TableName = Path.GetFileNameWithoutExtension(query);
                }
                return dataTable;

            }
            catch(Exception ex)
            {
                var errTable = new DataTable();
                errTable.TableName = "Erreur";
                return errTable;
            }
        }


        public bool Open(string file = "")
        {
            if (string.IsNullOrEmpty(file))
                return IsConnected;

            return File.Exists(Path.Combine(ConnectionString, file));
        }

        

    }
}
