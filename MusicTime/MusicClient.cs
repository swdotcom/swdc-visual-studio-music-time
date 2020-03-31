using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MusicTime
{

    class MusicClient
    {

        private static MusicClient instance = null;
        public static CodyConfig codyConfig = CodyConfig.getInstance;
        public static Device deviceList     = Device.getInstance;
        private MusicClient()
        {
        }

        public static MusicClient getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MusicClient();
                }
                return instance;
            }
        }
        public static bool IsOk(HttpResponseMessage response)
        {
            return (response != null &&( response.StatusCode == HttpStatusCode.OK||response.StatusCode ==HttpStatusCode.Created  ) );
        }

        public static string QueryString(string api ,List<object> qsOptions)
        {
            foreach (var item in qsOptions)
            {
                api += "?" + item;
            }
            return api;

        }

        public static async Task<HttpResponseMessage> SpotifyApiGetAsync(string api)
        {
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", codyConfig.spotifyAccessToken);

            try
            {
                string endpoint = Constants.api_Spotifyendpoint + "" + api;
                response = await client.GetAsync(endpoint);
                if (MusicClient.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                }
            }
            catch (Exception ex)
            {


            }
            return response;
        }

        public static async Task<HttpResponseMessage> SpotifyApiPutAsync(string api, string payload)
        {
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            HttpContent contentPost = null;
           // string Payload = "";
            try
            {
                if(payload==null)
                { payload = ""; }
                client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", codyConfig.spotifyAccessToken);


                contentPost = new StringContent(payload, Encoding.UTF8, "application/json");
                string endpoint = Constants.api_Spotifyendpoint + "" + api;
                response = await client.PutAsync(endpoint, contentPost);


            }
            catch (Exception ex)
            {

            }

            return response;

        }

        public static async Task<HttpResponseMessage> SpotifyApiPostAsync(string api, string payload)
        {
            HttpResponseMessage response    = null;
            HttpClient client               = new HttpClient();
            HttpContent contentPost         = null;
            
            try
            {
                if(payload==null)
                {
                    payload = "";
                }
                client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", codyConfig.spotifyAccessToken);


                contentPost = new StringContent(payload, Encoding.UTF8, "application/json");
                string endpoint = Constants.api_Spotifyendpoint + "" + api;
                response = await client.PostAsync(endpoint, contentPost);


            }
            catch (Exception ex)
            {

            }

            return response;

        }

        public static async Task refreshSpotifyTokenAsync()
        {
            try
            {
                HttpResponseMessage response = null;
                SpotifyAccessTokens spotifyTokens = new SpotifyAccessTokens();
                HttpClient client = new HttpClient();
                string spotifyAuth = string.Format("{0}:{1}", codyConfig.spotifyClientId, codyConfig.spotifyClientSecret);
                string encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(spotifyAuth));

                var parameters = new Dictionary<string, string> { { "grant_type", "refresh_token" }, { "refresh_token", codyConfig.spotifyRefreshToken } };
                var encodedContent = new FormUrlEncodedContent(parameters);

                client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Basic", encodedAuth);

                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");

                string endpoint = Constants.spotifyUrl + "? grant_type = refresh_token & refresh_token =" + codyConfig.spotifyRefreshToken;
                response = await client.PostAsync(endpoint, encodedContent);

                if (MusicClient.IsOk(response))
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    //update codyConfig
                    spotifyTokens = JsonConvert.DeserializeObject<SpotifyAccessTokens>(responseBody);
                    codyConfig.spotifyAccessToken = spotifyTokens.access_token;
                    SoftwareCoUtil.setItem("spotify_access_token", spotifyTokens.access_token);
                   
                }

            }
            catch (Exception ex)
            {


            }



        }

        public static async Task<Device> GetDeviceAsync()
        {

            HttpResponseMessage response = null;
            const string api    = "/v1/me/player/devices";
            response            = await MusicClient.SpotifyApiGetAsync(api);

            if (response == null || !response.IsSuccessStatusCode)
            {
                // refresh the tokens
                await MusicClient.refreshSpotifyTokenAsync();
                // Try again
                response = await MusicClient.SpotifyApiGetAsync(api);
            }

            if (MusicClient.IsOk(response))
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                deviceList          = JsonConvert.DeserializeObject<Device>(responseBody);
            }
            return deviceList;
        }

        public static async Task<HttpResponseMessage> spotifyApiDeleteAsync(string api, string payload)
        {
            HttpResponseMessage response    = null;
            HttpClient client               = new HttpClient();
            try
            {
                
                client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", codyConfig.spotifyAccessToken);
                
                string endpoint = Constants.api_Spotifyendpoint + "" + api;
            
               response  =
                client.SendAsync(
                new HttpRequestMessage(HttpMethod.Delete, endpoint)
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                })
                .Result;

              
            }
            catch (Exception ex)
            {

            }

            return response;
        }
    }
}
