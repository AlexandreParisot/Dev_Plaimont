using Newtonsoft.Json;

namespace APIComptageVDG.Helpers
{
   
    public static class JsonSerialize
    {


        public static void SerializeToFile<T>(string filePath, T data)
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        public static string Serialize<T>(T data)
        {
            try
            {
                return JsonConvert.SerializeObject(data, Formatting.Indented);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return string.Empty;
            }
        }

        public static T DeserializeFromFile<T>(string filePath)
        {
            try
            {
                string jsonString = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch
            {
                Type type = typeof(T);
                object instance = Activator.CreateInstance(type);
                return (T)instance;
            }

        }

        public static T Deserialize<T>(string jsonString)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch
            {
                Type type = typeof(T);
                object instance = Activator.CreateInstance(type);
                return (T)instance;
            }
        }

    }
}
