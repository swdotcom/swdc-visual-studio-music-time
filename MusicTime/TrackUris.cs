using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Converters;
namespace MusicTime
{
    public partial class TrackUris
    {
        [JsonProperty("uris")]
        public string[] Uris { get; set; }
    }

    public partial class TrackUris
    {
        public static TrackUris FromJson(string json) => JsonConvert.DeserializeObject<TrackUris>(json, MusicTime.Converter.Settings);
    }

    public static class serialize
    {
        public static string ToJson(this TrackUris self) => JsonConvert.SerializeObject(self, MusicTime.Converter.Settings);
    }
    
}
