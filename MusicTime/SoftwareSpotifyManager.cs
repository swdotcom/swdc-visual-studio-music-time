using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Threading;
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
            
            if (!MusicTimeCoPackage.isOnline)
            {
                await SoftwareUserSession.isOnlineCheckAsync();
            }

            if (MusicTimeCoPackage.isOnline)
            {
                string app_jwt = "";
                bool softwareSessionFileExists  = SoftwareCoUtil.softwareSessionFileExists();
                bool jwtExists                  = SoftwareCoUtil.jwtExists();

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



                string qryStr = "/auth/spotify?token=" + app_jwt + "&mac=" + SoftwareCoUtil.isMac().ToString().ToLower();

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
                    auths = await GetMusicTimeUserStatusAsync(online);

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
                        MusicTimeCoPackage.UpdateEnableCommands(auths.LoggedIn);
                     
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

        public static async Task<Auths> GetMusicTimeUserStatusAsync(bool isOnlinet) 
        {
          
            string app_jwt                  = SoftwareUserSession.GetJwt();
            string spotify_refresh_token    = (string)SoftwareCoUtil.getItem("spotify_refresh_token");
            string responseBody             = null;
            Auths auths                     = null;
            UserState userState       = new UserState();

            HttpResponseMessage response    = null;

            if (isOnlinet && (!string.IsNullOrEmpty(app_jwt) || string.IsNullOrEmpty(spotify_refresh_token)))
            {
                string api = "/users/plugin/state";
                response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, api, spotify_refresh_token, app_jwt);
                if (SoftwareHttpManager.IsOk(response))
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                    userState = JsonConvert.DeserializeObject<UserState>(responseBody);

                    if(userState.State =="OK")
                    {
                        string email = (string)SoftwareCoUtil.getItem("name");
                        if(email!= userState.Email)
                        {
                            SoftwareCoUtil.setItem("name", userState.Email);
                        }
                        if (userState.Jwt != app_jwt && userState.Jwt !=null)
                        {
                            // update it
                            SoftwareCoUtil.setItem("jwt", userState.Jwt);
                        }

                        if (userState.User.Auths != null)
                        {
                            

                            for (int i = 0; i < userState.User.Auths.Length; i++)
                            {
                                if (userState.User.Auths[i].Type == "spotify")
                                {
                                    auths = new Auths();
                                    auths = userState.User.Auths[i];

                                    auths.LoggedIn = true;

                                    await MusicManager.getInstance.UpdateSpotifyAccessInfoAsync(auths, spotifyTokens);
                                    if (auths.LoggedIn)
                                    {
                                        Auths slackaAuth    = new Auths();
                                        slackaAuth          =  await SlackControlManager.GetSlackUserStatusAsync(true);
                                        if (slackaAuth.LoggedIn == true)
                                        {
                                            SoftwareDisconnectSlackCommand.UpdateEnabledState(true);
                                            SoftwareConnectSlackCommand.UpdateEnabledState(false);
                                           
                                        }
                                        else
                                        {
                                            await MusicManager.UpdateSlackAccesInfoAsync(null);
                                            SoftwareConnectSlackCommand.UpdateEnabledState(true);
                                            SoftwareDisconnectSlackCommand.UpdateEnabledState(false);
                                        }

                                    }
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
            HttpResponseMessage response    = null;
            string app_jwt                  = "";

            if (!MusicTimeCoPackage.isOnline)
            {
                await SoftwareUserSession.isOnlineCheckAsync();
            }
            if (MusicTimeCoPackage.isOnline)
            {

                app_jwt = SoftwareUserSession.GetJwt();
                string api = "/auth/spotify/disconnect";

                response = await SoftwareHttpManager.SendRequestPutAsync(api, null);
                if (SoftwareHttpManager.IsOk(response))
                {
                    MusicManager.clearSpotifyAccessInfo(spotifyTokens);
                    MusicTimeCoPackage.UpdateUserStatusAsync(null);
                    SoftwareConnectSlackCommand.UpdateEnabledState(false);
                    SoftwareDisconnectSlackCommand.UpdateEnabledState(false);

                }
            }
        }



    }
    

    public class SpotifyTokens
    {
        public string clientId { get; set; }
        public string clientSecret { get; set; }
    }
}
