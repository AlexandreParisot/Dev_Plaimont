using APIComptageVDG.Helpers;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace APIComptageVDG.Provider
{
    public class InstagrappeApiProvider
    {

        private string _tokenUrl = String.Empty;
        private string _apiUrl =  String.Empty;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _token = string.Empty ;
        public TokenResponse token;

        public InstagrappeApiProvider(string tokenUrl, string apiUrl, string username, string password)
        {
            _tokenUrl = tokenUrl;
            _apiUrl = apiUrl;
            _username = username;
            _password = password;
        }         

        public async Task<string> GetToken()
        {
            if (string.IsNullOrEmpty(_token))
            {
                using var httpClient = new HttpClient();
                
                var req = new HttpRequestMessage(HttpMethod.Post, _tokenUrl);
                string postData = $"client_id=atrium-token&grant_type=password&username={_username.Trim()}&password={_password.Trim()}";
                req.Content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                var response = await httpClient.SendAsync(req);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    token = JsonSerializer.Deserialize<TokenResponse>(responseContent)!;
                    _token = token!.AccessToken;
                }
                else
                    Gestion.Erreur($"Reponse Token erreur : {responseContent}");
            }
            return _token;
        }

        public async Task<HttpResponseMessage> CallApi(string endpoint, HttpMethod http, string data = "" )
        {
            if(!string.IsNullOrEmpty(_token) && token.HasExpired())
                _token = await GetToken();

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
                    if(!string.IsNullOrEmpty(data) )
                        url += $"?{data}";
                    break;
                default:
                    break;
            }
            req.RequestUri = new Uri(url);


            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);            


            return await httpClient.SendAsync(req);
        }


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
