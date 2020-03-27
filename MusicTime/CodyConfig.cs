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
    public class CodyConfig
    {

        private static CodyConfig instance = null;

        private CodyConfig()
        {
        }

        public static CodyConfig getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CodyConfig();
                }
                return instance;
            }
        }
        public string spotifyAccessToken { get; set; }
        public string spotifyRefreshToken { get; set; }
        public string spotifyClientSecret { get; set; }
        public string spotifyClientId { get; set; }
        public bool enableSpotifyApi { get; set; }
        public string spoftifyUserId { get; set; }

        public void setConfig(CodyConfig codyConfig)
        {
            this.spotifyAccessToken = codyConfig.spotifyAccessToken;
            this.spotifyClientId = codyConfig.spotifyClientId;
            this.spotifyClientSecret = codyConfig.spotifyClientSecret;
            this.spotifyRefreshToken = codyConfig.spotifyRefreshToken;
            this.enableSpotifyApi = codyConfig.enableSpotifyApi;
        }
    }
    public class Device
    {
        private static Device instance = null;
        private Device()
        {
        }

        public static Device getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Device();
                }
                return instance;
            }
        }

        public string id { get; set; }
        public bool is_active { get; set; }
        public bool is_private_session { get; set; }
        public bool is_restricted { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int volume_percent { get; set; }

        public List<Device> devices { get; set; }
    }


    public enum trackState
    {
        Playing ,
        Paused ,
        Advertisement ,
        NotAssigned,
        GrantError 
    }
    public enum PlayerType
    {
        MacItunesDesktop ,
        MacSpotifyDesktop,
        WindowsSpotifyDesktop,
        WebSpotify,
        NotAssigned,
        track
    }

    public partial class ExternalUrls
    {
        private string spotify { get; set; }
    }

    public class Context
    {
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class ExternalUrls2
    {
        private string spotify { get; set; }
    }

    public class Artist
    {
        public ExternalUrls2 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public List<string> genres { get; set; }
    }

    public class ExternalUrls3
    {
        private string spotify { get; set; }
    }

    public class Image
    {
        public int height { get; set; }
        public string url { get; set; }
        public int width { get; set; }
    }

    public class Album
    {
        public string album_type { get; set; }
        public List<Artist> artists_name { get; set; }
        public List<string> available_markets { get; set; }
        public ExternalUrls3 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<Image> images { get; set; }
        public string name { get; set; }
        public string release_date { get; set; }
        public string release_date_precision { get; set; }
        public int total_tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class ExternalUrls4
    {
        private string spotify { get; set; }
    }

    public class Artist2
    {
        public ExternalUrls4 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class ExternalIds
    {
        public string isrc { get; set; }
    }

    public class ExternalUrls5
    {
        private string spotify { get; set; }
    }
    public class Owner
    {
        public string display_name { get; set; }
        public ExternalUrls2 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }
    public class Track
    {
        // start and end are in seconds
        public long start { get; set; }
        public long local_start { get; set; }
        public long end { get; set; }
        public long local_end { get; set; }
        public string timezone { get; set; }
        public double offset { get; set; } // in minutes
        public string pluginId { get; set; }
        public string os { get; set; }
        public string version { get; set; }
        public Album album { get; set; }
        public string artist { get; set; }
        public List<string> artist_names { get; set; }
        public List<Artist> artists { get; set; }
        public List<string> available_markets { get; set; }
        public double disc_number { get; set; }
        public double duration { get; set; }
        public double duration_ms { get; set; }
        public bool @explicit { get; set; }
        public ExternalIds external_ids { get; set; }
        public ExternalUrls5 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_local { get; set; }
        public string name { get; set; }
        public double popularity { get; set; }
        public string preview_url { get; set; }
        public double track_number { get; set; }
        public PlayerType player_type { get; set; }
        public string uri { get; set; }
        public string genre { get; set; }
        public string error { get; set; }
        public bool loved { get; set; }
        public double played_count { get; set; }
        public double volume { get; set; }
        public double progress_ms { get; set; }
        public AudioFeature features { get; set; } 
        public trackState state { get; set; }
        public string type { get; set; }
       
    }

    //public class SpotifyAudioFeature
    //{
    //    public string id { get; set; }
    //    public string uri { get; set; }
    //    public string track_href { get; set; }
    //    public string analysis_url { get; set; }
      
    //    public double danceability { get; set; }
    //    public double energy { get; set; }
    //    public double key { get; set; }
    //    public double loudness { get; set; }
    //    public double mode { get; set; }
    //    public double speechiness { get; set; }
    //    public double acousticness { get; set; }
    //    public double instrumentalness { get; set; }
    //    public double liveness { get; set; }
    //    public double valence { get; set; }
    //    public double tempo { get; set; }
    //    public double duration_ms { get; set; }
    //    public double time_signature { get; set; }

    //}
    
    public class Disallows
    {
        public bool resuming { get; set; }
        public bool skipping_prev { get; set; }
    }

    public class Actions
    {
        public Disallows disallows { get; set; }
       
    }

    

    public class Tracks
    {
        public string href { get; set; }
        public int total { get; set; }
    }
    public class PlaylistItem
    {
        public bool collaborative { get; set; }
        public string description { get; set; }
        public ExternalUrls external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public List<object> images { get; set; }
        public string name { get; set; }
        public Owner owner { get; set; }
        public object primary_color { get; set; }
        public bool @public { get; set; }
        public string snapshot_id { get; set; }
        public Tracks tracks { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
        public DateTime added_at { get; set; }
        public Track track { get; set; }
    }

    public class sourceKey
    {
        public string key { get; set; }
        public SourceData SourceData { get; set; }
    }

    public partial class SourceData
    {
        [JsonProperty("paste")]
        public long Paste { get; set; }

        [JsonProperty("open")]
        public long Open { get; set; }

        [JsonProperty("close")]
        public long Close { get; set; }

        [JsonProperty("delete")]
        public long Delete { get; set; }

        [JsonProperty("add")]
        public long Add { get; set; }

        [JsonProperty("netkeys")]
        public long Netkeys { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("lines")]
        public long Lines { get; set; }

        [JsonProperty("linesAdded")]
        public long LinesAdded { get; set; }

        [JsonProperty("linesRemoved")]
        public long LinesRemoved { get; set; }

        [JsonProperty("syntax")]
        public string Syntax { get; set; }

        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("local_start")]
        public long LocalStart { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }

        [JsonProperty("local_end")]
        public long LocalEnd { get; set; }
    }

    public class SpotifySongs
    {
        public List<PlaylistItem> items { get; set; }
        public int total { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public object previous { get; set; }
        public string href { get; set; }
        public object next { get; set; }
        public string id { get; set; }
    }
    public class TrackStatus
    {
        public long timestamp { get; set; }
        public Context context { get; set; }
        public int progress_ms { get; set; }
        public Track item { get; set; }
        public string currently_playing_type { get; set; }
        public Actions actions { get; set; }
        public bool is_playing { get; set; }
    }

    public class options
    {
        public string playlist_id { get; set; }
        public string track_id { get; set; }
        public string album_id { get; set; }
        public options(string album_id =null,string playlist_id=null,string track_id=null)
        {
            this.album_id = album_id;
            this.playlist_id = playlist_id;
            this.track_id = track_id;
        }
    }

   
    public class AiGeneratedPlaylistItem
    {
        public bool collaborative { get; set; }
        public string playlist_id { get; set; }
        public bool @public { get; set; }
        public string name { get; set; }
        public int deleted { get; set; }
        public int playlistTypeId { get; set; }
    }

    public class Seed
    {
        public int initialPoolSize { get; set; }
        public int afterFilteringSize { get; set; }
        public int afterRelinkingSize { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string href { get; set; }
    }

    public class TrackList
    {
        public List<Track> tracks { get; set; }
        public List<Seed> seeds { get; set; }
    }

    public partial class SpotifyAudioFeature
    {
        [JsonProperty("audio_features")]
        public List<AudioFeature> AudioFeatures { get; set; }
    }

    public partial class AudioFeature
    {
        [JsonProperty("danceability")]
        public double Danceability { get; set; }

        [JsonProperty("energy")]
        public double Energy { get; set; }

        [JsonProperty("key")]
        public long Key { get; set; }

        [JsonProperty("loudness")]
        public double Loudness { get; set; }

        [JsonProperty("mode")]
        public long Mode { get; set; }

        [JsonProperty("speechiness")]
        public double Speechiness { get; set; }

        [JsonProperty("acousticness")]
        public double Acousticness { get; set; }

        [JsonProperty("instrumentalness")]
        public double Instrumentalness { get; set; }

        [JsonProperty("liveness")]
        public double Liveness { get; set; }

        [JsonProperty("valence")]
        public double Valence { get; set; }

        [JsonProperty("tempo")]
        public double Tempo { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("track_href")]
        public Uri TrackHref { get; set; }

        [JsonProperty("analysis_url")]
        public Uri AnalysisUrl { get; set; }

        [JsonProperty("duration_ms")]
        public long DurationMs { get; set; }

        [JsonProperty("time_signature")]
        public long TimeSignature { get; set; }
    }

    public partial class SpotifyAudioFeature
    {
        public static SpotifyAudioFeature FromJson(string json) => JsonConvert.DeserializeObject<SpotifyAudioFeature>(json, MusicTime.Converter.Settings);
    }

    public static class Serializes
    {
        public static string ToJson(this SpotifyAudioFeature self) => JsonConvert.SerializeObject(self, MusicTime.Converter.Settings);
    }

    internal static class Converters
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

    public partial class SlackChannel
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("channels")]
        public List<Channel> Channels { get; set; }

        [JsonProperty("response_metadata")]
        public ResponseMetadata ResponseMetadata { get; set; }
    }

    public partial class Channel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("is_channel")]
        public bool IsChannel { get; set; }

        [JsonProperty("is_group")]
        public bool IsGroup { get; set; }

        [JsonProperty("is_im")]
        public bool IsIm { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("is_archived")]
        public bool IsArchived { get; set; }

        [JsonProperty("is_general")]
        public bool IsGeneral { get; set; }

        [JsonProperty("unlinked")]
        public long Unlinked { get; set; }

        [JsonProperty("name_normalized")]
        public string NameNormalized { get; set; }

        [JsonProperty("is_shared")]
        public bool IsShared { get; set; }

        [JsonProperty("parent_conversation")]
        public object ParentConversation { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("is_ext_shared")]
        public bool IsExtShared { get; set; }

        [JsonProperty("is_org_shared")]
        public bool IsOrgShared { get; set; }

        [JsonProperty("shared_team_ids")]
        public List<string> SharedTeamIds { get; set; }

        [JsonProperty("pending_shared")]
        public List<object> PendingShared { get; set; }

        [JsonProperty("pending_connected_team_ids")]
        public List<object> PendingConnectedTeamIds { get; set; }

        [JsonProperty("is_pending_ext_shared")]
        public bool IsPendingExtShared { get; set; }

        [JsonProperty("is_member")]
        public bool IsMember { get; set; }

        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }

        [JsonProperty("is_mpim")]
        public bool IsMpim { get; set; }

        [JsonProperty("topic")]
        public Purpose Topic { get; set; }

        [JsonProperty("purpose")]
        public Purpose Purpose { get; set; }

        [JsonProperty("previous_names")]
        public List<object> PreviousNames { get; set; }

        [JsonProperty("num_members")]
        public long NumMembers { get; set; }
    }

    public partial class Purpose
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("last_set")]
        public long LastSet { get; set; }
    }

    public partial class ResponseMetadata
    {
        [JsonProperty("next_cursor")]
        public string NextCursor { get; set; }
    }

    public partial class SlackChannel
    {
        public static SlackChannel FromJson(string json) => JsonConvert.DeserializeObject<SlackChannel>(json, MusicTime.Converter.Settings);
    }
}
