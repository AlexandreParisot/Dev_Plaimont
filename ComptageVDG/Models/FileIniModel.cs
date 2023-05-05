using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ComptageVDG.Models
{
    public class FileIniModel
    {
        public string UrlService { get; set; }
        [JsonIgnore]
        public string UserName { get; set; }
        [JsonIgnore]
        public List<string> Credentials { get; set; }
    }
}
