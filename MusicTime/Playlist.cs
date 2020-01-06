using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{

    class Playlist
    {
        private static Playlist instance        = null;
        public static MusicClient musicClient   = MusicClient.getInstance;
        public static CodyConfig codyConfig     = CodyConfig.getInstance;

       

        private Playlist()
        {
        }

        public static Playlist getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Playlist();
                }
                return instance;
            }
        }

        public static List<PlaylistItem> _Playlists { get; set; }

        public static async Task<List<PlaylistItem>> getPlaylistsAsync()
        {
           
            if(codyConfig.spoftifyUserId!= null)
            {
               _Playlists = await getPlaylistsForUserAsync(codyConfig.spoftifyUserId,50,0);     
            }
            return _Playlists;
        }

        public static async Task<List<PlaylistItem>> getPlaylistsForUserAsync(string spotifyUserid,
            int limit,int offset)
        {
            HttpResponseMessage response    = null;

            string api                      = "/v1/users/"+ spotifyUserid +"/playlists";

            SpotifySongs PlaylistItems      = new SpotifySongs();
            _Playlists = new List<PlaylistItem>();


            try
            {
                response = await MusicClient.SpotifyApiGetAsync(api);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiGetAsync(api);
                }

                if (MusicClient.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    PlaylistItems       = JsonConvert.DeserializeObject<SpotifySongs>(responseBody);

                    foreach (PlaylistItem item in PlaylistItems.items)
                    {
                        _Playlists.Add(item);
                    }

                }

            }
            catch (Exception ex)
            {


            }
            return _Playlists;
        }

        public static async Task<List<string>> getPlaylistNamesAsync()
        {
            List<string> names                  = new List<string>();
            List<PlaylistItem> playlistItems    = null;
            playlistItems                       = await getPlaylistsAsync();

            foreach (PlaylistItem item in _Playlists)
            {
                names.Add(item.name);
            }

            return names;
        }

        public static async Task<List<Track>> getPlaylistTracksAsync(string playlistId)
        {

            string api = "/v1/playlists/" + playlistId + "/tracks";

            HttpResponseMessage response    = null;
            SpotifySongs spotifySongs       = new SpotifySongs();
            List<Track> tracks              = new List<Track>();
            List<PlaylistItem> itemsList    = new List<PlaylistItem>();
            try
            {
                response = await MusicClient.SpotifyApiGetAsync(api);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiGetAsync(api);
                }

                if (MusicClient.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    spotifySongs        = JsonConvert.DeserializeObject<SpotifySongs>(responseBody);
                    itemsList           = spotifySongs.items;
                    foreach (PlaylistItem item in itemsList)
                    {
                        tracks.Add(item.track);
                    }
                }

                
            }
            catch (Exception ex)
            {


            }
            return tracks;
        }
        
        public static async Task generateMyAIPlaylistAsync()
        {
            SpotifyUser spotifyUser         = null;
            HttpResponseMessage response    = null;
            spotifyUser                     = await UserProfile.getInstance.GetUserProfileAsync();

            JsonObject PlayListProperty     = new JsonObject();
            PlayListProperty.Add("name", "My AI Top 40");
            PlayListProperty.Add("public", false);

            String Payload = PlayListProperty.ToString();

            if (spotifyUser.Id == null)
            {
                return;
            }

            string api = "/v1/users/" + spotifyUser.Id + "/playlists";
            try
            {
                response = await MusicClient.SpotifyApiPostAsync(api, Payload);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPostAsync(api, Payload);
                }

                if (MusicClient.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                }

            }
            catch (Exception ex)
            {


            }

        }

      
        public static async Task<List<Track>> getTopSpotifyTracksAsync()
        {
            String api                      = "/v1/me/top/tracks?time_range=medium_term&limit=40";
            HttpResponseMessage response    = null;
            SpotifySongs spotifySongs       = new SpotifySongs();
            List<Track>  TopSongs           = new List<Track>();
            List<PlaylistItem> itemsList    = new List<PlaylistItem>();
            
            try
            {
                response = await MusicClient.SpotifyApiGetAsync(api);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiGetAsync(api);
                }

                if (MusicClient.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    spotifySongs        = JsonConvert.DeserializeObject<SpotifySongs>(responseBody);
                    itemsList           = spotifySongs.items;
                    foreach (PlaylistItem item in itemsList)
                    {
                        TopSongs.Add(item.track);
                    }
                }
                
            }
            catch (Exception ex)
            {
                
            }
            return TopSongs;

        }

        public static async Task<List<Track>> getSpotifyLikedSongsAsync()
        {
            string api = "/v1/me/tracks?limit=50&offset=0";

            HttpResponseMessage response    = null;
            SpotifySongs spotifySongs       = new SpotifySongs();
            List<Track> LikedSongs          = new List<Track>();
            List<PlaylistItem> itemsList    = new List<PlaylistItem>();
            try
            {
                response = await MusicClient.SpotifyApiGetAsync(api);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiGetAsync(api);
                }

                if (MusicClient.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    spotifySongs        = JsonConvert.DeserializeObject<SpotifySongs>(responseBody);
                    itemsList           = spotifySongs.items;

                    foreach (PlaylistItem item in itemsList)
                    {
                        LikedSongs.Add(item.track);
                    }
                }



            }
            catch (Exception ex)
            {


            }
            return LikedSongs;
        }

    }
}
