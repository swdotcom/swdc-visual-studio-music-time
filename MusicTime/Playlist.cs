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
        private static Playlist instance = null;
        public static MusicClient musicClient = MusicClient.getInstance;
        public static CodyConfig codyConfig = CodyConfig.getInstance;


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

        // public static List<PlaylistItem> _Playlists { get; set; }
        public static List<Track> Software_Playlists { get; set; }
        public static List<Track> Liked_Playlist { get; set; }
       public static List<PlaylistItem> _UsersPlaylists = new List<PlaylistItem>();
        public static int offset = 0;
        public static Dictionary<PlaylistItem, List<Track>> Users_Playlist = new Dictionary<PlaylistItem, List<Track>>();

        public static string PlayListID { get; set; }
        
        public static async Task<List<PlaylistItem>> getPlaylistsAsync()
        {
            List<PlaylistItem> _Playlists = new List<PlaylistItem>();
            if (codyConfig.spoftifyUserId!= null)
            {
                offset = 0;
                _UsersPlaylists.Clear();
               _Playlists = await getPlaylistsForUserAsync(codyConfig.spoftifyUserId);     
            }
            return _Playlists;
        }

        public static async Task<List<PlaylistItem>> getPlaylistsForUserAsync(string spotifyUserid)
        {
            HttpResponseMessage response    = null;
            string api                      = "/v1/users/"+ spotifyUserid + "/playlists?offset="+offset+"&limit=50";
            Logger.Debug("offset :" + offset);
            int playlistCount               = 0;
            SpotifySongs PlaylistItems      = new SpotifySongs();
           


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
                        _UsersPlaylists.Add(item);
                        playlistCount++;
                    }

                }
                if(playlistCount==50)
                {
                    offset += 50;
                    _UsersPlaylists = await getPlaylistsForUserAsync(codyConfig.spoftifyUserId);
                }

            }
            catch (Exception ex)
            {


            }
            return _UsersPlaylists;
        }

        //public static async Task<List<string>> getPlaylistNamesAsync()
        //{
        //    List<string> names                  = new List<string>();
        //    List<PlaylistItem> playlistItems    = null;
        //    playlistItems                       = await getPlaylistsAsync();

        //    foreach (PlaylistItem item in playlistItems)
        //    {
        //        names.Add(item.name);
        //    }

        //    return names;
        //}

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
        
        public static async Task<string> generateMyAIPlaylistAsync()
        {
            SpotifyUser spotifyUser         = null;
            HttpResponseMessage response    = null;
            spotifyUser                     = await UserProfile.getInstance.GetUserProfileAsync();
            SpotifySongs spotifySongs       = new SpotifySongs();

            try
            {

                JsonObject _payload         = new JsonObject();

                _payload.Add("name", Constants.PERSONAL_TOP_SONGS_NAME);
                _payload.Add("public", false);
                _payload.Add("description", "");

                string Payload = _payload.ToString();

                if (spotifyUser.Id == null)
                {
                    return PlayListID;
                }

                string api  = "/v1/users/" + spotifyUser.Id + "/playlists";

                response    = await MusicClient.SpotifyApiPostAsync(api, Payload);

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
                    spotifySongs        = JsonConvert.DeserializeObject<SpotifySongs>(responseBody);
                    PlayListID          = spotifySongs.id;
                }
                
            }
            catch (Exception ex)
            {


            }
            return PlayListID;
        }

   
        public static async Task<List<Track>> getTopSpotifyTracksAsync()
        {
            string api                      = "/v1/me/top/tracks?time_range=medium_term&limit=40";
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

      

        public static async Task AddTracksToPlaylistAsync(string playlist_id , List<string> trackUris , int postion=0)
        {
            string api                      = "/v1/playlists/"+playlist_id+"/tracks";
            HttpResponseMessage response    = null;
            string app_jwt                  = SoftwareUserSession.GetJwt();
            List<string> tracks             = null;
            string Payload                  = null;
            tracks                          = MusicUtil.createUrisFromTrackId(trackUris);

            JsonObject payload          = new JsonObject();
            payload.Add("uris",tracks);
            payload.Add("position", postion);

            Payload = payload.ToString();

            if (!string.IsNullOrEmpty(app_jwt))
            {
                response = await MusicClient.SpotifyApiPostAsync(api, Payload);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPostAsync(api, Payload);
                    Logger.Debug("Songs added to AI Playlist");
                }
            }

           

        }
        public static async Task ReplaceTrackToPlaylistsyncAsync(string playlist_id, List<string> trackUris)
        {
            string api                      = "/v1/playlists/" + playlist_id + "/tracks";
            HttpResponseMessage response    = null;
            string app_jwt                  = SoftwareUserSession.GetJwt();
            List<string> tracks             = null;
            string Payload                  = null;
            tracks                          = MusicUtil.createUrisFromTrackId(trackUris);


            JsonObject payload = new JsonObject();
            payload.Add("uris", tracks);

            Payload = payload.ToString();

            if (!string.IsNullOrEmpty(app_jwt))
            {
                response = await MusicClient.SpotifyApiPutAsync(api, Payload);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api, Payload);
                }
            }
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
                    itemsList = spotifySongs.items;

                    if (itemsList.Count > 0)
                    {
                        foreach (PlaylistItem item in itemsList)
                        {
                            LikedSongs.Add(item.track);
                        }
                    }
                }



            }
            catch (Exception ex)
            {


            }
           
           
            return LikedSongs;
        }


        public static async void addTracksToPlaylist(string playlist_id,string track_id)
        {
            string api                      = "/v1/playlists/" + playlist_id + "/tracks";
            HttpResponseMessage response    = null;
            string _payload                 = null;
            string trackUris                = MusicUtil.createUriFromTrackId(track_id);

            JsonObject payload = new JsonObject();
            payload.Add("uris", trackUris);
            payload.Add("postion", 0);

            _payload    = payload.ToString();
             response   = await MusicClient.SpotifyApiPostAsync(api, _payload);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPostAsync(api, _payload);
                }
            

        }

        public static async void removeTracksToPlaylist(string playlist_id, string track_id)
        {
            string api = "/v1/playlists/" + playlist_id + "/tracks";
            HttpResponseMessage response = null;
            string _payload = null;
            JsonObject trackPayload = new JsonObject();
            JsonObject payload = new JsonObject();

            string trackUri = MusicUtil.createUriFromTrackId(track_id);
            JsonArray jsonArray = new JsonArray();
            payload.Add("uri", trackUri);
            jsonArray.Add(payload);
            trackPayload.Add("tracks", jsonArray);
            
            _payload = trackPayload.ToString();
           
            response = await MusicClient.spotifyApiDeleteAsync(api, _payload);

            if (response == null || !response.IsSuccessStatusCode)
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.spotifyApiDeleteAsync(api, _payload);
            }


        }
    }
}
