using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public List<Artist> artists { get; set; }
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
        public Album album { get; set; }
        public List<Artist2> artists { get; set; }
        public List<string> available_markets { get; set; }
        public int disc_number { get; set; }
        public int duration_ms { get; set; }
        public bool @explicit { get; set; }
        public ExternalIds external_ids { get; set; }
        public ExternalUrls5 external_urls { get; set; }
        public string href { get; set; }
        public string id { get; set; }
        public bool is_local { get; set; }
        public string name { get; set; }
        public int popularity { get; set; }
        public string preview_url { get; set; }
        public int track_number { get; set; }
        public string type { get; set; }
        public string uri { get; set; }
    }

    public class Disallows
    {
        public bool resuming { get; set; }
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
}
