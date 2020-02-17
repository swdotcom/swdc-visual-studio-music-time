using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MusicTime
{
    class SlackControlManager
    {
        private static int TEN_SECONDS = 1000 * 10;
        public static async Task ConnectToSlackAsync()
        {
           string app_jwt = SoftwareUserSession.GetJwt();
           string encodedJwt = Uri.EscapeUriString(app_jwt);
           string qryStr = "integrate=slack&plugin=musictime&token="+encodedJwt;

            // authorize the user for slack
            string endpoint = Constants.api_endpoint+"/auth/slack?"+qryStr;
            launchWebUrl(endpoint);
            refetchSlackConnectStatusLazily();
        }

        private static void refetchSlackConnectStatusLazily(int tryCountUntilFound = 10)
        {
            Thread.Sleep(TEN_SECONDS);
            SlackConnectStatusHandlerAsync(tryCountUntilFound);
        }

        private static async void SlackConnectStatusHandlerAsync(int tryCountUntilFound)
        {
            Auths auths = null;
            //bool online = MusicTimeCoPackage.isOnline;
            //if (!online)
            //{
            //    return;
            //}
            string app_jwt = SoftwareUserSession.GetJwt();

            if (app_jwt != null)
            {
                try
                {
                    auths = await GetSlackUserStatusAsync(true);

                    if (auths == null)
                    {
                        // try again if the count is not zero
                        if (tryCountUntilFound > 0)
                        {
                            tryCountUntilFound -= 1;
                            refetchSlackConnectStatusLazily(tryCountUntilFound);
                        }

                    }
                    else
                    {
                        SoftwareDisconnectSlackCommand.UpdateEnabledState(auths.LoggedIn);
                        SoftwareConnectSlackCommand.UpdateEnabledState(false);
                    }
                }
                catch (Exception e)
                {


                }
            }
        }

        public static async Task<Auths> GetSlackUserStatusAsync(bool online)
        {
            string app_jwt          = SoftwareUserSession.GetJwt();
            string responseBody     = null;
            Auths auths             = null;

            UserState userState = new UserState();
            HttpResponseMessage response = null;

            if (online && (!string.IsNullOrEmpty(app_jwt) ))
            {
                string api = "/users/plugin/state";
                response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, api, app_jwt);
                if (SoftwareHttpManager.IsOk(response))
                {
                    responseBody = await response.Content.ReadAsStringAsync();
                    userState = JsonConvert.DeserializeObject<UserState>(responseBody);

                    if (userState.State == "OK")
                    {
                        if (userState.User.Auths != null)
                        {
                            for (int i = 0; i < userState.User.Auths.Length; i++)
                            {
                                if (userState.User.Auths[i].Type == "slack")
                                {
                                    auths = new Auths();
                                    auths = userState.User.Auths[i];

                                    auths.LoggedIn = true;

                                    await MusicManager.UpdateSlackAccesInfoAsync(auths);
                                    Logger.Debug("Connected to slack");

                                }
                            }

                        }
                    }

                }
            }



            return auths;
        }

        private static void launchWebUrl(string url)
        {
            Process.Start(url);
        }

        public static async Task DisConnectToSlackAsync()
        {
            HttpResponseMessage response = null;
            string app_jwt = "";
            bool online = MusicTimeCoPackage.isOnline;
            if (!online)
            {
                return;
            }

            app_jwt = SoftwareUserSession.GetJwt();
            string api = "/auth/slack/disconnect";

            response = await SoftwareHttpManager.SendRequestPutAsync(api, null);
            if (SoftwareHttpManager.IsOk(response))
            {

                await MusicManager.UpdateSlackAccesInfoAsync(null);
                SoftwareConnectSlackCommand.UpdateEnabledState(true);
                SoftwareDisconnectSlackCommand.UpdateEnabledState(false);
                Logger.Debug("Slack Disconnected");
            }

        }
    }
}
