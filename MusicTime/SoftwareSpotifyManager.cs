using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace MusicTime
{
    class SoftwareSpotifyManager
    {
        private static int TEN_SECONDS = 1000 * 10;
        private static int THIRTY_SECONDS = 1000 * 30;
        private static int ONE_MINUTE = THIRTY_SECONDS * 2;
        public static SpotifyTokens spotifyTokens;

        public static async Task ConnectToSpotifyAsync()
        {
            bool online = MusicTimeCoPackage.isOnline;
            if (!online)
            {
                return;
            }

            string app_jwt = "";
            bool softwareSessionFileExists = SoftwareCoUtil.softwareSessionFileExists();
            bool jwtExists = SoftwareCoUtil.jwtExists();

            if (!softwareSessionFileExists || !jwtExists)
            {
                app_jwt = await SoftwareUserSession.GetAppJwtAsync(true);
                if (app_jwt != null)
                {
                    SoftwareCoUtil.setItem("jwt", app_jwt);
                }
            }
            else
            {

                app_jwt = SoftwareUserSession.GetJwt();
            }
            


            String qryStr = "/auth/spotify?token=" + app_jwt + "&mac="+SoftwareCoUtil.isMac().ToString().ToLower();

            launchWebUrl(Constants.api_endpoint + qryStr);
            try
            {
                await GetSpotifyTokenAsync();

            }
            catch (Exception ex)
            {


            }
            refetchSpotifyConnectStatusLazily();
            

        }
        
        private static void refetchSpotifyConnectStatusLazily(int tryCountUntilFound = 10)
        {
            Thread.Sleep(TEN_SECONDS);
            SpotifyConnectStatusHandlerAsync(tryCountUntilFound);
        }

        private static async void SpotifyConnectStatusHandlerAsync(int tryCountUntilFound)
        {
            Auths auths = new Auths();
            bool online = MusicTimeCoPackage.isOnline;
            if (!online)
            {
                return;
            }
            string app_jwt = SoftwareUserSession.GetJwt();
            if (app_jwt != null)
            {
                try
                {
                    auths = await getMusicTimeUserStatus(online);

                    if (auths == null)
                    {
                        // try again if the count is not zero
                        if (tryCountUntilFound > 0)
                        {
                            tryCountUntilFound -= 1;
                            refetchSpotifyConnectStatusLazily(tryCountUntilFound);
                        }
                        
                    }
                    else if(auths.LoggedIn==true)  
                    {
                        Logger.Debug("AuthsLogeedIn");
                        MusicTimeCoPackage.UpdateMusicStatusBar(true);
                        
                    }
                    
                }
                catch (Exception e)
                {


                }
                
               
            }

        }

        public static async Task<SpotifyTokens> GetSpotifyTokenAsync()
        {
            string app_jwt = SoftwareUserSession.GetJwt();
            spotifyTokens  = new SpotifyTokens();
            string responseBody = "";
            String endpoint = "/auth/spotify/clientInfo";
            HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, endpoint, app_jwt, app_jwt);
            if (SoftwareHttpManager.IsOk(response))
            {
                responseBody = await response.Content.ReadAsStringAsync();
                if (responseBody != null)
                {
                    spotifyTokens = JsonConvert.DeserializeObject<SpotifyTokens>(responseBody);
                }
            }
            return spotifyTokens;
        }

        public static async Task<Auths> getMusicTimeUserStatus(bool isOnlinet) 
        {
          
            string app_jwt                  = SoftwareUserSession.GetJwt();
            string spotify_refresh_token    = (string)SoftwareCoUtil.getItem("spotify_refresh_token");
            string responseBody             = null;
            Auths auths                     = null;
            SpotifyParam spotifyParam       = new SpotifyParam();

            HttpResponseMessage response    = null;

            if (isOnlinet && (!string.IsNullOrEmpty(app_jwt) || string.IsNullOrEmpty(spotify_refresh_token)))
            {
                string api = "/users/plugin/state";
                response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, api, spotify_refresh_token, app_jwt);
                if (SoftwareHttpManager.IsOk(response))
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                    spotifyParam    = JsonConvert.DeserializeObject<SpotifyParam>(responseBody);

                    if(spotifyParam.State =="OK")
                    {
                        string email = (string)SoftwareCoUtil.getItem("name");
                        if(email!=spotifyParam.Email)
                        {
                            SoftwareCoUtil.setItem("name", spotifyParam.Email);
                        }
                        if (spotifyParam.Jwt != app_jwt)
                        {
                            // update it
                            SoftwareCoUtil.setItem("jwt", spotifyParam.Jwt);
                        }

                        if (spotifyParam.User.Auths != null)
                        {
                            auths = new Auths();

                            for (int i = 0; i < spotifyParam.User.Auths.Length; i++)
                            {
                                if (spotifyParam.User.Auths[i].Type == "spotify")
                                {

                                    auths = spotifyParam.User.Auths[i];

                                    auths.LoggedIn = true;

                                    await MusicManager.GetInstance.UpdateSpotifyAccessInfoAsync(auths, spotifyTokens);

                                }
                            }

                        }
                    }

                }
            }
            
            

            return auths;
            
        }

        public static void launchWebUrl(string url)
        {
            Process.Start(url);
        }
        public static async Task DisConnectToSpotifyAsync()
        {
            HttpResponseMessage response = null;
            string app_jwt = "";
            bool online = MusicTimeCoPackage.isOnline;
            if (!online)
            {
                return;
            }
            
                app_jwt     = SoftwareUserSession.GetJwt();
                string api  = "/auth/spotify/disconnect";
               
                response = await SoftwareHttpManager.SendRequestPutAsync(api,null);
                if (SoftwareHttpManager.IsOk(response))
                {
                    MusicManager.cleaclearSpotifyAccessInfo(spotifyTokens);
                    MusicTimeCoPackage.UpdateUserStatusAsync(null);
                }
            
        }

    }
    

    public class SpotifyTokens
    {
        public string clientId { get; set; }
        public string clientSecret { get; set; }
    }
}
