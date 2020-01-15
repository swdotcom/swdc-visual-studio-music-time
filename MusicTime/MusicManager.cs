﻿using Newtonsoft.Json;
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
        private static MusicManager instance = null;

        public static SpotifyUser spotifyUser = new SpotifyUser();
        public static CodyConfig codyConfig = CodyConfig.getInstance;
        private static Device device = Device.getInstance;
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
        public static SpotifyUser _spotifyUser { get; set; }
        public static List<PlaylistItem> _spotifyPlaylists { get; set; }
        public static List<PlaylistItem> _savedPlaylists { get; set; }
        public static List<PlaylistItem> _musictimePlaylists { get; set; }

        public async Task UpdateSpotifyAccessInfoAsync(Auths auths, SpotifyTokens spotifyTokens)
        {

            try
            {

                if (auths != null)
                {
                    codyConfig.spotifyClientId = spotifyTokens.clientId;
                    codyConfig.spotifyClientSecret = spotifyTokens.clientSecret;
                    codyConfig.spotifyAccessToken = auths.AccessToken;
                    codyConfig.spotifyRefreshToken = auths.RefreshToken;
                    codyConfig.enableSpotifyApi = SoftwareCoUtil.isMac() ? true : false;

                    codyConfig.setConfig(codyConfig);

                    SoftwareCoUtil.setItem("spotify_access_token", auths.AccessToken);
                    SoftwareCoUtil.setItem("spotify_refresh_token", auths.RefreshToken);

                    UserProfile userProfile = UserProfile.getInstance;
                    _spotifyUser = await userProfile.GetUserProfileAsync();
                    bool isConnected = MusicManager.hasSpotifyPlaybackAccess();
                    await getDevicesAsync();
                   
                  //  SoftwareUserSession.GetSpotifyUserStatusTokenAsync(isConnected);

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
            bool isTrackPlaying = false;
            trackStatus = await SpotifyCurrentTrackAsync();
            if (trackStatus.is_playing == true)
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
            if (isConnected)
            {
                device = await MusicClient.GetDeviceAsync();
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
                if(device.devices.Count>1)
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
                    if (item.is_active == true)
                    { deviceNames = item.name; }
                }
            }
            return deviceNames;
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
            
          

            HttpResponseMessage response = null;
           
            if (!string.IsNullOrEmpty(MusicManager.DeviceID()))
            {
                String api = "/v1/me/player/play?"+ DeviceID();

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
                trackStatus         = JsonConvert.DeserializeObject<TrackStatus>(responseBody);
            }
            return trackStatus;
        }
    }
}
