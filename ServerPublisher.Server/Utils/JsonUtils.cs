using Newtonsoft.Json;

namespace ServerPublisher.Server.Utils
{
    public class JsonUtils
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented };
    }
}
