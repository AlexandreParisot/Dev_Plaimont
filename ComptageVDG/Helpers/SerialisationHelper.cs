using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ComptageVDG.Helpers
{
    
    public static class SerialisationHelper
    {
        public static bool SerialiserToFile<T>(T objet, string pathFile)
        {
            try
            {
                var option = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                };
                var strJson = JsonSerializer.Serialize<T>(objet,option);
                File.WriteAllText(pathFile, strJson);
                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public static T DeserialiserFromFile<T>(string pathFile)
        {
            try
            {
                var option = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                };
                var strJson = File.ReadAllText(pathFile);
                return JsonSerializer.Deserialize<T>(strJson,option);
            }
            catch
            {
                return default(T); 
            }
        }
    }
}
