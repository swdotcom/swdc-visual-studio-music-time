using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{
    internal static class Constants
    {
        internal const string PluginName                = "swdc-visualstudio";
        //
        // sublime = 1, vs code = 2, eclipse = 3, intellij = 4, visualstudio = 6, atom = 7
        //
        internal static int PluginId                    = 6;
        internal const string EditorName                = "visualstudio";

        internal const string api_endpoint              = "https://api.software.com";
        internal const string url_endpoint              = "https://app.software.com";
        internal const string spotifyUrl                = "https://accounts.spotify.com/api/token";
        internal const string api_Spotifyendpoint       = "https://api.spotify.com";
        internal const string CodeTimeExtension         = "Code Time";
        internal const string MusicTimeExtension        = "Music Time";
        internal const string VSGithubLink              = "https://github.com/swdotcom/swdc-visualstudio";
        internal const string SendFeedback              = "mailto:cody@software.com";
        internal const string SOFTWARE_TOP_40_ID        = "6jCkTED0V5NEuM8sKbGG1Z";
        internal const string PERSONAL_TOP_SONGS_NAME   = "MY AI Top 40";
        internal const int PERSONAL_TOP_SONGS_PLID      = 1;
        internal static string EditorVersion
        {
            get
            {
                if (MusicTimeCoPackage.ObjDte == null)
                {
                    return string.Empty;
                }
                return MusicTimeCoPackage.ObjDte.Version;
            }
        }
    }
}
