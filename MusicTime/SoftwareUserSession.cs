using SoftwareCo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicTime
{
    public class SoftwareUserSession
    {
        private static bool loggedInCacheState = false;
        private static string lastJwt = null;
        public static bool checkingLoginState = false;
        public static bool isOnline = true;
        public static long lastOnlineCheck = 0;
      
        public class UserStatus
        {
            public bool loggedIn;
        }

        public class User
        {
            public long id;
            public string email;
            public string plugin_jwt;
        }


        public static async Task isOnlineCheckAsync()
        {
            HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, "/ping", null, 3, null, true /*isOnlineCheck*/);
            MusicTimeCoPackage.isOnline = SoftwareHttpManager.IsOk(response);
            
        }

        //public static async Task<bool> IsOnlineAsync()
        //{
          
        //    long nowInSec = SoftwareCoUtil.getNowInSeconds();
          
        //    long thresholdSeconds = nowInSec - lastOnlineCheck;
           
        //    if (thresholdSeconds > 30)
        //    {
        //        // 3 second timeout
        //        HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, "/ping", null, 3, null, true /*isOnlineCheck*/);
        //        isOnline = SoftwareHttpManager.IsOk(response);
        //        lastOnlineCheck = nowInSec;
                
        //    }

        //    return isOnline;
        //}
        public static string GetJwt()
        {
            
                object jwt = SoftwareCoUtil.getItem("jwt");
                lastJwt = (jwt != null && !((string)jwt).Equals("")) ? (string)jwt : null;
            
                return lastJwt;
        }

        //public static string GetJwt()
        //{

        //        object jwt = SoftwareCoUtil.getItem("jwt");
        //        lastJwt = (jwt != null && !((string)jwt).Equals("")) ? (string)jwt : null;

        //    return lastJwt;
        //}

        public static async Task<string> CreateAnonymousUserAsync(bool online)
        {
            // get the app jwt
            string app_jwt = await GetAppJwtAsync(online);
            if (app_jwt != null && online)
            {
                string creation_annotation = "NO_SESSION_FILE";
                string osUsername = Environment.UserName;
                string timezone = "";
                if (TimeZone.CurrentTimeZone.DaylightName != null
                    && TimeZone.CurrentTimeZone.DaylightName != TimeZone.CurrentTimeZone.StandardName)
                {
                    timezone = TimeZone.CurrentTimeZone.DaylightName;
                }
                else
                {
                    timezone = TimeZone.CurrentTimeZone.StandardName;
                }

                JsonObject jsonObj = new JsonObject();
                jsonObj.Add("timezone", timezone);
                jsonObj.Add("username", osUsername);
                jsonObj.Add("hostname", SoftwareCoUtil.getHostname());
                jsonObj.Add("creation_annotation", creation_annotation);

                string api = "/data/onboard";
                string jsonData = jsonObj.ToString();
                HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Post, api, jsonData, app_jwt);

                if (SoftwareHttpManager.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    IDictionary<string, object> respObj = (IDictionary<string, object>)SimpleJson.DeserializeObject(responseBody);
                    respObj.TryGetValue("jwt", out object jwtObj);
                    string jwt = (jwtObj == null) ? null : Convert.ToString(jwtObj);
                    if (jwt != null)
                    {
                        SoftwareCoUtil.setItem("jwt", jwt);
                        return jwt;
                    }
                }
            }

            return null;
        }

        public static async Task<string> GetAppJwtAsync(bool online)
        {
            try
            {
                if (online)
                {
                    long seconds = SoftwareCoUtil.getNowInSeconds();
                    HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(
                            HttpMethod.Get, "/data/apptoken?token=" + seconds, null);

                    if (SoftwareHttpManager.IsOk(response))
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        IDictionary<string, object> jsonObj = (IDictionary<string, object>)SimpleJson.DeserializeObject(responseBody);
                        jsonObj.TryGetValue("jwt", out object jwtObj);
                        string app_jwt = (jwtObj == null) ? null : Convert.ToString(jwtObj);
                        return app_jwt;
                    }
                }
              
            }
            catch ( Exception e)
            {

                
            }
            return null;
        }

        private static async Task<User> GetUserAsync(bool online)
        {
            string jwt = GetJwt();
            if (jwt != null && online)
            {
                string api = "/users/me";
                HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, api, jwt);
                if (SoftwareHttpManager.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    IDictionary<string, object> jsonObj = (IDictionary<string, object>)SimpleJson.DeserializeObject(responseBody);
                    if (jsonObj != null)
                    {
                        jsonObj.TryGetValue("data", out object userObj);
                        if (userObj != null)
                        {
                            IDictionary<string, object> userData = (IDictionary<string, object>)userObj;

                            userData.TryGetValue("email", out object emailObj);
                            string email = (emailObj == null) ? null : Convert.ToString(emailObj);
                            userData.TryGetValue("plugin_jwt", out object pluginJwtObj);
                            string pluginJwt = (pluginJwtObj == null) ? null : Convert.ToString(pluginJwtObj);
                            userData.TryGetValue("id", out object idObj);
                            long userId = (idObj == null) ? 0L : Convert.ToInt64(idObj);

                            User user = new User();
                            user.email = email;
                            user.plugin_jwt = pluginJwt;
                            user.id = userId;
                            return user;
                        }
                    }
                }
            }
            return null;
        }

        private static async Task<bool> IsLoggedOn(bool online)
        {
            string jwt = GetJwt();
            if (online && jwt != null)
            {
                User user = await GetUserAsync(online);
                if (user != null && SoftwareCoUtil.IsValidEmail(user.email))
                {
                    SoftwareCoUtil.setItem("name", user.email);
                    SoftwareCoUtil.setItem("jwt", user.plugin_jwt);
                    lastJwt = user.plugin_jwt;
                    return true;
                }

                string api = "/users/plugin/state";
                HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Get, api, jwt);
                if (SoftwareHttpManager.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    IDictionary<string, object> jsonObj = (IDictionary<string, object>)SimpleJson.DeserializeObject(responseBody);
                    if (jsonObj != null)
                    {
                        jsonObj.TryGetValue("state", out object stateObj);
                        string state = (stateObj == null) ? "NONE" : Convert.ToString(stateObj);
                        jsonObj.TryGetValue("jwt", out object pluginJwtObj);
                        string pluginJwt = (pluginJwtObj == null) ? null : Convert.ToString(pluginJwtObj);
                        if (state.Equals("OK") && pluginJwt != null)
                        {
                            jsonObj.TryGetValue("email", out object nameObj);
                            string name = (nameObj == null) ? null : Convert.ToString(nameObj);
                            if (name != null)
                            {
                                SoftwareCoUtil.setItem("name", name);
                            }
                            SoftwareCoUtil.setItem("jwt", pluginJwt);
                            lastJwt = pluginJwt;
                        }
                        else if (state.Equals("NOT_FOUND"))
                        {
                            SoftwareCoUtil.setItem("jwt", null);
                            lastJwt = null;
                        }
                    }
                }

            }
            SoftwareCoUtil.setItem("name", null);
            return false;
        }

        public static async Task<UserStatus> GetSpotifyUserStatusTokenAsync(bool online)
        {
            //bool softwareSessionFileExists  = SoftwareCoUtil.softwareSessionFileExists();
            bool jwtExists                  = SoftwareCoUtil.jwtExists();
            Auths auths                     = new Auths();
            UserStatus userStatus           = new UserStatus();
            
            if (!jwtExists || !online)
            {
                userStatus.loggedIn         = false;
            }
            else
            {
                try
                {
                    SpotifyTokens  spotifyTokens = await SoftwareSpotifyManager.GetSpotifyTokenAsync();
                    auths                        = await SoftwareSpotifyManager.GetMusicTimeUserStatusAsync(online);

                    if (auths.LoggedIn == true)
                    {
                        userStatus.loggedIn      = true;

                        MusicTimeCoPackage.UpdateEnableCommands(userStatus.loggedIn);
                    }
                    else
                    {
                        MusicManager.clearSpotifyAccessInfo(spotifyTokens);
                    }
                    
                }
                catch (Exception ex)
                {


                }
                   
            }
            return userStatus;
        }

        public static async Task<bool> GetSlackUserStatusTokenAsync(bool online)
        {
          
            bool jwtExists          = SoftwareCoUtil.jwtExists();
            Auths auths             = new Auths();
            bool loginFlag = false;

            if (!jwtExists || !online)
            {
                return loginFlag;
            }
            else
            {

                try
                {
                    auths = await SlackControlManager.GetSlackUserStatusAsync(online);

                    if (auths.LoggedIn == true)
                    {
                        SoftwareDisconnectSlackCommand.UpdateEnabledState(true);
                        SoftwareConnectSlackCommand.UpdateEnabledState(false);
                        loginFlag = true;
                    }
                    else
                    {
                        await MusicManager.UpdateSlackAccesInfoAsync(null);
                        SoftwareConnectSlackCommand.UpdateEnabledState(true);
                        SoftwareDisconnectSlackCommand.UpdateEnabledState(false);
                    }

                   
                }

                catch (Exception e)
                {


                }
            
            }
            return loginFlag;
        }

        public static bool GetSpotifyUserStatus()
        { 
            
            return MusicManager.hasSpotifyPlaybackAccess() ? true : false;

        }

        public static async Task<UserStatus> GetUserStatusAsync(bool isInitialCall)
        {
            bool online = MusicTimeCoPackage.isOnline;
            bool softwareSessionFileExists = SoftwareCoUtil.softwareSessionFileExists();
            bool jwtExists = SoftwareCoUtil.jwtExists();

            if (!isInitialCall && online && !jwtExists)
            {
                await SoftwareUserSession.CreateAnonymousUserAsync(online);
            }

            bool loggedIn = await IsLoggedOn(online);

            UserStatus currentUserStatus    = new UserStatus();
            currentUserStatus.loggedIn      = loggedIn;

            if (online && loggedInCacheState != loggedIn)
            {
                // change in logged in state, send heatbeat and fetch kpm
                SendHeartbeat("STATE_CHANGE:LOGGED_IN:" + loggedIn);

                
            }

            loggedInCacheState = loggedIn;
            
            return currentUserStatus;
        }

        public static async void RefetchUserStatusLazily(int tryCountUntilFoundUser)
        {
            checkingLoginState = true;
            UserStatus userStatus = await GetUserStatusAsync(true);

            if (!userStatus.loggedIn && tryCountUntilFoundUser > 0)
            {
                tryCountUntilFoundUser -= 1;

                try
                {
                    Thread.Sleep(1000 * 10);
                    RefetchUserStatusLazily(tryCountUntilFoundUser);
                }
                catch (ThreadInterruptedException e)
                {
                    //
                }
            }
            else
            {
               
                checkingLoginState = false;
            }
        }

        public static async void SendHeartbeat(string reason)
        {
            string jwt = GetJwt();
            bool online = MusicTimeCoPackage.isOnline;
            if (online && jwt != null)
            {

                string version = Constants.EditorVersion;

                JsonObject jsonObj = new JsonObject();
                jsonObj.Add("version", MusicTimeCoPackage.GetVersion());
                jsonObj.Add("os", MusicTimeCoPackage.GetOs());
                jsonObj.Add("pluginId", Constants.PluginId);
                jsonObj.Add("start", SoftwareCoUtil.getNowInSeconds());
                jsonObj.Add("trigger_annotation", reason);
                jsonObj.Add("hostname", SoftwareCoUtil.getHostname());

                string api = "/data/heartbeat";
                string jsonData = jsonObj.ToString();
                HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Post, api, jsonData, jwt);

                if (!SoftwareHttpManager.IsOk(response))
                {
                  
                }
            }
        }
    }
}

