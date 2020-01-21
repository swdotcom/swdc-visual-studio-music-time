using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{
    class MusicUtil
    {

       public static string createUriFromTrackId(string track_id)
        {
            if (!string.IsNullOrEmpty(track_id) && !track_id.Contains("spotify:track:"))
            {
                track_id = "spotify:track:"+track_id;
            }

            return track_id;
        }
        public static List <string> createUrisFromTrackId(List<string> track_ids)
        {
            List<string> uris = new List<string>();

            foreach (string item in track_ids)
            {
                string uri = createUriFromTrackId(item);

                uris.Add(uri);
            }

            return uris;
        }

        internal static string CreateSpotifyIdFromUri(string uri)
        {
            if (!string.IsNullOrEmpty(uri))
                return uri;

            return "";
        }

        internal static string createSpotifyUserUriFromId(string user_id)
        {
            if (!string.IsNullOrEmpty(user_id) && !user_id.Contains("spotify:user:"))
            {
                user_id = "spotify:user:" + user_id;
            }

            return user_id;
        }

        internal static string createPlaylistUriFromPlaylistId(string playlist_id)
        {
            if (!string.IsNullOrEmpty(playlist_id) && !playlist_id.Contains("spotify:playlist:"))
            {
                playlist_id = "spotify:playlist:$"+playlist_id;
            }
            return playlist_id;
        }
    }
}
