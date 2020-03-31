using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
    //[ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    //[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    //[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(SpotifyPlayList))]
    public sealed class MusicTimeCoPackage : AsyncPackage
    {
        /// <summary>
        /// MusicTimeCoPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "cfba9e1f-15c0-4c56-806f-2a8f5060a535";
        public static DTE2 ObjDte;
        private DocumentEvents _docEvents;
        private DTEEvents _dteEvents;
        private TextDocumentKeyPressEvents _textDocKeyEvent;

        private System.Threading.Timer timer;
        private System.Threading.Timer DeviceTimer;
        private System.Threading.Timer TrackStatusBar;
        private System.Threading.Timer getSlackChannelTimer;
        private System.Threading.Timer OnlineCheckerTimer;
        private System.Threading.Timer offlineDataTimer;


        public LikeSongButton _likeSongButton;
        private bool _addedStatusBarButton = false;
        private static int ONE_SECOND = 1000;
        private static int THIRTY_SECONDS = 1000 * 30;
        private static int ONE_MINUTE = THIRTY_SECONDS * 2;
        private static int ONE_HOUR = ONE_MINUTE * 60;
        private static int THIRTY_MINUTES = ONE_MINUTE * 30;
        private static int ZERO_SECOND = 1;
        
        public static bool isValidRunningOrPausedTrack = false;
        public static bool isOnline = false;
       
        private static MusicStatusBar _musicStatus ;
        private static TrackStatus trackStatus  = new TrackStatus();
        private SoftwareData _softwareData;
        public static JsonObject KeystrokeData  = new JsonObject();
        private DateTime _lastPostTime          = DateTime.UtcNow;
        public static bool slackConnected           = false;
        public static List<Channel> SlackChannels   = null;
        public static List<Track> RecommendedTracks = new List<Track>();
        public static string RecommendedType        = "";
        public static bool isOffsetChange           = false;
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
            await JoinableTaskFactory.SwitchToMainThreadAsync(DisposalToken);
            Logger.Debug("Initialization");
            await InitializeSoftwareStatusAsync();
            Events2 events      = (Events2)ObjDte.Events;
            _textDocKeyEvent    = events.TextDocumentKeyPressEvents;
            _docEvents          = ObjDte.Events.DocumentEvents;

            // setup event handlers
            _textDocKeyEvent.AfterKeyPress  += AfterKeyPressedAsync;
            _docEvents.DocumentOpened       += DocEventsOnDocumentOpenedAsync;
            _docEvents.DocumentClosing      += DocEventsOnDocumentClosedAsync;
            _docEvents.DocumentSaved        += DocEventsOnDocumentSaved;
            _docEvents.DocumentOpening      += DocEventsOnDocumentOpeningAsync;

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
           
           // InitializeStatusBar();

            var autoEvent = new AutoResetEvent(false);

            OnlineCheckerTimer = new System.Threading.Timer(
                     OnlineCheckUpdate,
                     null,
                     ONE_MINUTE,
                     ONE_MINUTE );

            offlineDataTimer = new System.Threading.Timer(
                      SendOfflineData,
                      null,
                      THIRTY_MINUTES,
                      THIRTY_MINUTES);

            DeviceTimer = new System.Threading.Timer(
                     GetDeviceIDLazilyAsync,
                     null,
                     THIRTY_SECONDS/2,
                     ONE_SECOND*10);

            TrackStatusBar = new System.Threading.Timer(
                     UpdateCurrentTrackOnStatusAsync,
                     null,
                     ZERO_SECOND,
                     ONE_SECOND*5);

            getSlackChannelTimer = new Timer(getSlackChannelsAsync, null, ZERO_SECOND, ONE_MINUTE);
          //  Logger.Debug(GetExtensionInstallationDirectoryOrNull());

            this.InitializeUserInfoAsync();



        }
        private async void getSlackChannelsAsync(object state)
        {
            if (slackConnected)
            {
                SlackChannels = await SlackControlManager.GetSalckChannels();

            }
        }
        public  async void SendOfflineData(object stateinfo)
        {
          
            Logger.Info(DateTime.Now.ToString());
            bool online = isOnline;

            if (!online)
            {
                return;
            }

            string datastoreFile = SoftwareCoUtil.getSoftwareDataStoreFile();
            if (File.Exists(datastoreFile) && isCodetimeInstalled() == false)
            {
                // get the content
                string[] lines = File.ReadAllLines(datastoreFile, System.Text.Encoding.UTF8);

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

        public async void LaunchReadmeFileAsync()
        {
            try
            {
                string vsReadmeFile = SoftwareCoUtil.getVSReadmeFile();
                if (File.Exists(vsReadmeFile))
                {
                    MusicTimeCoPackage.ObjDte.ItemOperations.OpenFile(vsReadmeFile);
                }
                else
                {
                    Assembly _assembly = Assembly.GetExecutingAssembly();
                    string[] resourceNames = _assembly.GetManifestResourceNames();
                    string fileName = "README.txt";
                    string readmeFile = resourceNames.Single(n => n.EndsWith(fileName, StringComparison.InvariantCultureIgnoreCase));
                    if (readmeFile == null && resourceNames != null && resourceNames.Length > 0)
                    {
                        foreach (string name in resourceNames)
                        {
                            if (name.IndexOf("README") != -1)
                            {
                                readmeFile = fileName;
                                break;
                            }
                        }
                    }
                    if (readmeFile != null)
                    {
                        // SoftwareCoPackage.ObjDte.ItemOperations.OpenFile(readmeFile);
                        StreamReader _textStreamReader = new StreamReader(_assembly.GetManifestResourceStream(readmeFile));
                        string readmeContents = _textStreamReader.ReadToEnd();
                        File.WriteAllText(vsReadmeFile, readmeContents, System.Text.Encoding.UTF8);
                        MusicTimeCoPackage.ObjDte.ItemOperations.OpenFile(vsReadmeFile);
                    }
                }
            }
            catch (Exception ex)
            {
                //
            }
        }

        private async void InitializeUserInfoAsync()
        {
            string readmefile = (string) SoftwareCoUtil.getItem("displayedReadmefile");
            if(string.IsNullOrEmpty (readmefile) || readmefile != "true")
            {
                LaunchReadmeFile();
                
            }
            bool jwtExists  = SoftwareCoUtil.jwtExists();
            UpdateMusicStatusBar(false);
            Logger.Debug("Onlinecheck");
            await isOnlineCheckAsync();
            Logger.Debug(isOnline.ToString());
            bool online = isOnline;
            if (!jwtExists || !online)
            {
                
                return;
            }
            else
            {
                UserStatus status = await GetSpotifyUserStatusTokenAsync(online);
                UpdateMusicStatusBar(status.loggedIn);
                
            }
        }

        public static async Task LaunchReadmeFile()
        {
           
            string dashboardFile = SoftwareCoUtil.getReadmeFile();
            if (File.Exists(dashboardFile))
            {
                ObjDte.ItemOperations.OpenFile(dashboardFile);

                SoftwareCoUtil.setItem("displayedReadmefile", "true");
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

        public static async void LikeUnlikeSong()
        {
            bool isSongLiked        = false;
            List<Track> LikedSongs  = new List<Track>();

            if (MusicManager.isDeviceOpened())
            {
                trackStatus = await MusicManager.SpotifyCurrentTrackAsync();
                if (trackStatus != null)
                {

                    if (trackStatus.item != null)
                    {
                        LikedSongs = await Playlist.getSpotifyLikedSongsAsync();

                        foreach (Track item in LikedSongs)
                        {
                            if (item.id == trackStatus.item.id)
                            {
                                isSongLiked = true;
                                break;
                            }

                        }

                        if(isSongLiked)
                        {
                            MusicManager.removeToSpotifyLiked(trackStatus.item.id ,isSongLiked);

                        }
                        else
                        {
                           
                            MusicManager.saveToSpotifyLiked(trackStatus.item.id);
                        }


                    }

                }

            }



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
            try
            {
                SoftwareConnectSpotifyCommand.UpdateEnabledState(status);
                SoftwareDisconnectSpotifyCommand.UpdateEnabledState(status);
                SoftwareMusicTimeDashBoardCommand.UpdateEnabledState(status);
                OpenSpotifyCommand.UpdateEnabledState(status);
                UpdateEnablePlayercontrol(status);
            }
            catch (Exception ex)
            {

               
            }
               

        }

        public static void UpdateEnablePlayercontrol(bool status)
        {
            NextTrackCommand.UpdateEnabledState(status);
            PreviousTrackCommand.UpdateEnabledState(status);
            PlayPauseCommand.UpdateEnabledState(status);

        }

        public static  async void UpdateCurrentTrackOnStatusAsync(object state)
        {
            string currentTrack = "";
            string Pause        = "⏸️";
            string Play         = "▶️";
            string Liked        = "";
        
            
            string spotify_accessToken = "";
            List<Track> LikedSongs = new List<Track>();
            try
            {
                spotify_accessToken = (string)SoftwareCoUtil.getItem("spotify_access_token");

                if (String.IsNullOrEmpty(spotify_accessToken))
                {

                    UpdateMusicStatusBar(false);

                }
                else if (SoftwareUserSession.GetSpotifyUserStatus())
                {
                    if (MusicManager.isDeviceOpened()&& MusicManager.isDeviceActive())
                    {
                        trackStatus = await MusicManager.GetCurrentTrackAsync();
                        
                          
                            if (trackStatus.item != null)
                            {
                                if (trackStatus.actions != null)
                                {
                                    if (trackStatus.actions.disallows.skipping_prev == true)
                                    {
                                      PreviousTrackCommand.UpdateDisabeledState(false);
                                    }
                                    else
                                        PreviousTrackCommand.UpdateDisabeledState(true);
                                }

                            MusicStateManager.getInstance.GatherMusicInfo(trackStatus.item);

                            LikedSongs = await Playlist.getSpotifyLikedSongsAsync();

                                foreach (Track item in LikedSongs)
                                {
                                    if (item.id == trackStatus.item.id)
                                    {
                                        Liked = "🧡";
                                        break;
                                    }

                                }

                                if (trackStatus.is_playing == true)
                                {
                                    currentTrack = trackStatus.item.name;
                                    _musicStatus.SetTrackName(Pause + " " + currentTrack + " " + Liked);
                                    isValidRunningOrPausedTrack = true;


                                }
                                if (trackStatus.is_playing == false)
                                {
                                    currentTrack = trackStatus.item.name;
                                    _musicStatus.SetTrackName(Play + " " + currentTrack + " " + Liked);
                                    isValidRunningOrPausedTrack = true;
                                }
                            }
                                                
                    }
                    else
                    {

                        UpdateMusicStatusBar(true);
                        isValidRunningOrPausedTrack = false;
                    }
                }
                else
                {

                    isValidRunningOrPausedTrack = false;
                    UpdateMusicStatusBar(false);
                }
            }
            catch (Exception ex)
            {

               
            }
            
           
            
        }

        

        public async Task InitializeStatusBar()
        {
            try
            {
                if (_addedStatusBarButton)
                {
                    return;
                }
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                DockPanel statusBarObj = FindChildControl<DockPanel>(System.Windows.Application.Current.MainWindow, "StatusBarPanel");
                if (statusBarObj != null)
                {
                    statusBarObj.Children.Insert(0, _likeSongButton);
                    _addedStatusBarButton = true;
                }

            }
            catch (Exception ex)
            {

                
            }
                    }

        public async Task disposeStatusBar()
        {
            DockPanel statusBarObj = FindChildControl<DockPanel>(System.Windows.Application.Current.MainWindow, "StatusBarPanel");
            if (statusBarObj != null)
            {
                statusBarObj.Children.Clear();
                
            }
        }

        public T FindChildControl<T>(DependencyObject parent, string childName)
          where T : DependencyObject
        {
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                T childType = child as T;
                if (childType == null)
                {

                    foundChild = FindChildControl<T>(child, childName);


                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {

                    if (child is FrameworkElement frameworkElement && frameworkElement.Name == childName)
                    {

                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }


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
            await IntialisefileMap(fileName);
        }

        private async void AfterKeyPressedAsync(
            string Keypress, TextSelection Selection, bool InStatementCompletion)
        {
            String fileName = ObjDte.ActiveWindow.Document.FullName;
            InitializeSoftwareData(fileName);

            //Sets end and local_end for source file
            await IntialisefileMap(fileName);

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
            await IntialisefileMap(fileName);
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
            await IntialisefileMap(fileName);
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
        private void InitializeSoftwareData(string fileName)
        {
            string MethodName = "InitializeSoftwareData";
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
        private async Task IntialisefileMap(string fileName)
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
            double offset   = 0;
            long end        = 0;
            long local_end  = 0;

            NowTime nowTime = SoftwareCoUtil.GetNowTime();
            DateTime now    = DateTime.UtcNow;
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

                if (SoftwareCoUtil.isTelemetryOn() && MusicManager.hasSpotifyPlaybackAccess())
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
            if (_softwareData != null && isCodetimeInstalled() == false)
            {
                
                string softwareDataContent  = _softwareData.GetAsJson();

                string datastoreFile        = SoftwareCoUtil.getSoftwareDataStoreFile();
                // append to the file
                File.AppendAllText(datastoreFile, softwareDataContent + Environment.NewLine);
                
            }
            if(isValidRunningOrPausedTrack)
            {
                string musicDataFile = SoftwareCoUtil.getMusicDataStoreFile();
                File.AppendAllText(musicDataFile, _softwareData.GetAsJson() + Environment.NewLine);
            }
        }

        public bool isCodetimeInstalled()
        {
            bool isCodetimeInstalled = false;

            try
            {
                isCodetimeInstalled = (bool)SoftwareCoUtil.getItem("visualstudio_CtInit");
            }
            catch (Exception ex)
            {

                
            }
            

            //if(string.IsNullOrEmpty(visualstudio_CtInit))
            //{
            //    isCodetimeInstalled = true;
            //}

            return isCodetimeInstalled;
        }

        private void OnOnStartupComplete()
        {
            //
        }
        #endregion



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
        public void Dispose()
        {
            if (timer != null)
            {
                _textDocKeyEvent.AfterKeyPress -= AfterKeyPressedAsync;
                _docEvents.DocumentOpened -= DocEventsOnDocumentOpenedAsync;
                _docEvents.DocumentClosing -= DocEventsOnDocumentClosedAsync;
                _docEvents.DocumentSaved -= DocEventsOnDocumentSaved;
                _docEvents.DocumentOpening -= DocEventsOnDocumentOpeningAsync;

                timer.Dispose();
                timer = null;

                // process any remaining data
                // ProcessSoftwareDataTimerCallbackAsync(null);
            }
        }
        #endregion
    }
}
