using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{

    public partial class SpotifyUser
    {
        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("explicit_content")]
        public ExplicitContent ExplicitContent { get; set; }

        [JsonProperty("external_urls")]
        public ExternalUrls ExternalUrls { get; set; }

        [JsonProperty("followers")]
        public Followers Followers { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("images")]
        public List<object> Images { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public partial class ExplicitContent
    {
        [JsonProperty("filter_enabled")]
        public bool FilterEnabled { get; set; }

        [JsonProperty("filter_locked")]
        public bool FilterLocked { get; set; }
    }

    public partial class ExternalUrls
    {
        [JsonProperty("spotify")]
        public Uri Spotify { get; set; }
    }

    public partial class Followers
    {
        [JsonProperty("href")]
        public object Href { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }
    }

   

    public class UserProfile
    {
        private static UserProfile instance = null;
        public static CodyConfig codyConfig = CodyConfig.getInstance;
        private UserProfile()
        {
        }

        public static UserProfile getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new UserProfile();
                }
                return instance;
            }
        }

        public async Task<SpotifyUser> GetUserProfileAsync()
        {
            HttpResponseMessage response    = null;
            SpotifyUser spotifyUser         = new SpotifyUser();

            string api                      = "/v1/me";
            try
            {
                response                    = await MusicClient.SpotifyApiGetAsync(api);

                if (response == null || !response.IsSuccessStatusCode)
                {
                    // refresh the tokens
                    await MusicClient.refreshSpotifyTokenAsync();
                    // Try again
                    response                = await MusicClient.SpotifyApiGetAsync(api);
                }

                if (MusicClient.IsOk(response))
                {
                    string responseBody     = await response.Content.ReadAsStringAsync();
                    spotifyUser             = JsonConvert.DeserializeObject<SpotifyUser>(responseBody);
                    codyConfig.spoftifyUserId = spotifyUser.Id;
                }


            }
            catch (Exception ex)
            {


            }

            return spotifyUser;
        }
        
    }
    public class SpotifyAccessTokens
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }

    }
}
