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
            bool online = await SoftwareUserSession.IsOnlineAsync();
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
            try
            {
                await GetSpotifyTokenAsync(app_jwt);

            }
            catch (Exception ex)
            {


            }


            String qryStr = "/auth/spotify?token=" + app_jwt + "&mac="+SoftwareCoUtil.isMac();

            launchWebUrl(Constants.api_endpoint + qryStr);

            refetchSpotifyConnectStatusLazily();
            

        }
        
        private static void refetchSpotifyConnectStatusLazily(int tryCountUntilFound = 5)
        {
            Thread.Sleep(TEN_SECONDS);
            SpotifyConnectStatusHandlerAsync(tryCountUntilFound);
        }

        private static async void SpotifyConnectStatusHandlerAsync(int tryCountUntilFound)
        {
            Auths auths = new Auths();
            bool online = await SoftwareUserSession.IsOnlineAsync();
            if (!online)
            {
                return;
            }
            string app_jwt = SoftwareUserSession.GetJwt();
            if (app_jwt != null)
            {
                try
                {
                    auths = await getSpotifyUserAsync(online, app_jwt);

                }
                catch (Exception e)
                {


                }
                
                if (auths == null)
                {
                    // try again if the count is not zero
                    if (tryCountUntilFound > 0)
                    {
                        tryCountUntilFound -= 1;
                        refetchSpotifyConnectStatusLazily(tryCountUntilFound);
                    }
                    else
                    {
                        MusicTimeCoPackage.GetSpotifyUserStatus(null);
                    }
                }
                else
                {
                    try
                    {
                        MusicManager musicManager = MusicManager.GetInstance;

                        await musicManager.UpdateSpotifyAccessInfoAsync(auths, spotifyTokens);
                    }
                    catch (Exception ex)
                    {


                    }


                }
            }

        }

        private static async Task GetSpotifyTokenAsync(string app_jwt)
        {
            spotifyTokens = new SpotifyTokens();
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
        }

        private static async Task<Auths> getSpotifyUserAsync(bool isOnline, string app_jwt)
        {
            Auths auths = null;
            SpotifyParam spotifyParam = new SpotifyParam();
            string responseBody = null;
            String endpoint = "/auth/spotify/user";
            HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, endpoint, app_jwt, app_jwt);
            if (SoftwareHttpManager.IsOk(response))
            {
                responseBody = await response.Content.ReadAsStringAsync();
                if (!String.IsNullOrEmpty(responseBody))
                {
                    spotifyParam = JsonConvert.DeserializeObject<SpotifyParam>(responseBody);
                }
            }
            if (spotifyParam.Auths != null)
            {
                auths = new Auths();
                for (int i = 0; i < spotifyParam.Auths.Length; i++)
                {
                    if (spotifyParam.Auths[i].Type == "spotify")
                    {
                        auths = spotifyParam.Auths[i];

                        SoftwareCoUtil.setItem("name", spotifyParam.Email);

                        SoftwareCoUtil.setItem("jwt", spotifyParam.PluginJwt);

                        SoftwareCoUtil.setItem("checkstatus", null);

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
            bool online = await SoftwareUserSession.IsOnlineAsync();
            if (!online)
            {
                return;
            }

            bool isConnected = MusicManager.hasSpotifyPlaybackAccess();
            if (!isConnected)
            {

            }
            else
            {
                app_jwt     = SoftwareUserSession.GetJwt();
                string api  = "/auth/spotify/disconnect";
               
                response = await SoftwareHttpManager.SendRequestPutAsync(api,null);
                if (SoftwareHttpManager.IsOk(response))
                {
                    MusicManager.cleaclearSpotifyAccessInfo(spotifyTokens);
                    bool isConnect = MusicManager.hasSpotifyPlaybackAccess();

                    MusicTimeCoPackage.GetSpotifyUserStatus(isConnect);
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
