using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicTime
{
    class MusicController
    {
       private static MusicManager musicManager = MusicManager.getInstance;
       private static Device device             = Device.getInstance;
       private static MusicStateManager musicStateManager = MusicStateManager.getInstance; 
        public static async void PlayPauseTrackAsync()
        {
            try
            {
                if (SoftwareUserSession.GetSpotifyUserStatus())
                {
                    if(await MusicManager.isTrackPlayingAsync())
                    {
                        await MusicManager.SpotifyWebPauseAsync();
                        MusicTimeCoPackage.UpdateCurrentTrackOnStatusAsync(null);
                        MusicStateManager.getInstance.GatherMusicInfo();
                    }
                    else
                    {
                      
                        await MusicManager.SpotifyWebPlayAsync();
                        MusicTimeCoPackage.UpdateCurrentTrackOnStatusAsync(null);
                        MusicStateManager.getInstance.GatherMusicInfo();
                    }

                    if (!MusicManager.isDeviceOpened())
                    {
                        await LaunchPlayerAsync(new options());

                        await MusicManager.getDevicesAsync();


                    }


                }
            }
            catch (Exception ex)
            {

                
            }
            
           
          
        }


        public static async void PreviousTrackAsync()
        {
            try
            {
                if (SoftwareUserSession.GetSpotifyUserStatus())
                {
                    if (!MusicManager.isDeviceOpened())
                    {
                        await LaunchPlayerAsync(new options());

                        await MusicManager.getDevicesAsync();

                        
                    }


                    await MusicManager.SpotifyWebPlayPreviousAsync();
                    MusicStateManager.getInstance.GatherMusicInfo();

                }

            }
            catch (Exception ex)
            {

                
            }
            
        }

        public static async void NextTrackAsync()
        {
            try
            {
                if (SoftwareUserSession.GetSpotifyUserStatus())
                {
                    if (!MusicManager.isDeviceOpened())
                    {
                        await LaunchPlayerAsync(new options());

                        await MusicManager.getDevicesAsync();
                        
                    }

                     await MusicManager.SpotifyWebPlayNextAsync();
                    MusicStateManager.getInstance.GatherMusicInfo();
                }
            }
            catch (Exception ex)
            {

                
            }
           
        }

        public static async Task LaunchDesktopApp()
        {
            string userHomeDir = Environment.ExpandEnvironmentVariables("%APPDATA%");
            string strCmdText = Path.Combine(userHomeDir, "Spotify\\Spotify.exe");
            Process process = new Process();
            process.StartInfo.FileName = strCmdText;
            process.StartInfo.CreateNoWindow = false;
            process.Start();
           
        }
        public static async Task LaunchPlayerAsync(options options)
        {

            LaunchWebPlayerAsync(new options());

           // Thread.Sleep(5000);

            if (!string.IsNullOrEmpty(options.playlist_id))
            {
               await MusicManager.SpotifyPlayPlaylistAsync(options.playlist_id, options.track_id);
            }
            else
            {
                await MusicManager.SpotifyPlayPlaylistAsync(null, options.track_id);
            }

        }

        private static void LaunchWebPlayerAsync(options options )
        {
            CodyConfig codyConfig   = CodyConfig.getInstance;
            string userID           = codyConfig.spoftifyUserId;

            if (SoftwareUserSession.GetSpotifyUserStatus())
            {
                if (!string.IsNullOrEmpty(options.album_id))
                {
                    string albumId = MusicUtil.CreateSpotifyIdFromUri(options.album_id);
                    SoftwareSpotifyManager.launchWebUrl("https://open.spotify.com/album/" + albumId);
                }
                else if (!string.IsNullOrEmpty(options.track_id))
                {
                    string trackId = MusicUtil.CreateSpotifyIdFromUri(options.track_id);
                    SoftwareSpotifyManager.launchWebUrl("https://open.spotify.com/track/" + trackId);

                }
                else if (!string.IsNullOrEmpty(options.playlist_id))
                {
                    string playlistId = MusicUtil.CreateSpotifyIdFromUri(options.playlist_id);

                    SoftwareSpotifyManager.launchWebUrl("https://open.spotify.com/playlist/" + playlistId);
                }
                else
                    SoftwareSpotifyManager.launchWebUrl("https://open.spotify.com/browse");


                MusicTimeCoPackage.UpdateEnablePlayercontrol(true);
            }
        }

       
      

     
    }
   


}
