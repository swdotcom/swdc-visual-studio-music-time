using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net.Http;
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
    [ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(SpotifyPlayList))]
    public sealed class MusicTimeCoPackage : AsyncPackage
    {
        /// <summary>
        /// MusicTimeCoPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "cfba9e1f-15c0-4c56-806f-2a8f5060a535";

        public static DTE2 ObjDte;
        private DTEEvents _dteEvents;
        private DocumentEvents _docEvents;
        private WindowEvents _windowEvents;
        private TextDocumentKeyPressEvents _textDocKeyEvent;

        private System.Threading.Timer timer;
        private System.Threading.Timer DeviceTimer;
        private System.Threading.Timer TrackStatusBar;
        private System.Threading.Timer OnlineCheckerTimer;
        private System.Threading.Timer PlaylistUpdate;


        private static int ONE_SECOND = 1000;
        private static int THIRTY_SECONDS = 1000 * 30;
        private static int ONE_MINUTE = THIRTY_SECONDS * 2;
        private static int ONE_HOUR = ONE_MINUTE * 60;
        private static int THIRTY_MINUTES = ONE_MINUTE * 30;
        private static long lastDashboardFetchTime = 0;
        private static long day_in_sec = 60 * 60 * 24;
        private static int ZERO_SECOND = 1;
        private bool connected = false;

        public static bool isOnline = false;
       // public static UserStatus spotifyUser = new UserStatus();
        private static MusicStatusBar _musicStatus ;
        private static TrackStatus trackStatus = new TrackStatus();


        private DateTime _lastPostTime = DateTime.UtcNow;
        private SoftwareData _softwareData;
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
            Logger.Debug("Initialization");

            // VisualStudio Object
            Events2 events      = (Events2)ObjDte.Events;
            _textDocKeyEvent    = events.TextDocumentKeyPressEvents;
            _docEvents          = ObjDte.Events.DocumentEvents;

            // setup event handlers
            _textDocKeyEvent.AfterKeyPress  += AfterKeyPressedAsync;
            _docEvents.DocumentOpened       += DocEventsOnDocumentOpenedAsync;
            _docEvents.DocumentClosing      += DocEventsOnDocumentClosedAsync;
            _docEvents.DocumentSaved        += DocEventsOnDocumentSaved;
            _docEvents.DocumentOpening      += DocEventsOnDocumentOpeningAsync;



            await InitializeSoftwareStatusAsync();
           
            //Music Commands
            await SoftwareConnectSpotifyCommand.InitializeAsync(this);
            await SoftwareDisconnectSpotifyCommand.InitializeAsync(this);
            await SoftwareMusicTimeDashBoardCommand.InitializeAsync(this);
            await SoftwareConnectSlackCommand.InitializeAsync(this);
            await SoftwareDisconnectSlackCommand.InitializeAsync(this);
            await SoftwareSubmitOnGithubCommand.InitializeAsync(this);
            await SoftwareSubmitFeedbackCommand.InitializeAsync(this);
            await SpotifyPlayListCommand.InitializeAsync(this);
            //PlayerControls

            await NextTrackCommand.InitializeAsync(this);
            await PreviousTrackCommand.InitializeAsync(this);
            await PlayPauseCommand.InitializeAsync(this);
            await OpenSpotifyCommand.InitializeAsync(this);

           

            var autoEvent = new AutoResetEvent(false);

            OnlineCheckerTimer = new System.Threading.Timer(
                     OnlineCheckUpdate,
                     null,
                     ONE_MINUTE,
                     ONE_MINUTE );

            //timer = new System.Threading.Timer(
            //         UpdateUserStatusAsync,
            //         null,
            //        ONE_MINUTE / 6,
            //        ONE_MINUTE / 6);

            DeviceTimer = new System.Threading.Timer(
                     GetDeviceIDLazilyAsync,
                     null,
                     THIRTY_SECONDS/3,
                     ONE_SECOND*5);

            TrackStatusBar = new System.Threading.Timer(
                     UpdateCurrentTrackOnStatusAsync,
                     null,
                     ZERO_SECOND,
                     ONE_SECOND*2);

            //PlaylistUpdate = new System.Threading.Timer(
            //         UpdatePlaylistCallBackAsync,
            //         null,
            //         ONE_MINUTE / 2,
            //         ONE_MINUTE / 4);

            this.InitializeUserInfoAsync();



        }

        //private async void UpdatePlaylistCallBackAsync(object state)
        //{
        //    if (isOnline)
        //    {
        //        //Playlist.Liked_Playlist     = new List<Track>();
        //        //Playlist.Software_Playlists = new List<Track>();

        //        //Playlist.Liked_Playlist     = await Playlist.getSpotifyLikedSongsAsync();
        //        //Playlist.Software_Playlists = await Playlist.getPlaylistTracksAsync(Constants.SOFTWARE_TOP_40_ID);
        //        await UpdateUsersPlaylistsAsync();
        //    }

        //}

        //private async Task UpdateUsersPlaylistsAsync()
        //{
        //    try
        //    {
        //        List<Track> tracks = new List<Track>();
        //        List<PlaylistItem> playlistItems    = await Playlist.getPlaylistsAsync();
        //        Playlist.Users_Playlist             = new Dictionary<PlaylistItem, List<Track>>();

        //        foreach (PlaylistItem playlists in playlistItems)
        //        {
        //            tracks = await Playlist.getPlaylistTracksAsync(playlists.id);
        //            Playlist.Users_Playlist.Add(playlists, tracks);

        //        }

        //    }
        //    catch (Exception e)
        //    {


        //    }

        //}

        private async void InitializeUserInfoAsync()
        {
           
            bool jwtExists  = SoftwareCoUtil.jwtExists();
            UpdateMusicStatusBar(false);
            await SoftwareUserSession.isOnlineCheckAsync();
          
            bool online = MusicTimeCoPackage.isOnline;
            if (!jwtExists || !online)
            {
                return;
            }
            else
            {
                SoftwareUserSession.UserStatus status = await SoftwareUserSession.GetSpotifyUserStatusTokenAsync(online);
                UpdateMusicStatusBar(status.loggedIn);
            }
        }

        public void Dispose()
        {
            if (timer != null)
            {
                _textDocKeyEvent.AfterKeyPress  -= AfterKeyPressedAsync;
                _docEvents.DocumentOpened       -= DocEventsOnDocumentOpenedAsync;
                _docEvents.DocumentClosing      -= DocEventsOnDocumentClosedAsync;
                _docEvents.DocumentSaved        -= DocEventsOnDocumentSaved;
                _docEvents.DocumentOpening      -= DocEventsOnDocumentOpeningAsync;

                timer.Dispose();
                timer = null;

                // process any remaining data
                // ProcessSoftwareDataTimerCallbackAsync(null);
            }
        }
        #endregion

        #region Event Handlers

        private void DocEventsOnDocumentSaved(Document document)
        {
            if (document == null || document.FullName == null)
            {
                return;
            }
            String fileName = document.FullName;
            if (_softwareData == null || !_softwareData.source.ContainsKey(fileName))
            {
                return;
            }

            InitializeSoftwareData(fileName);

            FileInfo fi = new FileInfo(fileName);

            _softwareData.UpdateData(fileName, "length", fi.Length);

            
        }

        private async void DocEventsOnDocumentOpeningAsync(String docPath, Boolean readOnly)
        {
            FileInfo fi = new FileInfo(docPath);
            String fileName = fi.FullName;
            InitializeSoftwareData(fileName);

            //Sets end and local_end for source file
            await _IntialisefileMap(fileName);
        }

        private async void AfterKeyPressedAsync(
            string Keypress, TextSelection Selection, bool InStatementCompletion)
        {
            String fileName = ObjDte.ActiveWindow.Document.FullName;
            InitializeSoftwareData(fileName);

            //Sets end and local_end for source file
            await _IntialisefileMap(fileName);

            if (ObjDte.ActiveWindow.Document.Language != null)
            {
                _softwareData.addOrUpdateFileStringInfo(fileName, "syntax", ObjDte.ActiveWindow.Document.Language);
            }
            if (!String.IsNullOrEmpty(Keypress))
            {
                FileInfo fi = new FileInfo(fileName);

                bool isNewLine = false;
                if (Keypress == "\b")
                {
                    // register a delete event
                    _softwareData.UpdateData(fileName, "delete", 1);
                    Logger.Info("Code Time: Delete character incremented");
                }
                else if (Keypress == "\r")
                {
                    isNewLine = true;
                }
                else
                {
                    _softwareData.UpdateData(fileName, "add", 1);
                    Logger.Info("Code Time: KPM incremented");
                }

                if (isNewLine)
                {
                    _softwareData.addOrUpdateFileInfo(fileName, "linesAdded", 1);
                }

                _softwareData.keystrokes += 1;
            }
        }

        private async void DocEventsOnDocumentOpenedAsync(Document document)
        {
            if (document == null || document.FullName == null)
            {
                return;
            }
            String fileName = document.FullName;
            if (_softwareData == null || !_softwareData.source.ContainsKey(fileName))
            {
                return;
            }
            //Sets end and local_end for source file
            await _IntialisefileMap(fileName);
            try
            {
                _softwareData.UpdateData(fileName, "open", 1);
                Logger.Info("Code Time: File open incremented");
            }
            catch (Exception ex)
            {
                Logger.Error("DocEventsOnDocumentOpened", ex);
            }
        }

        private async void DocEventsOnDocumentClosedAsync(Document document)
        {
            if (document == null || document.FullName == null)
            {
                return;
            }
            String fileName = document.FullName;
            if (_softwareData == null || !_softwareData.source.ContainsKey(fileName))
            {
                return;
            }
            //Sets end and local_end for source file
            await _IntialisefileMap(fileName);
            try
            {
                _softwareData.UpdateData(fileName, "close", 1);
                Logger.Info("Code Time: File close incremented");
            }
            catch (Exception ex)
            {
                Logger.Error("DocEventsOnDocumentClosed", ex);
            }
        }

        private void OnOnStartupComplete()
        {
            //
        }
        #endregion

        #region Methods

        private void InitializeSoftwareData(string fileName)
        {
            NowTime nowTime = SoftwareCoUtil.GetNowTime();
            if (_softwareData == null || !_softwareData.initialized)
            {


                // get the project name
                String projectName = "Untitled";
                String directoryName = "Unknown";
                if (ObjDte.Solution != null && ObjDte.Solution.FullName != null && !ObjDte.Solution.FullName.Equals(""))
                {
                    projectName = Path.GetFileNameWithoutExtension(ObjDte.Solution.FullName);
                    string solutionFile = ObjDte.Solution.FullName;
                    directoryName = Path.GetDirectoryName(solutionFile);
                }
                else
                {
                    directoryName = Path.GetDirectoryName(fileName);
                }

                if (_softwareData == null)
                {
                    ProjectInfo projectInfo = new ProjectInfo(projectName, directoryName);
                    _softwareData = new SoftwareData(projectInfo);

                }
                else
                {
                    _softwareData.project.name = projectName;
                    _softwareData.project.directory = directoryName;
                }
                _softwareData.start = nowTime.now;
                _softwareData.local_start = nowTime.local_now;
                _softwareData.initialized = true;
                SoftwareCoUtil.SetTimeout(ONE_MINUTE, HasData, false);
            }
            _softwareData.EnsureFileInfoDataIsPresent(fileName, nowTime);
        }
        private async Task _IntialisefileMap(string fileName)
        {

            JsonObject localSource = new JsonObject();
            foreach (var sourceFiles in _softwareData.source)
            {
                object outend = null;
                JsonObject fileInfoData = null;
                NowTime nowTime = SoftwareCoUtil.GetNowTime();

                if (fileName != sourceFiles.Key)
                {
                    fileInfoData = (JsonObject)sourceFiles.Value;
                    fileInfoData.TryGetValue("end", out outend);

                    if (long.Parse(outend.ToString()) == 0)
                    {

                        fileInfoData["end"] = nowTime.now;
                        fileInfoData["local_end"] = nowTime.local_now;

                    }
                    localSource.Add(sourceFiles.Key, fileInfoData);
                }
                else
                {
                    fileInfoData = (JsonObject)sourceFiles.Value;
                    fileInfoData["end"] = 0;
                    fileInfoData["local_end"] = 0;
                    localSource.Add(sourceFiles.Key, fileInfoData);
                }

                _softwareData.source = localSource;

            }


        }
        public void HasData()
        {

            if (_softwareData.initialized && (_softwareData.keystrokes > 0 || _softwareData.source.Count > 0) && _softwareData.project != null && _softwareData.project.name != null)
            {

                SoftwareCoUtil.SetTimeout(ZERO_SECOND, PostData, false);
            }

        }

        public void PostData()
        {
            double offset = 0;
            long end = 0;
            long local_end = 0;

            NowTime nowTime = SoftwareCoUtil.GetNowTime();
            DateTime now = DateTime.UtcNow;
            if (_softwareData.source.Count > 0)
            {
                offset = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalMinutes;
                _softwareData.offset = Math.Abs((int)offset);
                if (TimeZone.CurrentTimeZone.DaylightName != null
                    && TimeZone.CurrentTimeZone.DaylightName != TimeZone.CurrentTimeZone.StandardName)
                {
                    _softwareData.timezone = TimeZone.CurrentTimeZone.DaylightName;
                }
                else
                {
                    _softwareData.timezone = TimeZone.CurrentTimeZone.StandardName;
                }

                foreach (KeyValuePair<string, object> sourceFiles in _softwareData.source)
                {

                    JsonObject fileInfoData = null;
                    fileInfoData = (JsonObject)sourceFiles.Value;
                    object outend;
                    fileInfoData.TryGetValue("end", out outend);

                    if (long.Parse(outend.ToString()) == 0)
                    {

                        end = nowTime.now;
                        local_end = nowTime.local_now;
                        _softwareData.addOrUpdateFileInfo(sourceFiles.Key, "end", end);
                        _softwareData.addOrUpdateFileInfo(sourceFiles.Key, "local_end", local_end);

                    }

                }

                try
                {

                    _softwareData.end = nowTime.now;
                    _softwareData.local_end = nowTime.local_now;

                }
                catch (Exception)

                {

                }

                string softwareDataContent = _softwareData.GetAsJson();
                Logger.Info("Code Time: sending: " + softwareDataContent);

                if (SoftwareCoUtil.isTelemetryOn())
                {
                    StorePayload(_softwareData);

                }
                else
                {
                    Logger.Info("Code Time metrics are currently paused.");

                }

                _softwareData.ResetData();
                _lastPostTime = now;
            }

        }

        private void StorePayload(SoftwareData _softwareData)
        {
            if (_softwareData != null)
            {
                
                string softwareDataContent = _softwareData.GetAsJson();

                string datastoreFile = SoftwareCoUtil.getSoftwareDataStoreFile();
                // append to the file
                File.AppendAllText(datastoreFile, softwareDataContent + Environment.NewLine);

               
            }
        }

        private async void SendOfflineData(object stateinfo)
        {
            Logger.Info(DateTime.Now.ToString());

            bool online = MusicTimeCoPackage.isOnline;
            if (!online)
            {
                return;
            }

            string datastoreFile = SoftwareCoUtil.getSoftwareDataStoreFile();
            if (File.Exists(datastoreFile))
            {
                // get the content
                string[] lines = File.ReadAllLines(datastoreFile);

                if (lines != null && lines.Length > 0)
                {
                    List<String> jsonLines = new List<string>();
                    foreach (string line in lines)
                    {
                        if (line != null && line.Trim().Length > 0)
                        {
                            jsonLines.Add(line);
                        }
                    }
                    string jsonContent = "[" + string.Join(",", jsonLines) + "]";
                    HttpResponseMessage response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Post, "/data/batch", jsonContent);
                    if (SoftwareHttpManager.IsOk(response))
                    {
                        // delete the file
                        File.Delete(datastoreFile);
                    }
                }
            }

           
        }


        public static async void UpdateUserStatusAsync(object state)
        {
            try
            {
                SoftwareUserSession.UserStatus status = await SoftwareUserSession.GetSpotifyUserStatusTokenAsync(true);
                UpdateEnableCommands(status.loggedIn);
               
            }
            catch (Exception e)
            {


            }

        }
        public static async void OnlineCheckUpdate(object state)
        {
            try
            {
               SoftwareUserSession.isOnlineCheckAsync();
            }
            catch (Exception e)
            {
                
            }

        }
        public static async void LaunchMusicTimeDashboardAsync()
        {

            await MusicManager.GetMusicTimeDashboardFileAsync();
            
            string dashboardFile = SoftwareCoUtil.getDashboardFile();
            if (File.Exists(dashboardFile))
                ObjDte.ItemOperations.OpenFile(dashboardFile);
            
            
        }

        public static async void GetDeviceIDLazilyAsync(object state)
        {
          
            if (SoftwareUserSession.GetSpotifyUserStatus())
            {
               await MusicManager.getDevicesAsync();
            }
        }

        public static void UpdateMusicStatusBar(bool Connected)
        {
           
            _musicStatus.SetStatus(Connected);
            
        }

        public static void UpdateEnableCommands(bool status)
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
            string Pause        = "⏸️";
            string Play         = "▶️";
            string spotify_accessToken = "";


            spotify_accessToken = (string)SoftwareCoUtil.getItem("spotify_access_token");
           
            if (String.IsNullOrEmpty(spotify_accessToken))
            {
               
                UpdateMusicStatusBar(false);
                
            }
            else if(SoftwareUserSession.GetSpotifyUserStatus())
            {
                if (MusicManager.isDeviceOpened())
                {
                    trackStatus = await MusicManager.SpotifyCurrentTrackAsync();
                    if (trackStatus.is_playing == true && trackStatus.item != null)
                    {
                        currentTrack = trackStatus.item.name;
                        _musicStatus.SetTrackName(Pause + " " + currentTrack);
                    }
                    if (trackStatus.is_playing == false && trackStatus.item != null)
                    {
                        currentTrack = trackStatus.item.name;
                        _musicStatus.SetTrackName(Play + " " + currentTrack);
                    }
                }
                else
                {
                  
                    UpdateMusicStatusBar(true);
                }
            }
            else 
            {
               
                UpdateMusicStatusBar(false);
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
