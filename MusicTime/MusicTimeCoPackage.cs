using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using MusicTime.PlayerCommands;
using static MusicTime.SoftwareUserSession;
using Task = System.Threading.Tasks.Task;

namespace MusicTime
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(MusicTimeCoPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class MusicTimeCoPackage : AsyncPackage
    {
        /// <summary>
        /// MusicTimeCoPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "cfba9e1f-15c0-4c56-806f-2a8f5060a535";
        public static DTE2 ObjDte;
        private DTEEvents _dteEvents;
        private System.Threading.Timer timer;
        private System.Threading.Timer DeviceTimer;
        private System.Threading.Timer TrackStatusBar;

        private static int THIRTY_SECONDS = 1000 * 30;
        private static int ONE_MINUTE = THIRTY_SECONDS * 2;
        private static int ONE_HOUR = ONE_MINUTE * 60;
        private static int THIRTY_MINUTES = ONE_MINUTE * 30;
        private static long lastDashboardFetchTime = 0;
        private static long day_in_sec = 60 * 60 * 24;
        private static int ZERO_SECOND = 1;
        private bool connected = false;

        public static UserStatus spotifyUser = new UserStatus();
        private static MusicStatusBar _musicStatus ;
        private static TrackStatus trackStatus = new TrackStatus();
        /// <summary>
        /// Initializes a new instance of the <see cref="MusicTimeCoPackage"/> class.
        /// </summary>
        public MusicTimeCoPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            ObjDte = await GetServiceAsync(typeof(DTE)) as DTE2;
            _dteEvents = ObjDte.Events.DTEEvents;
            _dteEvents.OnStartupComplete += OnOnStartupComplete;
            InitializeListenersAsync();
         
           
         
        }

        private void OnOnStartupComplete()
        {
            
        }

        public static string GetVersion()
        {
            return string.Format("{0}.{1}.{2}", MusicTimeAssembly.Version.Major, MusicTimeAssembly.Version.Minor, MusicTimeAssembly.Version.Build);
        }

        public static string GetOs()
        {
            return System.Environment.OSVersion.VersionString;
        }
        private async Task InitializeListenersAsync()
        {
            await InitializeSoftwareStatusAsync();
           
            //Music Commands
            await SoftwareConnectSpotifyCommand.InitializeAsync(this);
            await SoftwareDisconnectSpotifyCommand.InitializeAsync(this);
            await SoftwareMusicTimeDashBoardCommand.InitializeAsync(this);
            await SoftwareConnectSlackCommand.InitializeAsync(this);
            await SoftwareDisconnectSlackCommand.InitializeAsync(this);
            await SoftwareSubmitOnGithubCommand.InitializeAsync(this);
            await SoftwareSubmitFeedbackCommand.InitializeAsync(this);

            //PlayerControls

            await NextTrackCommand.InitializeAsync(this);
            await PreviousTrackCommand.InitializeAsync(this);
            await PlayPauseCommand.InitializeAsync(this);
            await OpenSpotifyCommand.InitializeAsync(this);

            updateMusicStatusBar(connected);

            var autoEvent = new AutoResetEvent(false);

            timer    = new System.Threading.Timer(
                     GetSpotifyUserStatus,
                     null,
                     ZERO_SECOND,
                     THIRTY_SECONDS/3);

            DeviceTimer = new System.Threading.Timer(
                     GetDeviceIDLazilyAsync,
                     null,
                     THIRTY_SECONDS,
                     THIRTY_SECONDS/3);

            TrackStatusBar = new System.Threading.Timer(
                     UpdateCurrentTrackOnStatusAsync,
                     null,
                     THIRTY_SECONDS,
                     THIRTY_SECONDS / 6);


        }

        public static void GetSpotifyUserStatus(object state)
        {
            spotifyUser.loggedIn = SoftwareUserSession.GetSpotifyUserStatus();     
            UpdateEnableCommands(spotifyUser.loggedIn);

           // UpdateEnablePlayercontrol(MusicManager.isDeviceOpened());
        }

        public static async void GetDeviceIDLazilyAsync(object state)
        {
          
            if (SoftwareUserSession.GetSpotifyUserStatus())
            {
               await MusicManager.getDevicesAsync();
            }
        }

        private static void updateMusicStatusBar(bool Connected)
        {
           
            _musicStatus.SetStatus(Connected);
            
        }

        private static void UpdateEnableCommands(bool status)
        {
            
                SoftwareConnectSpotifyCommand.UpdateEnabledState(status);
                SoftwareDisconnectSpotifyCommand.UpdateEnabledState(status);
                SoftwareMusicTimeDashBoardCommand.UpdateEnabledState(status);
                OpenSpotifyCommand.UpdateEnabledState(status);
                UpdateEnablePlayercontrol(status);

        }

        public static void UpdateEnablePlayercontrol(bool status)
        {
            NextTrackCommand.UpdateEnabledState(status);
            PreviousTrackCommand.UpdateEnabledState(status);
            PlayPauseCommand.UpdateEnabledState(status);

        }

        public static async void UpdateCurrentTrackOnStatusAsync(object state)
        {
            string currentTrack = "";
            string Pause    = "⏸️";
            string Play     = "▶️";

            if (SoftwareUserSession.GetSpotifyUserStatus()&& MusicManager.isDeviceOpened())
           {
                trackStatus = await MusicManager.SpotifyCurrentTrackAsync();
                
                if(trackStatus.is_playing == true && trackStatus.item != null)
                {
                    currentTrack = trackStatus.item.name;
                    _musicStatus.SetTrackName(Play + " " + currentTrack);
                }
                if(trackStatus.is_playing==false&& trackStatus.item!=null)
                {
                    currentTrack = trackStatus.item.name;
                    _musicStatus.SetTrackName(Pause + " " + currentTrack);
                }
           }
            else
            {
                updateMusicStatusBar(SoftwareUserSession.GetSpotifyUserStatus());
            }
        }

        public static class MusicTimeAssembly
        {
            static readonly Assembly Reference      = typeof(MusicTimeAssembly).Assembly;
            public static readonly Version Version  = Reference.GetName().Version;
        }

        public static void SubmitIssueOnGithub()
        {
            try
            {
                System.Diagnostics.Process.Start(Constants.VSGithubLink);
            }
            catch ( Exception e)
            {

                throw;
            }
          
        }

        public static void SendFeedBack()
        {
            try
            {
                System.Diagnostics.Process.Start(Constants.SendFeedback);
            }
            catch (Exception e)
            {

                throw;
            }

        }

        private async Task InitializeSoftwareStatusAsync()
        {
            if (_musicStatus == null)
            {
                IVsStatusbar statusbar = await GetServiceAsync(typeof(SVsStatusbar)) as IVsStatusbar;
                _musicStatus = new MusicStatusBar(statusbar);
            }
        }

        #endregion
    }
}
