using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptageVDG.IServices
{
    public interface IDataAccess
    {
         string ConnectionString { get; set; }
         bool IsConnected { get; }

         bool Open(string connectionString = "");
         DataTable ExecuteQuery(string query, object[]? paramsArray = null);
         IEnumerable<T> ExecuteQuery<T>(string query, object[]? paramsArray = null);
         int ExecuteNoQuery(string query, object[]? paramsArray = null );

         Task<IEnumerable<T>> AsyncExecuteQuery<T>(string query, object[]? paramsArray = null);

         Task<int> AsyncExecuteNoQuery<T>(string query, object[]? paramsArray = null);
         bool Close();
    }
}
