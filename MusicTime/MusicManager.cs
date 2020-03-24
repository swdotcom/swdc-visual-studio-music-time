using Newtonsoft.Json;
using SoftwareCo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusicTime
{
    class MusicManager
    {
        private static MusicManager instance    = null;
        public static SpotifyUser spotifyUser   = new SpotifyUser();
        public static CodyConfig codyConfig     = CodyConfig.getInstance;
        private static Device device            = Device.getInstance;
        private static TrackStatus trackStatus  = new TrackStatus();
      
        private MusicManager()
        {
        }

        public static MusicManager getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MusicManager();
                }
                return instance;
            }
        }
        public static SpotifyUser _spotifyUser { get; set; }
        public  List<PlaylistItem> _usersPlaylists { get; set; }
       // public static List<Track> RecommendedTracks = new List<Track>();
       
        public async Task UpdateSpotifyAccessInfoAsync(Auths auths, SpotifyTokens spotifyTokens)
        {

            try
            {

                if (auths != null)
                {
                    codyConfig.spotifyClientId      = spotifyTokens.clientId;
                    codyConfig.spotifyClientSecret  = spotifyTokens.clientSecret;
                    codyConfig.spotifyAccessToken   = auths.AccessToken;
                    codyConfig.spotifyRefreshToken  = auths.RefreshToken;
                    codyConfig.enableSpotifyApi     = SoftwareCoUtil.isMac() ? true : false;

                    codyConfig.setConfig(codyConfig);

                    SoftwareCoUtil.setItem("spotify_access_token", auths.AccessToken);
                    SoftwareCoUtil.setItem("spotify_refresh_token", auths.RefreshToken);

                    UserProfile userProfile = UserProfile.getInstance;
                    _spotifyUser            = await userProfile.GetUserProfileAsync();
                    Logger.Debug(_spotifyUser.Id.ToString());
                    bool isConnected        = hasSpotifyPlaybackAccess();
                    await getDevicesAsync();
                   
                }
                else
                {
                    clearSpotifyAccessInfo(spotifyTokens);
                }

            }
            catch (Exception ex)
            {


            }


        }

        internal static async Task<bool> isTrackPlayingAsync()
        {
            bool isTrackPlaying = false;
            trackStatus = await SpotifyCurrentTrackAsync();
            if (trackStatus.is_playing == true)
            {
                isTrackPlaying = true;
            }

            return isTrackPlaying;
        }

        public static async Task<string> CurrentTrackAsync()
        {
            trackStatus = await SpotifyCurrentTrackAsync();
            return trackStatus.item.name.ToString();
        }

        public static async Task getDevicesAsync()
        {
            bool isConnected = hasSpotifyPlaybackAccess();
            if (isConnected)
            {
                device = await MusicClient.GetDeviceAsync();
            }

        }

        public static async Task UpdateSlackAccesInfoAsync(Auths auths)
        {
            if(auths !=null)
            {
                SoftwareCoUtil.setItem("slack_access_token", auths.AccessToken);
            }
            else
            {
                SoftwareCoUtil.setItem("slack_access_token", null);
            }
        }

        public static List<Device> getDevices()
        {
            List<Device> devices = null;
            if (device.devices != null)
            {
                return devices= device.devices;
            }
            return devices;
        }



        public static string getDeviceNames()
        {
            string deviceNames = "";
            if (device.devices != null)
            {
                if(device.devices.Count==0)
                {
                    deviceNames = device.devices[0].name;
                }
                if(device.devices.Count>0)
                {

                    foreach (Device item in device.devices)
                    {
                        deviceNames = deviceNames + "," + item.name;

                    }
                   return deviceNames.TrimStart(new char[] { ',' });
                }

               
                return deviceNames;
            }
            return deviceNames;
        }

        internal static async Task<string> getGenreAsync(string artistName, string songName, string artistId)
        {
           string genre = await GetGenreFromSpotifyAsync(artistName, artistId);
           return genre;
        }

        private static async Task<string> GetGenreFromSpotifyAsync(string artist, string spotifyArtistId)
        {
            string api = "";
            HttpResponseMessage response = null;
            if (!string.IsNullOrEmpty(spotifyArtistId))
            {
                api = "/v1/artists/"+spotifyArtistId;
            }
            else
            {
                // use the search api
                string qParam = "artist:"+artist;
                string qryStr = "="+qParam + "&type=artist&limit=1";
                api = "/v1/search?"+qryStr;
                
            }
           
            response = await MusicClient.SpotifyApiGetAsync(api);

            if (response == null || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiGetAsync(api);
            }

            if (MusicClient.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
               
            }


            return "";


        }

        internal static string getArtist(Track songSession)
        {
            return "";
        }
        public static async Task<Artist> getSpotifyArtistByIdAsync(string id)
        {
            Artist artist = null;
            HttpResponseMessage response = null;
            string ArtistId = MusicUtil.CreateSpotifyIdFromUri(id);
            string api = "/v1/artists/"+ ArtistId;

            response = await MusicClient.SpotifyApiGetAsync(api);

            if (response == null || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiGetAsync(api);
            }

            if (MusicClient.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                artist = JsonConvert.DeserializeObject<Artist>(responseBody);
            }

            return artist;

        }
        public static string getActiveDeviceName()
        {
            string deviceNames = "";
            if (device.devices != null)
            {
                foreach (Device item in device.devices)
                {
                    if (item.is_active == true && item.type =="Computer")
                    { deviceNames = item.name; }
                }
            }
            return deviceNames;
        }
        public static string getActiveDeviceID()
        {
            string activeDevice = null;
            if (device != null)
            {
                if (device.devices != null)
                {
                    foreach (Device item in device.devices)
                    {
                        if ((item.is_active == true && item.type == "Computer")|| (item.is_active==false &&item.type=="Computer"))
                        { activeDevice = item.id; break; }
                    }
                }
            }
            return activeDevice;
        }
        public static bool isDeviceOpened()
        {
            bool isDeviceOpened = false;
            if (device.devices != null )
            {
                foreach (Device item in device.devices)
                {
                    if ( item.type == "Computer")
                    { isDeviceOpened = true; }
                }
               // isDeviceOpened = device.devices.Count > 0 ? true : false;
            }

            return isDeviceOpened;
        }

        public static string DeviceID()
        {
            string currentDeviceId = null;

            if (device.devices.Count > 0)
            {
                currentDeviceId = device.devices[0].id;
            }
            return currentDeviceId;
        }

        public static void clearSpotifyAccessInfo(SpotifyTokens spotifyTokens)
        {
            CodyConfig codyConfig = CodyConfig.getInstance;
            codyConfig.spotifyClientId = spotifyTokens.clientId;
            codyConfig.spotifyClientSecret = spotifyTokens.clientSecret;
            codyConfig.spotifyAccessToken = null;
            codyConfig.spotifyRefreshToken = null;
            codyConfig.enableSpotifyApi = SoftwareCoUtil.isMac() ? true : false;
            codyConfig.setConfig(codyConfig);
            SoftwareCoUtil.setItem("spotify_access_token", null);
            SoftwareCoUtil.setItem("spotify_refresh_token", null);
            _spotifyUser = null;
            Logger.Debug(spotifyTokens.clientId);
        }

        public static bool hasSpotifyPlaybackAccess()
        {

            if (_spotifyUser != null && _spotifyUser.Product == "premium")
            {

                return true;
            }
            return false;
        }

        public static async Task SpotifyWebPlayAsync()
        {
            HttpResponseMessage response = null;

            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
                string api = "/v1/me/player/play?" + DeviceID();

                response = await MusicClient.SpotifyApiPutAsync(api,null);
                if (response == null || !MusicClient.IsOk(response))
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api,null);
                }


            }


        }

        public static async Task SpotifyWebPauseAsync()
        {
            HttpResponseMessage response = null;


            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
                string api = "/v1/me/player/pause?" + DeviceID();

                response = await MusicClient.SpotifyApiPutAsync(api,null);
                if (response == null || !MusicClient.IsOk(response))
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api,null);
                }

            }


        }

        public static async Task SpotifyWebPlayNextAsync()
        {
            HttpResponseMessage response = null;


            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
                string api = "/v1/me/player/next?" + DeviceID();

                response = await MusicClient.SpotifyApiPostAsync(api, null);
                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPostAsync(api, null);
                }

            }


        }

        public static async Task SpotifyWebPlayPreviousAsync()
        {
            HttpResponseMessage response = null;

            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
                string api = "/v1/me/player/previous?" + DeviceID();

                response = await MusicClient.SpotifyApiPostAsync(api, null);
                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPostAsync(api, null);
                }

            }
           
        }

        public static async Task SpotifyTransferDevice(string device_id )
        {

            HttpResponseMessage response = null;
            JsonObject payload = new JsonObject();
            string[] stringArray = new string[] { device_id };
            payload.Add("device_ids", stringArray);
            payload.Add("play", true);

           string _payload = payload.ToString();
            if (!string.IsNullOrEmpty(device_id))
            {
              string  api = "/v1/me/player";

                response = await MusicClient.SpotifyApiPutAsync(api, _payload);

                Logger.Debug(_payload);

                if (response == null || !MusicClient.IsOk(response))
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api, _payload);

                }

            }

        }
        public static async Task SpotifyPlayPlaylistold(string PlaylistId, string trackid,List<Track> tracklist)
        {
           
            string api             = null;
            List<string> track_ids = new List<string>();
            JsonObject payload     = new JsonObject();
            int index;
            if (tracklist != null && tracklist.Count > 0)
            {
                if (trackid != null)
                {
                    JsonArray track_uris = new JsonArray();

                    foreach (Track item in tracklist)
                    {
                       // track_uris.Add(MusicUtil.createUriFromTrackId(item.id));
                        track_ids.Add(item.id);
                    }
                   index = track_ids.IndexOf(trackid);


                    if(index<=50)
                    {
                        tracklist = tracklist.GetRange(0,50);

                    }

                    else
                    {
                        tracklist = (List<Track>)tracklist.GetRange(50,50);
                    }
                    track_ids.Clear();
                    foreach (Track item in tracklist)
                    {
                         track_uris.Add(MusicUtil.createUriFromTrackId(item.id));
                       track_ids.Add(item.id);
                    }
                     index = track_ids.IndexOf(trackid);

                    payload.Add("uris", track_uris);

                   


                    if (index >= 0)
                    {
                        JsonObject position = new JsonObject();
                        position.Add("position", index);
                        payload.Add("offset", position);
                    }
                }
            }
            string _payload = payload.ToString();

            await getDevicesAsync();

            HttpResponseMessage response = null;

            if (!string.IsNullOrEmpty(getActiveDeviceID()))
            {
                api = "/v1/me/player/play?device_id=" + getActiveDeviceID();
                Logger.Debug("Active device id" + getActiveDeviceID());

                response = await MusicClient.SpotifyApiPutAsync(api, _payload);

                if (response == null || !(response.StatusCode == HttpStatusCode.NoContent) && !MusicClient.IsOk(response))
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api, _payload);

                }

            }
            MusicTimeCoPackage.UpdateCurrentTrackOnStatusAsync(null);
            MusicStateManager.getInstance.GatherMusicInfo();
            

        }



        public static async Task SpotifyPlayPlaylist(string PlaylistId, string trackid, List<Track> tracklist)
        {

            string api = null;
            List<string> track_ids = new List<string>();
            JsonObject payload = new JsonObject();
           
            if (tracklist != null && tracklist.Count > 0)
            {
                if (trackid != null)
                {
                    JsonArray track_uris = new JsonArray();

                    foreach (Track item in tracklist)
                    {
                        track_uris.Add(MusicUtil.createUriFromTrackId(item.id));
                        track_ids.Add(item.id);
                    }


                    int index = track_ids.IndexOf(trackid);

                    payload.Add("uris", track_uris);
                    
                    if (index >= 0)
                    {
                        JsonObject position = new JsonObject();
                        position.Add("position", index);
                        payload.Add("offset", position);
                    }
                }
            }
            string _payload = payload.ToString();

            await getDevicesAsync();

            HttpResponseMessage response = null;

            if (!string.IsNullOrEmpty(getActiveDeviceID()))
            {
                api = "/v1/me/player/play?device_id=" + getActiveDeviceID();
                Logger.Debug("Active device id" + getActiveDeviceID());

                response = await MusicClient.SpotifyApiPutAsync(api, _payload);

                if (response == null || !(response.StatusCode == HttpStatusCode.NoContent) && !MusicClient.IsOk(response))
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api, _payload);

                }

            }
            MusicTimeCoPackage.UpdateCurrentTrackOnStatusAsync(null);
            MusicStateManager.getInstance.GatherMusicInfo();


        }
        public static async Task SpotifyPlayPlaylistAsync(string PlaylistId,string trackid)
        {
            string payload      = string.Empty;
            Payload _payload    = new Payload();
            TrackUris trackUris = new TrackUris();
            string api = null;
            if (!string.IsNullOrEmpty(PlaylistId))
            {
               
                _payload.ContextUri = "spotify:playlist:" + PlaylistId;
                
                if (!string.IsNullOrEmpty(trackid))
                {
                    Offset offset = new Offset();
                    offset.Uri = "spotify:track:" + trackid;
                    _payload.Offset = offset;
                }
                payload = _payload.ToJson();
            }
            else
            {
                if (!string.IsNullOrEmpty(trackid))
                {
                    string trackID = "spotify:track:" + trackid;
                    string[] stringArray = new string[] { trackID };
                    trackUris.Uris = stringArray;
                    payload = trackUris.ToJson();
                }
            }


            await getDevicesAsync();

            HttpResponseMessage response = null;
           
            if (!string.IsNullOrEmpty(getActiveDeviceID()))
            {
               api  = "/v1/me/player/play?device_id=" + getActiveDeviceID();
                Logger.Debug("Active device id" + getActiveDeviceID());

                response = await MusicClient.SpotifyApiPutAsync(api,payload);
              
                if (response == null || !(response.StatusCode == HttpStatusCode.NoContent) && !MusicClient.IsOk(response) )
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api, payload);
                    
                }
                
            }
            MusicTimeCoPackage.UpdateCurrentTrackOnStatusAsync(null);
            MusicStateManager.getInstance.GatherMusicInfo();
            //SoftwareCoUtil.SetTimeout(5000, MusicStateManager.getInstance.GatherMusicInfo, true);
            
        }

        public static async Task<TrackStatus> SpotifyCurrentTrackAsync()
        {
            HttpResponseMessage response    = null;

            string api  = "/v1/me/player/currently-playing" /*+ getActiveDeviceID()*/;

            response    = await MusicClient.SpotifyApiGetAsync(api);
            
            if (response == null || !(response.StatusCode == HttpStatusCode.NoContent) || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiGetAsync(api);
            }

            if (MusicClient.IsOk(response) || response.StatusCode == HttpStatusCode.NoContent)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                trackStatus         = JsonConvert.DeserializeObject<TrackStatus>(responseBody);
            }
            return trackStatus;
        }
        public  async Task<Track> GetCurrentTrackAsync()
        {
            HttpResponseMessage response = null;
            TrackStatus track = new TrackStatus();
            string api = "/v1/me/player/currently-playing?" + getActiveDeviceID();

            response = await MusicClient.SpotifyApiGetAsync(api);

            if (response == null || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiGetAsync(api);
            }

            if (MusicClient.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                track = JsonConvert.DeserializeObject<TrackStatus>(responseBody);
               
            }
            if(track.item!=null)
            {
                track.item.progress_ms = track.progress_ms;
            }
            if (track.item !=null)
            {
                track.item.state = trackState.NotAssigned;
            }
            if (track.item!=null && track.item.uri!=null)
            {
                if (track.item.uri.Contains("spotify:ad:"))
                {
                    track.item.state = trackState.Advertisement;
                }
                else
                {
                    TrackStatus context = new TrackStatus();
                    context             = await getSpotifyPlayerContextAsync();
                  
                    if (context!=null && context.is_playing)
                    {
                        track.item.state = trackState.Playing;
                        track.item.type = context.item.type;
                    }
                    else
                    {
                        track.item.state = trackState.Paused;
                        track.item.type = context.item.type;
                    }
                }
            }
            
            if(track.item!=null && track.item.available_markets!=null)
            {
                track.item.available_markets = null;
            }

            return track.item;
        }

        private async Task<TrackStatus> getSpotifyPlayerContextAsync()
        {
            TrackStatus playerContext   = new TrackStatus();

            HttpResponseMessage response = null;
         
            string api  = "/v1/me/player";

            response    = await MusicClient.SpotifyApiGetAsync(api);

            if (response == null || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiGetAsync(api);
            }

            if (MusicClient.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                playerContext       = JsonConvert.DeserializeObject<TrackStatus>(responseBody);

                playerContext.item.type         = "Spotify";
                playerContext.item.player_type  = PlayerType.WebSpotify;

                playerContext.item = extractAristFromSpotifyTrack(playerContext.item);
            }
            return playerContext;
        }

        public static async Task<List<Track>> getAITop40TracksAsync()
        {
            string responseBody             = null;
            HttpResponseMessage response    = null;
            string app_jwt                  = SoftwareUserSession.GetJwt();
           
            List<Track> TopSongs            = new List<Track>();
            Track spotifySongs              = new Track();

            string api                      = "/music/recommendations?limit=40";

            if (!string.IsNullOrEmpty(app_jwt))
            {
                response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, api, "", app_jwt);

                if (SoftwareHttpManager.IsOk(response))
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                    TopSongs = SimpleJson.DeserializeObject<List<Track>>(responseBody);
                  //  TopSongs = JsonConvert.DeserializeObject<List<Track>>(responseBody);

                }

            }

            return TopSongs;

        }
        public static async Task<Track> GetSpotifyTrackByIdAsync(string trackId, bool includeFullArtist, bool includeAudioFeatures, bool includeGenre)
        {
            Track track =  new Track();
            HttpResponseMessage response = null;
            string id  = MusicUtil.CreateSpotifyIdFromUri(trackId);
            string api = "/v1/tracks/"+id;

            response = await MusicClient.SpotifyApiGetAsync(api);

            if (response == null || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiGetAsync(api);
            }

            if (MusicClient.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                track = JsonConvert.DeserializeObject<Track>(responseBody);
                track = copySpotifyTrackToCodyTrack(track);
            }
          
            if(includeFullArtist && track.artists!=null)
            {
                List<Artist> artists = new List<Artist>();

                for (int i = 0; i < track.artists.Count; i++)
                {
                    Artist artist = await getSpotifyArtistByIdAsync(track.artists[i].id);
                    if (artist != null)
                    {
                        artists.Add(artist);
                    }
                }

                if (artists.Count > 0)
                {
                    track.artists = artists;
                }
                else
                    track.artists = artists;

                     

            }
            if(includeGenre && track.genre==null)
            {

                string genre = null;
                if (track.artists !=null  &&
                    track.artists.Count > 0)
                {
                    // make sure we use the highest frequency genre
                //    genre = await getHighestFrequencySpotifyGenre(track.artists[0].genres);
                    
                }
                if (genre ==null)
                {
                    // get the genre
                  //  genre = await getGenre(track.artist,track.name);
                    
                }
                if (genre != null)
                {
                    track.genre = genre;
                }
            }
            if(includeAudioFeatures)
            {
                SpotifyAudioFeature spotifyAudioFeatures = await getSpotifyAudioFeaturesAsync(trackId);
              
                track.features = spotifyAudioFeatures.AudioFeatures[0] ;
            }

            return track;
        }

        //public static Task<string> getHighestFrequencySpotifyGenre(List<string> genres)
        //{
        //    string selectedGenre = "";

        //    if(genres.Count==0)
        //    {
        //        return selectedGenre;
        //    }

        //}

        private static Task<string> getGenre(string artist, string name)
        {
            throw new NotImplementedException();
        }



        private static Track copySpotifyTrackToCodyTrack(Track spotifyTrack)
        {
            if (spotifyTrack.album != null)
            {
                spotifyTrack.album.available_markets = null;
                spotifyTrack.album.external_urls = null;
            }
            if (spotifyTrack.external_urls != null)
            {
                spotifyTrack.external_urls = null;
            }

            if (spotifyTrack.external_ids!= null)
            {
               spotifyTrack.external_ids = null;
            }
            spotifyTrack =  extractAristFromSpotifyTrack(spotifyTrack);

            if(spotifyTrack.duration_ms!=0)
            {
                spotifyTrack.duration = spotifyTrack.duration_ms;
            }

            spotifyTrack.type = "Spotify";
            return spotifyTrack;
        }

        private static Track extractAristFromSpotifyTrack(Track track_Data)
        {
            if(track_Data.artists.Count > 0)
            {
                List<string> artist_name = new List<string>();
                for (int i = 0; i < track_Data.artists.Count; i++)
                {
                    artist_name.Add(track_Data.artists[i].name);
                    track_Data.artists[i].href = null;
                    track_Data.artists[i].external_urls = null;
                    track_Data.artists[i].type = null;
                }

                if(artist_name.Count>0)
                {
                    track_Data.artist_names = artist_name;
                }
                if (artist_name.Count > 0)
                {
                    if (artist_name.Count > 1)
                    {
                        string artist_val = "";
                        for (int i = 0; i < artist_name.Count; i++)
                        {
                            artist_val = artist_val + artist_name[i]+ ",";

                        }
                        char[] charsToTrim  = { ',' };
                        track_Data.artist   = artist_val.TrimEnd(charsToTrim);
                        
                    }
                    else
                    track_Data.artist = artist_name[0];
                }
                
            }
            return track_Data;
    
        }

        public static async Task<SpotifyAudioFeature> getSpotifyAudioFeaturesAsync(string trackId)
        {
            SpotifyAudioFeature spotifyAudioFeature = new SpotifyAudioFeature();
            HttpResponseMessage response            = null;

             string id      = MusicUtil.CreateSpotifyIdFromUri(trackId);
             string qstr    = "?ids="+id;
             string api     = "/v1/audio-features"+qstr;
          
            response    = await MusicClient.SpotifyApiGetAsync(api);

            if (response == null || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiGetAsync(api);
            }

            if (MusicClient.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                
                spotifyAudioFeature = JsonConvert.DeserializeObject<SpotifyAudioFeature>(responseBody);
            }

            return spotifyAudioFeature;
        }

        public static async Task SeedSongsToPlaylistAsync(string playListId)
        {
            List<Track> SwtopSongs  = null;
            List<string> Uris       = new List<string>();
            try
            {
                SwtopSongs = await getAITop40TracksAsync();

                if (SwtopSongs != null && SwtopSongs.Count > 0)
                {

                    foreach (Track item in SwtopSongs)
                    {
                        Uris.Add(item.id);
                    }

                    await Playlist.AddTracksToPlaylistAsync(playListId, Uris, 0);
                }
            }
            catch (Exception ex)
            {


            }
        }
        public static async Task RefreshSongsToPlaylistAsync(string playListId)
        {
            List<Track> SwtopSongs  = null;
            List<string> Uris       = new List<string>();
            try
            {
                SwtopSongs          = await getAITop40TracksAsync();

                if (SwtopSongs != null && SwtopSongs.Count > 0)
                {

                    foreach (Track item in SwtopSongs)
                    {
                        Uris.Add(item.id);
                    }

                    await Playlist.ReplaceTrackToPlaylistsyncAsync(playListId, Uris);
                }
            }
            catch (Exception ex)
            {


            }
        }

        public static async Task UpdateSavedPlaylistsAsync(string playlist_id,int playlistTypeId, string name)
        {
            string responseBody             = null;
            HttpResponseMessage response    = null;
            string app_jwt                  = SoftwareUserSession.GetJwt();

            JsonObject payload = new JsonObject();
            payload.Add("playlist_id", playlist_id);
            payload.Add("playlistTypeId", playlistTypeId);
            payload.Add("name", name);
            string Payload      = payload.ToString();

            string api          = "/music/playlist/generated";

            if (!string.IsNullOrEmpty(app_jwt))
            {
                response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Post, api, Payload, app_jwt);
                if (SoftwareHttpManager.IsOk(response))
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                 
                }

            }

        }

        public static async Task<string> FetchSavedPlayListAsync()
        {
            HttpResponseMessage response    = null;
            string responseBody             = null;
            string app_jwt                  = SoftwareUserSession.GetJwt();
            string api                      = "/music/playlist/generated";
            string AIPlaylistId             = null;

            List< AiGeneratedPlaylistItem> AIPlyalist  = new List<AiGeneratedPlaylistItem>();

            if (!string.IsNullOrEmpty(app_jwt))
            {
                    response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, api, "", app_jwt);

                    if (MusicClient.IsOk(response))
                    {
                        responseBody    = await response.Content.ReadAsStringAsync();
                        AIPlyalist      = JsonConvert.DeserializeObject<List<AiGeneratedPlaylistItem>>(responseBody);

                        foreach (AiGeneratedPlaylistItem item in AIPlyalist)
                        {
                            AIPlaylistId = item.playlist_id;
                            break;
                        }
                       
                    }

            }

            return AIPlaylistId;

        }

        public static async Task GetMusicTimeDashboardFileAsync()
        {
            HttpResponseMessage response    = null;
            string responseBody             = null;
            string app_jwt                  = SoftwareUserSession.GetJwt();
            string api                      = "/dashboard/music";
            string dashboardFile            = SoftwareCoUtil.getDashboardFile();
            if (!string.IsNullOrEmpty(app_jwt))
            {
                response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, api, "", app_jwt);

                if (SoftwareHttpManager.IsOk(response))
                {
                    responseBody = await response.Content.ReadAsStringAsync();


                    if (File.Exists(dashboardFile))
                    {
                        File.SetAttributes(dashboardFile, FileAttributes.Normal);
                    }

                    try
                    {

                        File.WriteAllText(dashboardFile, responseBody);
                        
                    }
                    catch (Exception e)
                    {


                    }
                }

            }

        }

        

        public static async Task saveToSpotifyLiked (string trackID)
        {
            HttpResponseMessage response    = null;
            string responseBody             = null;
            string api                      = "/v1/me/tracks";
            JsonObject payload      = new JsonObject();
            string[] stringArray    = new string[] { trackID };
            payload.Add("ids", stringArray);
            string _payload = payload.ToString();

            response = await MusicClient.SpotifyApiPutAsync(api, _payload);
            if (response == null || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiPutAsync(api, _payload);
            }

            MusicTimeCoPackage.UpdateCurrentTrackOnStatusAsync(null);
        }

        public static async Task removeToSpotifyLiked(string trackID)
        {
            HttpResponseMessage response    = null;
            string responseBody             = null;
            string api                      = "/v1/me/tracks";
            JsonObject payload              = new JsonObject();
            string[] stringArray            = new string[] { trackID };
            payload.Add("ids", stringArray);
            string _payload = payload.ToString();

            response = await MusicClient.spotifyApiDeleteAsync(api, _payload);
            if (response == null || !MusicClient.IsOk(response))
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.spotifyApiDeleteAsync(api, _payload);
            }

            if(MusicClient.IsOk(response))
            {
                string message = "Removed song from your Liked Songs playlist";
                MessageBox.Show(message,"Spotify"); 
            }

            
            MusicTimeCoPackage.UpdateCurrentTrackOnStatusAsync(null);

        }

       
        public static async Task<List<Track>> getRecommendationsForTracks(string value ,bool offset_flag )
        {
           
           
            HttpResponseMessage response    = null;
            string responseBody             = null;
            string api                      = "/v1/recommendations";
            string query = string.Empty;
            List <string> seed_tracks_list  = new List<string>();
            string seed_tracks = "";
            List<string>  seed_genres    = new List<string>();
            List<string>  seed_artists   = new List<string>();
            int limit                   = 100    ;
            string market               = "";
            int min_popularity          = 20;
            int target_popularity       = 90;
            string mood                 = string.Empty;
            JsonObject queryParams      = new JsonObject();
            List<Track> likedTracks     = new List<Track>();
            List<Track> Tracks          = new List<Track>();
            TrackList trackList         = new TrackList();
            List<string> trackIds       = new List<string>();
            MusicTimeCoPackage.isOffsetChange = offset_flag;
            if (MusicTimeCoPackage.RecommendedType != value  )
            {
                MusicTimeCoPackage.RecommendedType = value;
                MusicTimeCoPackage.RecommendedTracks.Clear();
                
                if (Constants.spotifyMoods.Contains(value))
                {
                    
                        if (Playlist.Liked_Tracks.Count <= 0)
                        {
                            likedTracks = await Playlist.getSpotifyLikedSongsAsync();
                        }
                        else
                            likedTracks = Playlist.Liked_Tracks;



                        if (likedTracks.Count <= 5)
                        {
                            if (Playlist.SoftwareTOP_Tracks.Count <= 0)
                            {
                                likedTracks = await Playlist.getPlaylistTracksAsync(Constants.SOFTWARE_TOP_40_ID);
                            }
                            else
                                likedTracks = Playlist.SoftwareTOP_Tracks;
                        }

                        if (likedTracks.Count > 0)
                        {
                            likedTracks = likedTracks.GetRange(0, 5);

                            foreach (Track item in likedTracks)
                            {
                                trackIds.Add(item.id);
                            }

                            seed_tracks = string.Join(",", trackIds.ToArray(), 0, 5);
                        }

                        switch (value)
                        {
                            case "Happy":
                                mood = "&min_valence=0.7&target_valence=1";
                                break;
                            case "Energetic":
                                mood = "&min_energy=0.7&target_energy=1";
                                break;
                            case "Danceable":
                                mood = "&min_danceability=0.7&target_danceability=1";
                                break;
                            case "Instrumental":
                                mood = "&min_instrumentalness=0.0&target_instrumentalness=0.1";
                                break;
                            case "Quiet":
                                mood = "&max_loudness=-5&target_loudness=-10";
                                break;
                            default:
                                break;
                        }
                    }
                    if (!string.IsNullOrEmpty(mood))
                    {
                        query = api + "?limit=" + limit + "&min_popularity=" + min_popularity + "&target_popularity=" + target_popularity + "&seed_tracks=" + seed_tracks + "" + mood;
                    }
                    else if (value == "Familiar")
                    {
                        query = api + "?limit=" + limit + "&min_popularity=" + min_popularity + "&target_popularity=" + target_popularity + "&seed_tracks=" + seed_tracks;
                    }
                    else
                    {
                        query = api + "?limit=" + limit + "&min_popularity=" + min_popularity + "&target_popularity=" + target_popularity + "&seed_genres=" + value.ToLower();
                    }

                    try
                    {

                        response = await MusicClient.SpotifyApiGetAsync(query);

                        if (response == null || !MusicClient.IsOk(response))
                        {
                            // refresh the tokens
                            await MusicClient.refreshSpotifyTokenAsync();
                            // Try again
                            response = await MusicClient.SpotifyApiGetAsync(query);
                        }

                        if (MusicClient.IsOk(response))
                        {
                            responseBody = await response.Content.ReadAsStringAsync();
                            trackList = JsonConvert.DeserializeObject<TrackList>(responseBody);

                            MusicTimeCoPackage.RecommendedTracks = trackList.tracks;
                        }

                    }
                    catch (Exception ex)
                    {


                    }
            }
            if (MusicTimeCoPackage.RecommendedTracks.Count > 50)
            {
                if (MusicTimeCoPackage.isOffsetChange == false)
                {
                    Tracks = MusicTimeCoPackage.RecommendedTracks.GetRange(0, 50);
                }
                else if (MusicTimeCoPackage.isOffsetChange == true)
                {
                    Tracks = MusicTimeCoPackage.RecommendedTracks.GetRange(50, 50);
                }

            }
            else
            {
                Tracks = MusicTimeCoPackage.RecommendedTracks;
            }

            return Tracks;
        }
    }
}
