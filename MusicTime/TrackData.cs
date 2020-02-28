using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{
    class TrackData
    {
        Track Track = new Track();
        public long keystrokes { get; set; } // keystroke count
        // start and end are in seconds
        public string timezone { get; set; }
        
        public double offset { get; set; } // in minutes
        public string os { get; set; }
        public string version { get; set; }
        public string pluginId { get; set; }
        
        public long Add { get; set; }
        public long Paste { get; set; }
        public long Delete { get; set; }
        public long Netkeys { get; set; }
        public long LinesAdded { get; set; }
        public long LinesRemoved { get; set; }
        public long Open { get; set; }
        public long Close { get; set; }

       
  
        public TrackData(Track track)
        {
            Track.album = track.album;
            Track.artist = track.artist;
            Track.artists = track.artists;
            Track.artist_names = track.artist_names;
            Track.available_markets = track.available_markets;
            Track.disc_number = track.disc_number;
            Track.duration = track.duration;
            Track.duration_ms = track.duration_ms;
            Track.@explicit = track.@explicit;
            Track.external_ids = track.external_ids;
            Track.external_urls = track.external_urls;
            Track.features = track.features;
            Track.genre = track.genre;
            Track.href = track.href;
            Track.id = track.id;
            Track.is_local = track.is_local;
            Track.name = track.name;
            Track.played_count = track.played_count;
            Track.popularity = track.popularity;
            Track.preview_url = track.preview_url;
            Track.track_number = track.track_number;
            Track.type = track.type;
            Track.uri = track.uri;
            Track.volume = track.volume;
            Track.start = track.start;
            Track.local_start = track.local_start;
            Track.end = track.end;
            Track.local_end = track.local_end;
            Track.os = track.os;
            Track.timezone = track.timezone;
            Track.version = track.version;
            Track.player_type = track.player_type;
            Track.source = track.source;
           
    
    }

        public IDictionary<string, object> GetAsDictionary()
        {
            IDictionary<string, object> dict = new Dictionary<string, object>();
            //dict.Add("start", Track.start);
            //dict.Add("local_start", Track.local_start);
            //dict.Add("keystrokes", Track.keystrokes);
            //dict.Add("timezone", Track.timezone);
            //dict.Add("offset", Track.offset);
            dict.Add("album", Track.album);
            dict.Add("artist", Track.artist);
            dict.Add("artists", Track.artists);
            dict.Add("artist_names", Track.artist_names);
            dict.Add("available_markets", Track.available_markets);
            dict.Add("disc_number", Track.disc_number);
            dict.Add("duration", Track.duration);
            dict.Add("duration_ms", Track.duration_ms);
            dict.Add("error", Track.error);
            dict.Add("@explicit", Track.@explicit);
            dict.Add("external_ids", Track.external_ids);
            dict.Add("external_urls", Track.external_urls);
            dict.Add("features", Track.features);
            dict.Add("href", Track.href);
            dict.Add("id", Track.id);
            dict.Add("is_local", Track.is_local);
            dict.Add("name", Track.name);
            dict.Add("played_count", Track.played_count);
            dict.Add("popularity", Track.popularity);
            dict.Add("preview_url", Track.preview_url);
            dict.Add("track_number", Track.track_number);
            dict.Add("type", Track.type);
            dict.Add("uri", Track.uri);
            dict.Add("volume", Track.volume);
            return dict;
        }

        public string GetAsJson()
        {
            JsonObject jsonObj = new JsonObject();
           
            jsonObj.Add("album", Track.album);
            jsonObj.Add("artist", Track.artist);
            jsonObj.Add("artists", Track.artists);
            jsonObj.Add("artist_names", Track.artist_names);
            jsonObj.Add("available_markets", Track.available_markets);
            jsonObj.Add("disc_number", Track.disc_number);
            jsonObj.Add("duration", Track.duration);
            jsonObj.Add("duration_ms", Track.duration_ms);
            jsonObj.Add("error", Track.error);
            jsonObj.Add("@explicit", Track.@explicit);
            jsonObj.Add("external_ids", Track.external_ids);
            jsonObj.Add("external_urls", Track.external_urls);
            jsonObj.Add("features", Track.features);
            jsonObj.Add("href", Track.href);
            jsonObj.Add("id", Track.id);
            jsonObj.Add("is_local", Track.is_local);
            jsonObj.Add("name", Track.name);
            jsonObj.Add("played_count", Track.played_count);
            jsonObj.Add("popularity", Track.popularity);
            jsonObj.Add("preview_url", Track.preview_url);
            jsonObj.Add("track_number", Track.track_number);
            jsonObj.Add("type", Track.type);
            jsonObj.Add("uri", Track.uri);
            jsonObj.Add("volume", Track.volume);
            jsonObj.Add("start", Track.start);
            jsonObj.Add("local_start", Track.local_start);
            jsonObj.Add("end", Track.end);
            jsonObj.Add("local_end", Track.local_end);
            jsonObj.Add("add", this.Add);
            jsonObj.Add("paste", this.Paste);
            jsonObj.Add("delete", this.Delete);
            jsonObj.Add("netkeys", this.Netkeys);
            jsonObj.Add("linesAdded", this.LinesAdded);
            jsonObj.Add("linesRemoved", this.LinesRemoved);
            jsonObj.Add("open", this.Open);
            jsonObj.Add("close", this.Close);
            jsonObj.Add("keystrokes", this.keystrokes);
           
            jsonObj.Add("timezone", Track.timezone);
            jsonObj.Add("offset", Track.offset);
            jsonObj.Add("pluginId", this.pluginId);
            jsonObj.Add("os", this.os);
            jsonObj.Add("version", this.version);
           
            jsonObj.Add("source", Track.source);
            return jsonObj.ToString();
        }
    }
}
