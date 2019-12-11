using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicTime
{
    class MusicController
    {
       private static MusicManager musicManager = MusicManager.GetInstance;
       private static Device device             = Device.getInstance;

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
                    }
                    else
                    {
                      
                        await MusicManager.SpotifyWebPlayAsync();
                        MusicTimeCoPackage.UpdateCurrentTrackOnStatusAsync(null);
                    }

                    if (!MusicManager.isDeviceOpened())
                    {
                        await LaunchPlayerAsync();

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
                        await LaunchPlayerAsync();

                        await MusicManager.getDevicesAsync();
                        
                    }


                    await MusicManager.SpotifyWebPlayPreviousAsync();
                   

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
                        await LaunchPlayerAsync();

                        await MusicManager.getDevicesAsync();
                        
                    }

                     await MusicManager.SpotifyWebPlayNextAsync();
                   
                }
            }
            catch (Exception ex)
            {

                
            }
           
        }

        public static async Task LaunchPlayerAsync()
        {
            
            if (SoftwareUserSession.GetSpotifyUserStatus())
            { 
                SoftwareSpotifyManager.launchWebUrl("https://open.spotify.com/");
                MusicTimeCoPackage.UpdateEnablePlayercontrol(true);
            }
            
        }

    }
   


}
