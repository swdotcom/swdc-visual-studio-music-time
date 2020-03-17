using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicTime
{
    class MusicController
    {    
       private static MusicManager musicManager = MusicManager.getInstance;
       private static Device device             = Device.getInstance;
       private static MusicStateManager musicStateManager = MusicStateManager.getInstance;
        private static MusicController instance = null;
        private MusicController()
        {
        }

        public static MusicController getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MusicController();
                }
                return instance;
            }
        }
       
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
                       // await LaunchPlayerAsync(new options());

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
                        //await LaunchPlayerAsync(new options());

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
                       // await LaunchPlayerAsync(new options());

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
            bool flag = await checkInstalled("Spotify");
            if ( flag )
            {
                try
                {
                    string userHomeDir = Environment.ExpandEnvironmentVariables("%APPDATA%");
                    string strCmdText = Path.Combine(userHomeDir, "Spotify\\Spotify.exe");
                    Process process = new Process();
                    process.StartInfo.FileName = strCmdText;
                    process.StartInfo.CreateNoWindow = false;
                    process.Start();


                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                string message = "Desktop player is not available"; /*, open web player instead ?*/
                string title    = "SPOTIFY";

                //MessageBox.Show(message);
               // MessageBoxButtons buttons = MessageBoxButtons.OKCancel;
                DialogResult result = MessageBox.Show(message, title);
                if (result == DialogResult.OK)
                {
                    LaunchWebPlayerAsync(new options());
                }
               
            }

            
        }

        public static async void launchDevicePrompt()
        {


        }

        public static async Task< bool> checkInstalled(string c_name)
        {
            string displayName;

            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return true;
                    }
                }
                key.Close();
            }

            registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            key = Registry.CurrentUser.OpenSubKey(registryKey);
            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(c_name))
                    {
                        return true;
                    }
                }
                key.Close();
            }
            return false;
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
                    SoftwareSpotifyManager.launchWebUrl("https://open.spotify.com");


                MusicTimeCoPackage.UpdateEnablePlayercontrol(true);
            }
        }

       
      

     
    }
   


}
