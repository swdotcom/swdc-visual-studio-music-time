using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MusicTime
{
    class SlackControlManager
    {
        private static SlackControlManager instance = null;

       
        private SlackControlManager()
        {
        }

        public static SlackControlManager getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SlackControlManager();
                }
                return instance;
            }
        }





        private static int TEN_SECONDS = 1000 * 10;
        public static List<Channel> SlackChannels = null;
     

        //public delegate void SlackConnectionHandler(object source, EventArgs args);

        //public  event SlackConnectionHandler eventSlackConnected;
      

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
            
            string app_jwt = SoftwareUserSession.GetJwt();

            if (app_jwt != null)
            {
                try
                {
                    auths = await GetSlackUserStatusAsync(true);

                    if (auths.LoggedIn !=true)
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
            Auths auths             = new Auths();

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
                                    MusicTimeCoPackage.slackConnected = true;
                                    MusicTimeCoPackage.SlackChannels = await GetSalckChannels();
                                    
                                    Logger.Debug("Connected to slack");
                                    SoftwareDisconnectSlackCommand.UpdateEnabledState(true);
                                    SoftwareConnectSlackCommand.UpdateEnabledState(false);

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
            List<Channel> slackChannels = null;
            slackChannels = await GetSalckChannels();
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
                MusicTimeCoPackage.slackConnected = false;
                Logger.Debug("Slack Disconnected");
            }

        }
        public static async Task<List<Channel>> GetSalckChannels()
        {
            string api = "https://slack.com/api/conversations.list";
            List<Channel> channel_list       = new List<Channel>();
            HttpResponseMessage response    = null;
            SlackChannel slackChannel = new SlackChannel();
            string SlackAccessToken = (string)SoftwareCoUtil.getItem("slack_access_token");

            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3000)
            };

            var cts = new CancellationTokenSource();


            client.DefaultRequestHeaders.Authorization =
                          new AuthenticationHeaderValue("Bearer", SlackAccessToken);

            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");
            
            try
            {
                string endpoint = api;
         
                response = await client.GetAsync(endpoint, cts.Token);

                if (MusicClient.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    slackChannel = JsonConvert.DeserializeObject<SlackChannel>(responseBody);
                    if(slackChannel.Channels.Count>0)
                    {
                        foreach (Channel item in slackChannel.Channels)
                        {
                            channel_list.Add(item);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                
            }
            finally
            {
            }
          
            return channel_list;

        }

        private static void NotifyPostException(Exception e)
        {
            throw new NotImplementedException();
        }

        public static async Task ShareOnSlackChannel(string channelId, string trackId)
        {
            string track_url = "https://open.spotify.com/track/";
            string trackUrl = track_url + trackId;
            string post_msg_url = "https://slack.com/api/chat.postMessage";

            HttpResponseMessage response = null;
           
            string SlackAccessToken = (string)SoftwareCoUtil.getItem("slack_access_token");
            string msg = "Check out this Song\n" + trackUrl;
            JsonObject payload = new JsonObject();
            payload.Add("channel", channelId);
            payload.Add("text", msg);
            string Payload = payload.ToString();


            HttpClient client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3000)
            };

            var cts = new CancellationTokenSource();
            HttpContent contentPost = null;
            try
            {
                if (Payload != null)
                {
                    contentPost = new StringContent(Payload, Encoding.UTF8, "application/json");
                }
            }
            catch (Exception e)
            {
                NotifyPostException(e);
            }

            client.DefaultRequestHeaders.Authorization =
                          new AuthenticationHeaderValue("Bearer", SlackAccessToken);

            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");

            response = await client.PostAsync(post_msg_url, contentPost, cts.Token);
            if (MusicClient.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
            }

            }



        public static async Task CopylinkToClipboard(string Track_Id)
        {
            string track_url = "https://open.spotify.com/track/";
            string trackUrl = track_url + Track_Id;

            Clipboard.SetText(trackUrl);
            MessageBox.Show("Spotify track link copied to clipboard.");
        }

        public static async Task ShareOnFacebook(string Track_Id)
        {
            string track_url = "https://open.spotify.com/track/";
            string trackUrl = track_url + Track_Id;
            string api = "https://www.facebook.com/sharer/sharer.php?u=" + trackUrl + "&hashtag=#MusicTime";
            launchWebUrl(api);
        }

        public static async Task ShareOnTwitter(string Track_Id)
        {
            string track_url = "https://open.spotify.com/track/";
            string trackUrl = track_url + Track_Id;
            string api = "https://twitter.com/intent/tweet?text=Check+out+this+track+I’m+listening+to+using&url=" + trackUrl + "&hashtags=MusicTime&via=Software(www.software.com)";
            launchWebUrl(api);
            
        }
        public static async Task ShareOnTumbler(string Track_Id)
        {
            string track_url = "https://open.spotify.com/track/";
            string trackUrl = track_url + Track_Id;
            string api = "http://tumblr.com/widgets/share/tool?canonicalUrl=" + trackUrl + "&content=" + trackUrl + "&posttype=link&title=Check+out+this+song&caption=Software+Audio+Share&tags=MusicTime";
            launchWebUrl(api);
        }
        public static  async Task ShareOnWhatsApp(string Track_Id)
        {
            string track_url = "https://open.spotify.com/track/";
            string trackUrl = track_url + Track_Id;
            string api = "https://api.whatsapp.com/send?text=Check+out+this+track+I’m+listening+to+using+Music+Time.+Created+by+Software(www.software.com):" + trackUrl;
            
            launchWebUrl(api);
        }
      

    }
}
