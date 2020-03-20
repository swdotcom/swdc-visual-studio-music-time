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
        internal const string PERSONAL_TOP_SONGS_NAME   = "My AI Top 40";
        internal const int PERSONAL_TOP_SONGS_PLID      = 1;
        internal static List<string> spotifyGenres = new List<string>() { "Acoustic", "Afrobeat", "Alt rock", "Alternative", "Ambient", "Anime",
              "Black metal", "Bluegrass", "Blues", "Bossanova", "Brazil", "Breakbeat",
              "British", "Cantopop", "Chicago house", "Children", "Chill", "Classical",
              "Club", "Comedy", "Country", "Dance", "Dancehall", "Death metal", "Deep house",
              "Detroit techno", "Disco", "Disney", "Drum and bass", "Dub", "Dubstep",
              "Edm", "Electro", "Electronic", "Emo", "Folk", "Forro", "French", "Funk",
              "Garage", "German", "Gospel", "Goth", "Grindcore", "Groove", "Grunge",
              "Guitar", "Happy", "Hard rock", "Hardcore", "Hardstyle", "Heavy-metal",
              "Hip-hop", "Hip hop", "Holidays", "Honky tonk", "House", "Idm", "Indian",
              "Indie", "Indie pop", "Industrial", "Iranian", "J dance", "J idol", "J pop",
              "J rock", "Jazz", "K pop", "Kids", "Latin", "Latino", "Malay", "Mandopop",
              "Metal", "Metal misc", "Metalcore", "Minimal techno", "Movies", "Mpb",
              "New age", "New release", "Opera", "Pagode", "Party", "Philippines opm",
              "Piano", "Pop", "Pop film", "Post dubstep", "Power pop", "Progressive house",
              "Psych rock", "Punk", "Punk-rock", "R n b", "Rainy day", "Reggae", "Reggaeton",
              "Road trip", "Rock", "Rock n roll", "Rockabilly", "Romance", "Sad", "Salsa",
              "Samba", "Sertanejo", "Show tunes", "Singer-songwriter", "Ska", "Sleep", "Songwriter",
              "Soul", "Soundtracks", "Spanish", "Study", "Summer", "Swedish", "Synth-popv",
              "Tango", "Techno", "Trance", "Trip hop", "Turkish", "Work out", "World-music"};
        

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
