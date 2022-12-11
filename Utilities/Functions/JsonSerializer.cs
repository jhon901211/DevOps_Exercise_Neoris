using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Utilities.Functions
{
    /// <summary>
    /// Clase encargada se serealizar y deserealizar.
    /// </summary>
    public class JsonSerializer
    {
        /// <summary>
        /// Convierte objeto en formato JSON.
        /// </summary>
        public static string SerializeEntity<T>(T entity)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            return JsonConvert.SerializeObject(entity, Formatting.None, settings);
        }

        /// <summary>
        /// Convierte formato JSON en objeto.
        /// </summary>
        public static T DeserializeObject<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        /// <summary>
        /// Convierte objeto en formato JSON.
        /// </summary>
        public static JObject DeserializeString(string jsonString)
        {
            return JObject.Parse(jsonString);
        }

    }
}
