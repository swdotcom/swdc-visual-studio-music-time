using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{

    public partial class Payload
    {
        [JsonProperty("context_uri")]
        public string ContextUri { get; set; }

        [JsonProperty("offset")]
        public Offset Offset { get; set; }
    }

    public partial class Offset
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
   
    public partial class Payload
    {
        public static Payload FromJson(string json) => JsonConvert.DeserializeObject<Payload>(json, MusicTime.Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Payload self) => JsonConvert.SerializeObject(self, MusicTime.Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

