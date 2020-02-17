using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{

    public partial class UserState
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("jwt")]
        public string Jwt { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }

    public partial class User
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public bool Password { get; set; }

        [JsonProperty("profile_image")]
        public object ProfileImage { get; set; }

        [JsonProperty("salt")]
        public bool Salt { get; set; }

        [JsonProperty("setup_complete")]
        public object SetupComplete { get; set; }

        [JsonProperty("code_goals")]
        public CodeGoals CodeGoals { get; set; }

        [JsonProperty("client_cache")]
        public object ClientCache { get; set; }

        [JsonProperty("timezone")]
        public object Timezone { get; set; }

        [JsonProperty("offset_minutes")]
        public long OffsetMinutes { get; set; }

        [JsonProperty("occupation")]
        public object Occupation { get; set; }

        [JsonProperty("company")]
        public object Company { get; set; }

        [JsonProperty("location")]
        public object Location { get; set; }

        [JsonProperty("github_id")]
        public object GithubId { get; set; }

        [JsonProperty("github_access_token")]
        public bool GithubAccessToken { get; set; }

        [JsonProperty("github_refresh_token")]
        public object GithubRefreshToken { get; set; }

        [JsonProperty("github_scopes")]
        public object GithubScopes { get; set; }

        [JsonProperty("google_id")]
        public object GoogleId { get; set; }

        [JsonProperty("google_access_token")]
        public bool GoogleAccessToken { get; set; }

        [JsonProperty("google_refresh_token")]
        public object GoogleRefreshToken { get; set; }

        [JsonProperty("permissions")]
        public object[] Permissions { get; set; }

        [JsonProperty("login_timestamp")]
        public long LoginTimestamp { get; set; }

        [JsonProperty("login_count")]
        public long LoginCount { get; set; }

        [JsonProperty("invite_token")]
        public object InviteToken { get; set; }

        [JsonProperty("plugin_token")]
        public string PluginToken { get; set; }

        [JsonProperty("reset_token")]
        public object ResetToken { get; set; }

        [JsonProperty("confirm_token")]
        public object ConfirmToken { get; set; }

        [JsonProperty("plugin_jwt")]
        public string PluginJwt { get; set; }

        [JsonProperty("oauths")]
        public object Oauths { get; set; }

        [JsonProperty("preferences")]
        public Preferences Preferences { get; set; }

        [JsonProperty("mac_addr")]
        public object MacAddr { get; set; }

        [JsonProperty("mac_addr_share")]
        public object MacAddrShare { get; set; }

        [JsonProperty("deactivated")]
        public long Deactivated { get; set; }

        [JsonProperty("phone")]
        public object Phone { get; set; }

        [JsonProperty("creation_annotation")]
        public object CreationAnnotation { get; set; }

        [JsonProperty("hostname")]
        public object Hostname { get; set; }

        [JsonProperty("last_codetime_metrics_timestamp")]
        public object LastCodetimeMetricsTimestamp { get; set; }

        [JsonProperty("last_search_file_timestamp")]
        public long LastSearchFileTimestamp { get; set; }

        [JsonProperty("registered")]
        public long Registered { get; set; }

        [JsonProperty("musictime_token")]
        public string MusictimeToken { get; set; }

        [JsonProperty("codetime_token")]
        public object CodetimeToken { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("integrations")]
        public Integration[] Integrations { get; set; }

        [JsonProperty("apps")]
        public string[] Apps { get; set; }

        [JsonProperty("auths")]
        public Auths[] Auths { get; set; }
    }

    public partial class Auths
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("accountId")]
            public long AccountId { get; set; }

            [JsonProperty("authId")]
            public string AuthId { get; set; }

            [JsonProperty("email")]
            public string Email { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("access_token")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty("scopes")]
            public object[] Scopes { get; set; }

            [JsonProperty("password")]
            public object Password { get; set; }

            [JsonProperty("salt")]
            public object Salt { get; set; }

            [JsonProperty("createdAt")]
            public DateTimeOffset CreatedAt { get; set; }

            [JsonProperty("updatedAt")]
            public DateTimeOffset UpdatedAt { get; set; }

            [JsonProperty("userId")]
            public long UserId { get; set; }

            [JsonProperty("loggedIn")]
            public bool LoggedIn { get; set; }

    }

        public partial class CodeGoals
        {
            [JsonProperty("working_hours")]
            public long[][] WorkingHours { get; set; }

            [JsonProperty("daily_code_hours")]
            public DailyCodeHours DailyCodeHours { get; set; }
        }

        public partial class DailyCodeHours
        {
            [JsonProperty("days")]
            public Days Days { get; set; }

            [JsonProperty("modified")]
            public bool Modified { get; set; }
        }

        public partial class Days
        {

            [JsonProperty("mon")]
            public Mon Mon { get; set; }

            [JsonProperty("tue")]
            public Tue Tue { get; set; }

            [JsonProperty("wed")]
            public Wed Wed { get; set; }

            [JsonProperty("thr")]
            public Thr Thr { get; set; }

            [JsonProperty("fri")]
            public Fri Fri { get; set; }


            [JsonProperty("sat")]
            public Sat Sat { get; set; }

            [JsonProperty("sun")]
            public Sun Sun { get; set; }

        }

        public partial class Mon
        {
            [JsonProperty("goal")]
            public long Goal { get; set; }
        }
        public partial class Tue
        {
            [JsonProperty("goal")]
            public long Goal { get; set; }
        }
        public partial class Wed
        {
            [JsonProperty("goal")]
            public long Goal { get; set; }
        }
        public partial class Thr
        {
            [JsonProperty("goal")]
            public long Goal { get; set; }
        }
        public partial class Fri
        {
            [JsonProperty("goal")]
            public long Goal { get; set; }
        }
        public partial class Sat
        {
            [JsonProperty("goal")]
            public long Goal { get; set; }
        }
        public partial class Sun
        {
            [JsonProperty("goal")]
            public long Goal { get; set; }
        }
        public partial class Preferences
        {
            [JsonProperty("showMusic")]
            [JsonConverter(typeof(ParseStringConverter))]
            public bool ShowMusic { get; set; }

            [JsonProperty("showGit")]
            [JsonConverter(typeof(ParseStringConverter))]
            public bool ShowGit { get; set; }
        }


    public partial class Integration
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("upgraded")]
        public long Upgraded { get; set; }

        [JsonProperty("last_datum_timestamp")]
        public object LastDatumTimestamp { get; set; }

        [JsonProperty("last_heartbeat_timestamp")]
        public object LastHeartbeatTimestamp { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("pluginId")]
        public long PluginId { get; set; }
    }
    internal class ParseStringConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(bool) || t == typeof(bool?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
            if (Boolean.TryParse(value, out bool b))
            {
                return b;
            }
            throw new Exception("Cannot unmarshal type bool");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (bool)untypedValue;
                var boolString = value ? "true" : "false";
                serializer.Serialize(writer, boolString);
                return;
            }

            public static readonly ParseStringConverter Singleton = new ParseStringConverter();
        }
    }

