using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{
    class MusicManager
    {
        private static MusicManager instance    = null;

        public static SpotifyUser spotifyUser   = new SpotifyUser();
        public static CodyConfig codyConfig     = CodyConfig.getInstance;
        private static Device device            = Device.getInstance;
        private static TrackStatus trackStatus = new TrackStatus();
        private MusicManager()
        {
        }

        public static MusicManager GetInstance
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

        public async Task UpdateSpotifyAccessInfoAsync(Auths auths, SpotifyTokens spotifyTokens)
        {

            try
            {
               
                if (auths != null)
                {
                    
                   
                    codyConfig.spotifyClientId          = spotifyTokens.clientId;
                    codyConfig.spotifyClientSecret      = spotifyTokens.clientSecret;
                    codyConfig.spotifyAccessToken       = auths.AccessToken;
                    codyConfig.spotifyRefreshToken      = auths.RefreshToken;
                    codyConfig.enableSpotifyApi         = SoftwareCoUtil.isMac() ? true : false;

                    codyConfig.setConfig(codyConfig);

                    SoftwareCoUtil.setItem("spotify_access_token", auths.AccessToken);
                    SoftwareCoUtil.setItem("spotify_refresh_token", auths.RefreshToken);

                    UserProfile userProfile             = UserProfile.getInstance;
                    spotifyUser                         = await userProfile.GetUserProfileAsync();

                    bool isConnected = MusicManager.hasSpotifyPlaybackAccess();
                    await getDevicesAsync();
                    MusicTimeCoPackage.GetSpotifyUserStatus(isConnected);

                }
                else
                {
                    MusicManager.cleaclearSpotifyAccessInfo(spotifyTokens);
                }

            }
            catch (Exception ex)
            {

                
            }


        }

        internal static async Task<bool> isTrackPlayingAsync()
        {
            bool isTrackPlaying     = false;
            trackStatus             = await SpotifyCurrentTrackAsync();
            if(trackStatus.is_playing == true)
            {
                isTrackPlaying = true;
            }

            return isTrackPlaying;
        }

        public static async Task<string> CurrentTrack()
        {
            trackStatus = await SpotifyCurrentTrackAsync();
            return trackStatus.item.name.ToString();
        }

        public static async Task getDevicesAsync()
        {
           bool isConnected = MusicManager.hasSpotifyPlaybackAccess();
            if(isConnected)
            {
                device = await MusicClient.GetDeviceAsync();
            }
            
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
            string currentDeviceId  = null;

            if (device.devices.Count > 0)
            {
                currentDeviceId = device.devices[0].id;
            }
            return currentDeviceId;
        }


       

        public static void cleaclearSpotifyAccessInfo(SpotifyTokens spotifyTokens)
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
            spotifyUser = null;

        }

       public static bool hasSpotifyPlaybackAccess()
        {
      
            if (spotifyUser!= null && spotifyUser.Product == "premium")
            {
                return true;
            }
            return false;
        }

        public static async Task SpotifyWebPlayAsync()
        {
            HttpResponseMessage response      = null;
            
            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
               string api = "/v1/me/player/play?" + DeviceID();

                response    = await MusicClient.SpotifyApiPutAsync(api);
                
                if (response == null || !MusicClient.IsOk(response))
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api);
                }


            }
            
            
        }

        public static async Task SpotifyWebPauseAsync()
        {
            HttpResponseMessage response = null;
          

            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
                string api = "/v1/me/player/pause?" + DeviceID();

                response = await MusicClient.SpotifyApiPutAsync(api);

                if (response == null || !MusicClient.IsOk(response))
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPutAsync(api);
                }

            }


        }

        public static async Task SpotifyWebPlayNextAsync()
        {
            HttpResponseMessage response = null;
           

            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
                string api = "/v1/me/player/next?" + DeviceID();

                response = await MusicClient.SpotifyApiPostAsync(api);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPostAsync(api);
                }

            }


        }
        public static async Task SpotifyWebPlayPreviousAsync()
        {
            HttpResponseMessage response = null;
            
            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
                string api = "/v1/me/player/previous?" + DeviceID();

                response = await MusicClient.SpotifyApiPostAsync(api);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response = await MusicClient.SpotifyApiPostAsync(api);
                }

            }


        }


        public static async Task<TrackStatus> SpotifyCurrentTrackAsync()
        {
            HttpResponseMessage response    = null;

            string api  = "/v1/me/player/currently-playing?" + DeviceID();

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
                trackStatus = JsonConvert.DeserializeObject<TrackStatus>(responseBody);
            }
            return trackStatus;
        }
    }
}
