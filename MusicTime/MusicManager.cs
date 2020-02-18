using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{
    class MusicManager
    {
        private static MusicManager instance = null;

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
                   
                  //  SoftwareUserSession.GetSpotifyUserStatusTokenAsync(isConnected);

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
            if (device.devices != null)
            {
                foreach (Device item in device.devices)
                {
                    if (item.is_active == true || item.type == "Computer")
                    { activeDevice = item.id; break; }
                }
            }
            return activeDevice;
        }
        public static bool isDeviceOpened()
        {
            bool isDeviceOpened = false;
            if (device.devices != null)
            {
                isDeviceOpened = device.devices.Count > 0 ? true : false;
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

                response = await MusicClient.SpotifyApiPutAsync(api,payload);
              
                if (response == null || !MusicClient.IsOk(response))
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api, payload);
                    
                }
                
            }
          

        }

        public static async Task<TrackStatus> SpotifyCurrentTrackAsync()
        {
            HttpResponseMessage response    = null;

            string api  = "/v1/me/player/currently-playing?" + getActiveDeviceID();

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
                trackStatus         = JsonConvert.DeserializeObject<TrackStatus>(responseBody);
            }
            return trackStatus;
        }
        public  async Task<Track> GetCurrentTrackAsync()
        {
            HttpResponseMessage response = null;
            Track track = null;
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
                track = JsonConvert.DeserializeObject<Track>(responseBody);
            }
            return track;
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
                    TopSongs = JsonConvert.DeserializeObject<List<Track>>(responseBody);

                }

            }

            return TopSongs;

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

        public  async Task<List<PlaylistItem>> GetAllPlaylistAsync()
        {
           _usersPlaylists  = await Playlist.getPlaylistsAsync();

            return _usersPlaylists;
        }
    }
}
