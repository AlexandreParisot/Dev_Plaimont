using ComptageVDG.IServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using System.Text.Json.Serialization;
using ComptageVDG.Helpers;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Collections;
using System.Windows.Ink;
using System.Windows.Markup;

namespace ComptageVDG.DataAccess
{
    internal class DataAccessApi : IDataAccess
    {


        private string _tokenUrl = String.Empty;
        private string _apiUrl = String.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _token = string.Empty;
        public TokenResponse token;


        #region Methode Private 
        public  HttpResponseMessage CallApi(string endpoint, HttpMethod http, string data = "")
        {
            try
            {
                var url = $"{_apiUrl}/{endpoint}";
                var req = new HttpRequestMessage();
                req.Method = http;     

                switch (http)
                {
                    case HttpMethod m when m == HttpMethod.Post:
                        if (!string.IsNullOrEmpty(data))
                            req.Content = new StringContent(data, Encoding.UTF8, "application/json");
                        break;
                    case HttpMethod m when m == HttpMethod.Put:
                        break;
                    case HttpMethod m when m == HttpMethod.Get:
                        if (!string.IsNullOrEmpty(data))
                            url += $"?{data}";
                        break;
                    default:
                        break;
                }
                req.RequestUri = new Uri(url);


                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var result = httpClient?.Send(req);

                return result;
            }catch(Exception ex)
            {
                Gestion.Erreur($"endpoint : {endpoint} - entetedata : {data} - message : {ex.Message}");
                return null;
            }
           
        }
        #endregion region



        #region Implementation Interface
        public string ConnectionString { get => _apiUrl; set => _apiUrl = value; }

        public  bool IsConnected { get {                   
                    var res =  CallApi("api/Status", HttpMethod.Get);                      
                    return (res != null && res.IsSuccessStatusCode);                    
            }
        }

        public async Task<DataTable> AsyncExecuteQuery(string query, object[]? paramsArray = null)
        {
            return await Task.Run(() => ExecuteQuery(query, paramsArray));
        }

        public async Task<int> AsyncExecuteNoQuery(string query, object[]? paramsArray = null)
        {
            return await Task.Run(() => ExecuteNoQuery(query, paramsArray));
        }


        public async Task<IEnumerable<T>> AsyncExecuteQuery<T>(string query, object[]? paramsArray = null)
        {
            return await Task.Run(() => ExecuteQuery<T>(query, paramsArray));
        }

        public bool Close()
        {
            return true;
        }

        public int ExecuteNoQuery(string query, object[]? paramsArray = null)
        {
            try {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentNullException(nameof(query));

                var cCommandeGet = string.Empty;
                var _httpMethod = HttpMethod.Post;

                if (paramsArray!.Any())
                {
                    var strCommand = paramsArray!.OfType<String>().FirstOrDefault();
                    if (!string.IsNullOrEmpty(strCommand))
                        cCommandeGet = strCommand;
                    var httpmethod = paramsArray!.OfType<HttpMethod>().FirstOrDefault();
                    if (httpmethod != null)
                        _httpMethod = httpmethod;
                }


                var result = CallApi(query, _httpMethod, cCommandeGet);
                 if (result!=null && result.IsSuccessStatusCode)
                    {
                    var str = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(str))
                    {
                        var tab = JsonSerialize.Deserialize<IEnumerable<int>>(str);
                        return tab.Count();
                    }                       
                    }
                else
                    return -1;

                return 0;
            }
            catch (Exception ex)
            {
                Gestion.Erreur(ex.Message);
                return -1;
            }
        }


        public DataTable ExecuteQuery(string query, object[]? paramsArray = null)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentNullException(nameof(query));

                var cCommandeGet = string.Empty;
                var _httpMethod = HttpMethod.Get;

                if (paramsArray != null && paramsArray!.Any())
                {
                    var strCommand =  paramsArray!.OfType<String>().FirstOrDefault();
                    if(!string.IsNullOrEmpty(strCommand))
                        cCommandeGet = strCommand;
                    var httpmethod = paramsArray!.OfType<HttpMethod>().FirstOrDefault();
                    if(httpmethod != null)
                        _httpMethod = httpmethod;
                }


                var result = CallApi(query, _httpMethod, cCommandeGet);
                if (result.IsSuccessStatusCode)
                {
                    var str = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(str))
                    {
                        DataTable? dataTable = new();
                        if (string.IsNullOrWhiteSpace(str))
                        {
                            return dataTable;
                        }
                        dataTable = JsonConvert.DeserializeObject<DataTable>(str);
                        return dataTable;
                    }
                }
                else
                    return null;

                return new DataTable();
            }
            catch (Exception ex)
            {
                Gestion.Erreur(ex.Message);
                return null;
            }
        }


       public T ExecuteQueryofT<T>(string query, object[]? paramsArray = null)
       {
            try
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentNullException(nameof(query));


                var cCommandeGet = string.Empty;
                var _httpMethod = HttpMethod.Get;

                if (paramsArray != null && paramsArray!.Any())
                {
                    var strCommand = paramsArray!.OfType<String>().FirstOrDefault();
                    if (!string.IsNullOrEmpty(strCommand))
                        cCommandeGet = strCommand;
                    var httpmethod = paramsArray!.OfType<HttpMethod>().FirstOrDefault();
                    if (httpmethod != null)
                        _httpMethod = httpmethod;
                }


                var result = CallApi(query, _httpMethod, cCommandeGet);
                if (result.IsSuccessStatusCode)
                {
                    var str = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(str))
                    {                       
                        var res = JsonSerialize.Deserialize<T>(str);
                        return res;
                    }         
                }

                Type type = typeof(T);
                object instance = Activator.CreateInstance(type);
                return (T)instance;

            }
            catch (Exception ex)
            {
                Gestion.Erreur(ex.Message);
                return default(T);
            }
        }

        public IEnumerable<T> ExecuteQuery<T>(string query, object[]? paramsArray = null)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                    throw new ArgumentNullException(nameof(query));


                var cCommandeGet = string.Empty;
                var _httpMethod = HttpMethod.Get;

                if (paramsArray != null && paramsArray!.Any())
                {
                    var strCommand = paramsArray!.OfType<String>().FirstOrDefault();
                    if (!string.IsNullOrEmpty(strCommand))
                        cCommandeGet = strCommand;
                    var httpmethod = paramsArray!.OfType<HttpMethod>().FirstOrDefault();
                    if (httpmethod != null)
                        _httpMethod = httpmethod;
                }


                var result = CallApi(query, _httpMethod, cCommandeGet);
                if (result != null && result.IsSuccessStatusCode)
                {
                    var str = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(str))
                    {
                        if(!str.Contains("["))
                            str = "["+str+"]"; 
                        
                        var res = JsonSerialize.Deserialize<IEnumerable<T>>(str);
                        return res;
                    } 
                        
              
                }
                else
                {
                   
                    return null;
                }
                    

                return Enumerable.Empty<T>();
            }
            catch(Exception ex)
            {
                Gestion.Erreur(ex.Message);
                return null;   
            }
           
        }

        public bool Open(string connectionString = "")
        {
            var result = (! string.IsNullOrEmpty(connectionString)? true : false);
            if(result)
                _apiUrl = connectionString;

            return result;  
        }

       
        #endregion
    }



    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("issued_at")]
        public long IssuedAt { get; set; }

        public DateTime IssuedAtDateTime
        {
            get { return DateTime.FromFileTimeUtc(IssuedAt); }
        }

        public bool HasExpired()
        {
            return DateTime.UtcNow > IssuedAtDateTime.AddSeconds(ExpiresIn);
        }
    }

}
