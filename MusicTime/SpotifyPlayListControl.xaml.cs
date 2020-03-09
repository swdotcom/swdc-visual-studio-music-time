namespace MusicTime
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for SpotifyPlayListControl.
    /// </summary>
    public partial class SpotifyPlayListControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpotifyPlayListControl"/> class.
        /// </summary>
        private static bool isConnected                 = false;
        private static Playlist _Playlist               = Playlist.getInstance;
        private static MusicManager musicManager        = MusicManager.getInstance;
        public static bool isAIPlaylistUpdated          = false;
        public static bool isUsersPlaylistUpdated       = false;
        public static bool isMusicTimePlaylistUpdated   = false;
        public static List<Track> LikedSongIds          = null;
        public static bool SortPlaylistFlag             = false;
        public static bool AIPlyalistGenerated          = false;
        public static string AIPlaylistID               = null;

        public static List<Device> WebDevices       = new List<Device>();
        public static List<Device> ComputerDevices  = new List<Device>();
        public static PlaylistItem AIPlaylistItem       = null;
     
        enum SortOrder
        {
           Alphabatically_Sort,
           Latest_Sort
        }

        public SpotifyPlayListControl()
        {
            this.InitializeComponent(); 
            Init();
        }
        
        private void Init()
        {
           
            SetConnectContent();
            System.Windows.Forms.Timer UpdateCallBackTimer = new System.Windows.Forms.Timer();
            UpdateCallBackTimer.Interval = 5000;//5 seconds
            UpdateCallBackTimer.Tick += new System.EventHandler(UpdateCallBack);
            UpdateCallBackTimer.Start();
            
        }
        private void UpdateCallBack(object sender, EventArgs e)
        {
            UpdateTreeviewAsync();
        }

        private void ConnectStatusClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isConnected = MusicManager.hasSpotifyPlaybackAccess();
            if (!isConnected)
            {
                SoftwareSpotifyManager.ConnectToSpotifyAsync();
            }
        }

        //checks user is connected or not ,sets static boolean variable 
        private async Task CheckUserStatusAsync()
        {
            bool online = MusicTimeCoPackage.isOnline;
            isConnected = MusicManager.hasSpotifyPlaybackAccess();
      
        }

        private async void RefreshAsync(object sender, RoutedEventArgs e)
        {
            if (isConnected)
            {
                try
                {
                    e.Handled = true;
                    btnRefresh.IsEnabled = false;
                   
                    SetGenerateAIContent();
                    await SoftwareTop40PlaylistAsync();
                    
                    await  UsersPlaylistAsync();
                    btnRefresh.IsEnabled = true;
                }
                finally
                {

                }

            }
        }

        

        public async Task UpdateTreeviewAsync()
        {
            try
            {
                await CheckUserStatusAsync();

                if (isConnected)
                {
                    btnRefresh.Visibility = Visibility.Visible;
                    SetConnectContent();
                    SetWebAnalyticsContent();
                    SetDeviceDetectionContentAsync();
                    SeperatorContent(); 
                    SetSortContent();
                    if (!isAIPlaylistUpdated)
                    {
                        SetGenerateAIContent();
                        AIPlaylistAsync();
                    }
                    if(!isMusicTimePlaylistUpdated)
                    {
                       // await LikedSongsPlaylistAsync();
                       await SoftwareTop40PlaylistAsync();
                    }
                 
                    if (!isUsersPlaylistUpdated) { UsersPlaylistAsync(); }
                        
                }
                else
                {
                    ClearAll();
                }
            }
            catch (Exception ex)
            {

                
            }
           
        }
        
        private void ClearAll()
        {
            try
            {
                btnRefresh.Visibility = Visibility.Hidden;
                UsersPlaylistTV.Items.Clear();
                SoftwarePlaylistTV.Items.Clear();
                LikePlaylistTV.Items.Clear();
                AIPlaylistTV.Items.Clear();
                isUsersPlaylistUpdated  = false;
                isAIPlaylistUpdated     = false;
                isMusicTimePlaylistUpdated = false;
                SetConnectContent();
                SetWebAnalyticsContent();
                SetDeviceDetectionContentAsync();
                SeperatorContent();
                GenerateAIContent();
                SetSortContent();
               
            }
            catch (Exception e)
            {


            }

        }

       


        //UI Setters for button /label /icons
        private void SetConnectContent()
        {
            try
            {
                if (isConnected)
                {
                    ConnectLabel.Content    = "Spotify Connected";
                    ConnectImage.Source     = new BitmapImage(new Uri("Resources/Connected.png", UriKind.Relative));
                }
                else
                {
                    ConnectLabel.Content    = "Connect Spotify";
                    ConnectImage.Source     = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                }

            }
            catch (Exception e)
            {


            }
        }
        private void SetWebAnalyticsContent()
        {
            if (isConnected)
            {

                AnalyticLabel.Content   = "See web analytics";
                AnalyticImage.Source    = new BitmapImage(new Uri("Resources/PAW.png", UriKind.Relative));
            }
            else
            {
                AnalyticLabel.Content   = null;
                AnalyticImage.Source    = null;
            }

        }
        //private async void SetDeviceDetectionContentAsyncold()
        //{
        //    if (isConnected)
        //    {
        //      //  DeviceLabel.Visibility = Visibility.Visible;
        //        if (MusicManager.isDeviceOpened())
        //        {
        //            DeviceImage.Source = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
        //            List<Device> devices = null;
        //            devices = MusicManager.getDevices();

        //            if (devices.Count > 0)
        //            {

        //                string Active_Device = MusicManager.getActiveDeviceName();

        //                if (!string.IsNullOrEmpty(Active_Device))
        //                {
        //                    DeviceLabel.Content = "Listening on " + MusicManager.getActiveDeviceName();
        //                    DeviceLabel.ToolTip = "Listening on a Spotify device";

        //                }
        //                else
        //                {
        //                    DeviceLabel.Content = "Connected on " + MusicManager.getDeviceNames();
        //                    if (devices.Count > 1)
        //                        DeviceLabel.ToolTip = "Multiple Spotify devices connected";
        //                    else
        //                        DeviceLabel.ToolTip = "Spotify device connected";
        //                }
        //            }
        //        }
        //        else
        //        {

        //            DeviceImage.Source = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
        //            DeviceLabel.Content = "Connect a spotify device";
        //            DeviceLabel.ContextMenu = await getDeviceContextMenu();
        //        }
        //    }
        //    else
        //    {
        //        DeviceLabel.Content = null;
        //        DeviceImage.Source = null;
        //    }

        //}
        


        private async void SetDeviceDetectionContentAsync()
        {
            if (isConnected)
            {

                if (MusicManager.isDeviceOpened())
                {
                    DeviceImage.Source   = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                 
                    List <Device> devices = MusicManager.getDevices();
                    
                    if (devices.Count>0)
                    {
                        WebDevices      = getWebDevices(devices);
                        ComputerDevices = getDesktopDevices(devices);
                        DeviceImage.Source = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                        DeviceLabel.Content = " Spotify device";
                        DeviceLabel.Click += DeviceLabel_ClickAsync;


                    }
                }
                else
                {

                    DeviceImage.Source      = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                    DeviceLabel.Content     = "Connect a spotify device";
                    WebDevices = new List<Device>();
                    ComputerDevices = new List<Device>();
                    DeviceLabel.Click       += DeviceLabel_ClickAsync; 
                   
                }
            }
            else
            {
                DeviceLabel.Content = null;
                DeviceImage.Source  = null;
            }

        }

        public async void PlayTrackFromContext(string playlist_id ,string track_id)
        {
            DeviceLabel.ContextMenu = await getDeveviceContext(WebDevices, ComputerDevices,playlist_id, track_id);
        }

        private async void DeviceLabel_ClickAsync(object sender, RoutedEventArgs e)
        {
            if(e!=null)
            e.Handled = true;

            DeviceLabel.ContextMenu = await getDeveviceContext(WebDevices, ComputerDevices,null,null);
          
        }

        private List<Device> getWebDevices(List<Device> devices)
        {
            List<Device> webDevices = new List<Device>();
            foreach (Device item in devices)
            {
                if(item.name.Contains("Web Player"))
                {
                    webDevices.Add(item);
                }
            }
            return webDevices;
        }

        private List<Device> getDesktopDevices(List<Device> devices)
        {
            List<Device> desktopDevices = new List<Device>();
            foreach (Device item in devices)
            {
                if (!item.name.Contains("Web Player"))
                {
                    desktopDevices.Add(item);
                }
            }
            return desktopDevices;
        }

        //private async void DeviceLabel_Click(object sender, RoutedEventArgs e)
        //{
           
        //    e.Handled = true;
        //    // DeviceLabel.ContextMenu = await getDeviceContextMenu();
        //    DeviceLabel.ContextMenu = await getDeveviceContext(WebDevices, ComputerDevices);
        //}

        //private async void DeviceLabel_MouseDownAsync(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        Image image = sender as Image;
        //        DeviceLabel.ContextMenu = await getDeviceContextMenu();
        //        e.Handled = true;
        //    }
        //    // DeviceLabel.ContextMenu = await getDeviceContextMenu();
        //}

        private void SeperatorContent()
        {
            if (isConnected)
            {
                Seperator1.Visibility = Visibility.Visible;
                Seperator2.Visibility = Visibility.Visible;
            }
            else
            {
                Seperator1.Visibility = Visibility.Hidden;
                Seperator2.Visibility = Visibility.Hidden;
            }
        }
        private void GenerateAIContent()
        {

            if (isConnected)
            {
                //chcek if AI playlits is present or not
                //if not
                GeneratePlaylistLabel.Content = "Generate my AI playlist";
                GeneratePlaylistImage.Source = new BitmapImage(new Uri("Resources/settings.png", UriKind.Relative));
                // if yes

            }
            else
            {
                GeneratePlaylistLabel.Content = null;
                GeneratePlaylistImage.Source = null;
            }


        }
        private void RefreshAIContent()
        {
            if (isConnected)
            {
                GeneratePlaylistLabel.Content = "Refresh my AI playlist";
                GeneratePlaylistImage.Source = new BitmapImage(new Uri("Resources/settings.png", UriKind.Relative));
            }
            else
            {
                GeneratePlaylistLabel.Content = null;
                GeneratePlaylistImage.Source = null;
            }
        }
        private void SetSortContent()
        {
            try
            {
                if (isConnected)
                {
                    SortDock.Visibility = Visibility.Visible;

                }
                else
                {
                    SortDock.Visibility = Visibility.Hidden;
                }

            }
            catch (Exception e)
            {


            }
        }
        private async void SetGenerateAIContent()
        {
            if(AIPlaylistID==null)
            GenerateAIContent();
            await IsAIPlaylistgeneratedAsync();

            if (!AIPlyalistGenerated)
            {
                GenerateAIContent();
                if(!isAIPlaylistUpdated)
                AIPlaylistAsync();
            }
            else
            {
                RefreshAIContent();
                if (!isAIPlaylistUpdated)
                    AIPlaylistAsync();
            }
        }

        

    

        //AI Playlists Functions
       
        public async Task RefreshMyAIPlaylist()
        {
            Logger.Debug("RefreshMyAIPlaylist");
            //Added Songs to AI playlist From Backend
            await MusicManager.RefreshSongsToPlaylistAsync(AIPlaylistID);
            isAIPlaylistUpdated = false;
            UpdateTreeviewAsync();
        }

        private async Task IsAIPlaylistgeneratedAsync()
        {

            try
            {
                AIPlaylistID = await MusicManager.FetchSavedPlayListAsync();

                if (AIPlaylistID != null)
                {
                    AIPlaylistItem      = await reconcilePlaylistsAsync(AIPlaylistID);

                    if(AIPlaylistItem != null)
                    {
                       AIPlyalistGenerated = true;
                    }
                    else
                    {
                      AIPlyalistGenerated = false;
                      isAIPlaylistUpdated = false;
                    }
                }
                else
                {
                 
                        AIPlyalistGenerated = false;
                       
                    
                }

            }
            catch (Exception ex)
            {


            }


        }

        private async Task<PlaylistItem> reconcilePlaylistsAsync(string AIPlaylistID)
        {
            List<PlaylistItem> playlistItems    = await Playlist.getPlaylistsAsync();
            PlaylistItem AIplaylist             = null;
        
            AIplaylist = playlistItems.FirstOrDefault(x => x.id == AIPlaylistID);

            if (playlistItems.Count>0 && (AIplaylist == null && AIPlaylistID != null))
            {
                await SoftwareHttpManager.SendRequestDeleteAsync("/music/playlist/generated/" + AIPlaylistID);
                Logger.Debug("deleted");
                AIPlaylistID = null;
            }

            return AIplaylist;
        }
        

        private async Task LikedSongsPlaylistAsync()
        {
            try
            {
                TreeViewItem treeItem = null;
                if (isConnected)
                {
                    
               
                    List<Track> LikedTracks             = new List<Track>();
                   // LikedTracks                         = await Playlist.getSpotifyLikedSongsAsync();
                    treeItem                            = PlaylistTreeviewUtil.GetTreeView("Liked Songs", "Heart_Red.png", "Liked Songs");
                    treeItem.MouseLeftButtonUp          += PlayPlaylist;
                    treeItem.Expanded                   += AddTracksAsync;

                    treeItem.Items.Add(null);
                   
                        if (LikePlaylistTV.Items.Count > 0)
                        {
                            LikePlaylistTV.Items.Clear();
                        }

                        LikePlaylistTV.Items.Add(treeItem);
                        isMusicTimePlaylistUpdated = true;
                  
                }
                else
                {
                    LikePlaylistTV.Items.Clear();
                    
                }
            }
            catch (Exception e)
            {


            }


        }
        private  async Task SoftwareTop40PlaylistAsync()
        {
            try
            {
                TreeViewItem SwtopTreeItem = null;
                TreeViewItem LikedTreeItem = null;
                if (isConnected)
                {
                    List<PlaylistItem> playlistItems      = await Playlist.getPlaylistsAsync();
                    List<Track> Swtoptracks               = new List<Track>();

                    SwtopTreeItem  = PlaylistTreeviewUtil.GetTreeView("Software top 40", "PAW.png", Constants.SOFTWARE_TOP_40_ID);
                    LikedTreeItem  = PlaylistTreeviewUtil.GetTreeView("Liked Songs", "Heart_Red.png", "Liked Songs");

                    SwtopTreeItem.MouseLeftButtonUp     += PlayPlaylist;
                    SwtopTreeItem.Expanded              += AddTracksAsync;
                    LikedTreeItem.MouseLeftButtonUp     += PlayPlaylist;
                    LikedTreeItem.Expanded              += AddTracksAsync;

                    LikedTreeItem.Items.Add(null);

                    SwtopTreeItem.Items.Add(null);
                    
                    if (SoftwarePlaylistTV.Items.Count > 0)
                    {
                      SoftwarePlaylistTV.Items.Clear();
                    }

                    SoftwarePlaylistTV.Items.Add(SwtopTreeItem);
                    SoftwarePlaylistTV.Items.Add(LikedTreeItem);
                    isMusicTimePlaylistUpdated = true;
                    
                }
                else
                {
                    SoftwarePlaylistTV.Items.Clear();
                    
                }
            }
            catch (Exception e)
            {


            }


        }
        private async Task UsersPlaylistAsync()
        {
            try
            {
                if (isConnected)
                {
                    List<PlaylistItem> playlistItems    = null;
                    List<TreeViewItem> treeItemList     = new List<TreeViewItem>();
                    playlistItems                       = await Playlist.getPlaylistsAsync();
                 
                    SortPlaylist(ref playlistItems);
 
                    List<Track> tracks                  = new List<Track>();
                    
                    foreach (PlaylistItem playlists in playlistItems)
                    {
                      
                        if (((!string.IsNullOrEmpty(AIPlaylistID) && playlists.id == AIPlaylistID))
                            || playlists.name == Constants.PERSONAL_TOP_SONGS_NAME)
                        { continue; }
                        if(playlists.id == Constants.SOFTWARE_TOP_40_ID )
                        {
                            continue;
                        }
                        
                        TreeViewItem treeItem           = null;
                        treeItem                        = PlaylistTreeviewUtil.GetTreeView(playlists.name, "spotify.png", playlists.id);
                        treeItem.MouseLeftButtonUp      += PlayPlaylist;
                        treeItem.Expanded               += AddTracksAsync;
                        treeItem.Items.Add(null);
                       
                        treeItemList.Add(treeItem);

                    }
                    if (UsersPlaylistTV.Items.Count > 0)
                    {
                        UsersPlaylistTV.Items.Clear();
                    }
                    foreach (TreeViewItem item in treeItemList)
                    {
                        UsersPlaylistTV.Items.Add(item);
                    }


                    isUsersPlaylistUpdated = true;

                }
                else
                {
                    UsersPlaylistTV.Items.Clear();

                }
            }
            catch (Exception e)
            {


            }

        }
        private async Task AIPlaylistAsync()
        {

            try
            {
                TreeViewItem treeItem = null;

                if (isConnected)
                {
                 
                    List<TreeViewItem> treeItemList = new List<TreeViewItem>();

                    if (AIPlaylistItem == null)
                    {
                        AIPlaylistTV.Items.Clear();
                        return;
                    }

                    treeItem = PlaylistTreeviewUtil.GetTreeView(AIPlaylistItem.name, "PAW.png", AIPlaylistItem.id);

                    treeItem.MouseLeftButtonUp  += PlayPlaylist;
                    treeItem.Expanded           += AddTracksAsync;
                    treeItem.Items.Add(null);

                    treeItemList.Add(treeItem);

                    if (AIPlaylistTV.Items.Count > 0)
                    {
                        AIPlaylistTV.Items.Clear();
                    }

                    AIPlaylistTV.Items.Add(treeItem);
                    isAIPlaylistUpdated = true;
                }
                else
                {
                    AIPlaylistTV.Items.Clear();

                }


            }
            catch (Exception e)
            {


            }

        }

        private async void AddTracksAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                bool isLikedSongs = false;
                List<Track> tracks          = new List<Track>();
                PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;
                item.Items.Clear();
              
                if(item.PlayListId == "Liked Songs")
                {
                    tracks = await Playlist.getSpotifyLikedSongsAsync();
                    isLikedSongs = true;
                }
                else
                tracks = await Playlist.getPlaylistTracksAsync(item.PlayListId);

                if (tracks.Count<1)
                {
                    TreeViewItem treeviewItem = PlaylistTreeviewUtil.GetTreeView("Your tracks will appear here", null, "EmptyPlaylist");
                    item.Items.Add(treeviewItem);
                }

                foreach (Track items in tracks)
                {
                    TreeViewItem playlistTreeviewItem = PlaylistTreeviewUtil.GetTrackTreeView(items.name, "share.png", items.id);

                    if(isLikedSongs)
                        playlistTreeviewItem.MouseLeftButtonUp += PlayLikedSongs;
                    else
                        playlistTreeviewItem.MouseLeftButtonUp += PlaySelectedSongAsync;

                    playlistTreeviewItem.MouseRightButtonDown += PlaylistTreeviewItem_MouseRightButtonDownAsync;
                   
                    item.Items.Add(playlistTreeviewItem);
                }
            }
            catch (Exception ex)
            {

                
            }
           
        }

        private async void PlaylistTreeviewItem_MouseRightButtonDownAsync(object sender, MouseButtonEventArgs e)
        {
            PlaylistTreeviewItem item = sender as PlaylistTreeviewItem;
            item.ContextMenu = await GetContextMenuAsync(item.PlayListId);

        }

        private static async Task<ContextMenu> getDeviceContextMenu()
        {
            ContextMenu contextMenu = new ContextMenu();
            CustomMenu webPlayerMenu = new CustomMenu();
            webPlayerMenu.Header = "Launch Web Player";

            webPlayerMenu.Foreground = System.Windows.Media.Brushes.DarkCyan;
            webPlayerMenu.Click += launchWebAndPlayTrack; ;

            CustomMenu desktoPlayerMenu = new CustomMenu();
            desktoPlayerMenu.Header = "Launch Desktop Player";

            desktoPlayerMenu.Foreground = System.Windows.Media.Brushes.DarkCyan;
            desktoPlayerMenu.Click += launchDesktopAndPlayTrack; ;

            contextMenu.Items.Add(webPlayerMenu);
            contextMenu.Items.Add(desktoPlayerMenu);
            contextMenu.IsOpen = true;
            return contextMenu;
        }


        private  async Task<ContextMenu> getDeveviceContext(List<Device> WebDevices ,List<Device> ComputerDevices, string playlist_id,string track_id)
        {
            ContextMenu contextMenu = new ContextMenu();

            if (WebDevices.Count > 0)
            {
                foreach (Device item in WebDevices)
                {
                    DeviceContextMenu webPlayerMenu = null;
                    if (item.is_active == true)
                    {
                       webPlayerMenu = new DeviceContextMenu();
                        webPlayerMenu.Header = "Listening on "+item.name;
                        webPlayerMenu.isActive = item.is_active;
                        webPlayerMenu.deviceId = item.id;
                        webPlayerMenu.Foreground = System.Windows.Media.Brushes.DarkCyan;
                        webPlayerMenu.Click += WebPlayerPlay; 
                    }
                    else
                    {
                        webPlayerMenu = new DeviceContextMenu();
                        webPlayerMenu.Header = "Available on " + item.name;
                        webPlayerMenu.isActive = item.is_active;
                        webPlayerMenu.deviceId = item.id;
                        webPlayerMenu.Foreground = System.Windows.Media.Brushes.DarkCyan;
                        webPlayerMenu.Click += transferPlayer;

                    }
                    contextMenu.Items.Add(webPlayerMenu);
                }
            }
            else
            {
                DeviceContextMenu webPlayerMenu     = new DeviceContextMenu();
                webPlayerMenu.Header                = "Launch Web Player";
                webPlayerMenu.playlist_id           = playlist_id;
                webPlayerMenu.track_id              = track_id;
                webPlayerMenu.Foreground            = System.Windows.Media.Brushes.DarkCyan;
                webPlayerMenu.Click                 += launchWebAndPlayTrack;

                contextMenu.Items.Add(webPlayerMenu);
            }
            if (ComputerDevices.Count > 0)
            {
                foreach (Device item in ComputerDevices)
                {
                    DeviceContextMenu computerMenu = null;
                    if (item.is_active == true)
                    {
                        computerMenu                = new DeviceContextMenu();
                        computerMenu.Header         = "Listening on " + item.name;
                        computerMenu.isActive       = item.is_active;
                        computerMenu.deviceId       = item.id;
                        computerMenu.Foreground     = System.Windows.Media.Brushes.DarkCyan;
                        computerMenu.Click          += WebPlayerPlay;
                    }
                    else
                    {
                        computerMenu            = new DeviceContextMenu();
                        computerMenu.Header     = "Available on " + item.name;
                        computerMenu.isActive   = item.is_active;
                        computerMenu.deviceId   = item.id;
                        computerMenu.Foreground = System.Windows.Media.Brushes.DarkCyan;
                        computerMenu.Click      += transferPlayer;

                    }
                    contextMenu.Items.Add(computerMenu);
                }
            }
            else
            {
                DeviceContextMenu desktoPlayerMenu  = new DeviceContextMenu();
                desktoPlayerMenu.Header             = "Launch Desktop Player";

                desktoPlayerMenu.Foreground         = System.Windows.Media.Brushes.DarkCyan;
                desktoPlayerMenu.Click              += launchDesktopAndPlayTrack;
                contextMenu.Items.Add(desktoPlayerMenu);
            }
            
            contextMenu.IsOpen = true;
                return contextMenu;
        }

        private void transferPlayer(object sender, RoutedEventArgs e)
        {
            DeviceContextMenu deviceContextMenu = sender as DeviceContextMenu;
            MusicManager.SpotifyTransferDevice(deviceContextMenu.deviceId);
        }

        private void WebPlayerPlay(object sender, RoutedEventArgs e)
        {
            
        }

        private static async void launchDesktopAndPlayTrack(object sender, RoutedEventArgs e)
        {
            DeviceContextMenu deviceContextMenu = sender as DeviceContextMenu;

            await MusicController.LaunchDesktopApp();
            Thread.Sleep(5000);
           // Logger.Debug(deviceContextMenu.playlist_id);
            await MusicManager.getDevicesAsync();
            if (!string.IsNullOrEmpty(deviceContextMenu.playlist_id) || !string.IsNullOrEmpty(deviceContextMenu.track_id))
            {
                Logger.Debug(deviceContextMenu.playlist_id);
                await MusicManager.SpotifyPlayPlaylistAsync(deviceContextMenu.playlist_id, deviceContextMenu.track_id);
            }
           
        }

        private static async void launchWebAndPlayTrack(object sender, RoutedEventArgs e)
        {
            DeviceContextMenu deviceContextMenu = sender as DeviceContextMenu;

            await MusicController.LaunchPlayerAsync(new options(null, null, null));
            await MusicManager.getDevicesAsync();
            Thread.Sleep(5000);
            if (!string.IsNullOrEmpty(deviceContextMenu.playlist_id) || !string.IsNullOrEmpty(deviceContextMenu.track_id))
            {
                Logger.Debug(deviceContextMenu.playlist_id);
                await MusicManager.SpotifyPlayPlaylistAsync(deviceContextMenu.playlist_id, deviceContextMenu.track_id);
            }
        }

        private static async Task<ContextMenu> GetContextMenuAsync(string playlist_Id)
        {
            ContextMenu contextMenu = new ContextMenu();

            CustomMenu addMenu      = new CustomMenu();
            addMenu.Header          = "Add Song";
            addMenu.PlaylistId      = playlist_Id;
            addMenu.Foreground      = System.Windows.Media.Brushes.DarkCyan;
            addMenu.Click           += AddMenu_Click;

            CustomMenu removeMenu   = new CustomMenu();
            removeMenu.Header       = "Remove Song";
            removeMenu.PlaylistId   = playlist_Id;
            removeMenu.Foreground   = System.Windows.Media.Brushes.DarkCyan;
            removeMenu.Click        += RemoveMenu_Click;

            CustomMenu copyToClipBoardMenu = new CustomMenu();
            copyToClipBoardMenu.Header = "Copy Song Link";
            copyToClipBoardMenu.PlaylistId = playlist_Id;
            copyToClipBoardMenu.Foreground = System.Windows.Media.Brushes.DarkCyan;
            copyToClipBoardMenu.Click += CopyToClipBoardMenu_Click;


            CustomMenu facebookMenu     = new CustomMenu();
            facebookMenu.Header         = "Facebook";
            facebookMenu.PlaylistId     = playlist_Id;
            facebookMenu.Foreground     = System.Windows.Media.Brushes.DarkCyan;
            facebookMenu.Click += FacebookMenu_Click;

            CustomMenu twitterMenu      = new CustomMenu();
            twitterMenu.Header          = "Twitter";
            twitterMenu.PlaylistId      = playlist_Id;
            twitterMenu.Foreground      = System.Windows.Media.Brushes.DarkCyan;
            twitterMenu.Click           += TwitterMenu_Click; ;

            CustomMenu whatsAppMenu     = new CustomMenu();
            whatsAppMenu.Header         = "WhatsApp";
            whatsAppMenu.PlaylistId     = playlist_Id;
            whatsAppMenu.Foreground     = System.Windows.Media.Brushes.DarkCyan;
            whatsAppMenu.Click += WhatsAppMenu_Click; ;

            CustomMenu tumblerMenu      = new CustomMenu();
            tumblerMenu.Header          = "Tumbler";
            tumblerMenu.PlaylistId      = playlist_Id;
            tumblerMenu.Foreground      = System.Windows.Media.Brushes.DarkCyan;
            tumblerMenu.Click += TumblerMenu_Click;

            CustomMenu shareMenuItem    = new CustomMenu();
            shareMenuItem.Header        = "Share Song";
            shareMenuItem.Foreground    = System.Windows.Media.Brushes.DarkCyan;

            CustomMenu slackMenuItem =  await GetSlackMenuItemAsync(playlist_Id);

            shareMenuItem.Items.Add(facebookMenu);
            shareMenuItem.Items.Add(twitterMenu);
            shareMenuItem.Items.Add(whatsAppMenu);
            shareMenuItem.Items.Add(tumblerMenu);
            shareMenuItem.Items.Add(slackMenuItem);

            contextMenu.Items.Add(addMenu);
            contextMenu.Items.Add(removeMenu);
            contextMenu.Items.Add(copyToClipBoardMenu);
            contextMenu.Items.Add(shareMenuItem);
         
            return contextMenu;
        }

        private static void CopyToClipBoardMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;

                if(item!=null)
                    SlackControlManager.CopylinkToClipboard(item.PlaylistId);
            }
            catch (Exception ex)
            {

               
            }
         

        }

        private static void AddMenu_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private static void RemoveMenu_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private static void FacebookMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;

                if(item!=null)
                    SlackControlManager.ShareOnFacebook(item.PlaylistId);
            }
            catch (Exception ex)
            {

               
            }
        
        }

        private static void TumblerMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;
                if(item!=null)
                    SlackControlManager.ShareOnTumbler(item.PlaylistId);
            }
            catch (Exception ex)
            {

                
            }
           
        }

        private static void WhatsAppMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;
                if (item != null)
                    SlackControlManager.ShareOnWhatsApp(item.PlaylistId);
            }
            catch ( Exception ex)
            {

                
            }
         
        }

        private static void TwitterMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;
                
                if(item!=null)
                SlackControlManager.ShareOnTwitter(item.PlaylistId);

            }
            catch (Exception ex)
            {

                
            }
          
        }

        private static async Task<CustomMenu> GetSlackMenuItemAsync(string playlist_id)
        {
            CustomMenu SlackMenuItem = new CustomMenu();
            SlackMenuItem.Foreground = System.Windows.Media.Brushes.DarkCyan;

          
            if (MusicTimeCoPackage.slackConnected)
            {
                SlackMenuItem.Header = "Slack";
              
                foreach (Channel item in MusicTimeCoPackage.SlackChannels)
                {
                    CustomMenu menuItem = new CustomMenu();
                    menuItem.Header = item.Name;
                    menuItem.PlaylistId = playlist_id;
                    menuItem.SlackChannelId = item.Id;
                    menuItem.Foreground = System.Windows.Media.Brushes.DarkCyan;
                    menuItem.Click += ShareOnSlackChannel; ;
                    SlackMenuItem.Items.Add(menuItem);
                    
                }

            }
            else
            {
                SlackMenuItem.Header = "Connect Slack";
                SlackMenuItem.Click += SlackMenuItem_Click;
            }
            

          
            

          

            return SlackMenuItem;
        }

        private static void ShareOnSlackChannel(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;

                if(item!=null)
                SlackControlManager.ShareOnSlackChannel(item.SlackChannelId, item.PlaylistId);
            }
            catch (Exception ex)
            {

                
            }
          
        }

        private static void SlackMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SlackControlManager.ConnectToSlackAsync();
        }
        private void ShareOnFacebook(object sender, RoutedEventArgs e)
        {
        }




        private void SortPlaylist(ref List<PlaylistItem> playlistItems)
        {
           
            if (SortPlaylistFlag == true && playlistItems.Count > 0)
            {
                playlistItems = playlistItems.OrderBy(x => x.name).ToList();
            }
        }


        /// <summary>
        /// Functions To Play Selected Songs / Playlist /LikedSongs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PlayPlaylist(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string playlistID       = string.Empty;
                string trackID          = string.Empty;
                options options         = new options();

                PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;
                playlistID                  = item.PlayListId;
                

                if (MusicManager.isDeviceOpened())
                {
                    await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
                }
                else
                {
                    options.playlist_id = playlistID;
                    await MusicController.LaunchPlayerAsync(options);
                    
                   // await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
                }

            }
            catch (Exception ex)
            {


            }
        }

        private async void PlaySelectedSongAsync(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled           = true;       
                string playlistID   = string.Empty;
                string trackID      = string.Empty;

                PlaylistTreeviewItem parent = null;
                PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;
                parent                      = PlaylistTreeviewUtil.GetSelectedTreeViewItemParent(item);
                if(parent!= null)
                {
                    playlistID  = parent.PlayListId;
                    trackID     = item.PlayListId;
                  //  Logger.Debug(playlistID +":" + trackID);
                }
                else
                {
                    playlistID = item.PlayListId;
                    
                }

                
                    if (MusicManager.isDeviceOpened())
                    {
                        await MusicManager.SpotifyPlayPlaylistAsync(playlistID,trackID);
                    }
                    else
                    {

                    //  DeviceLabel_ClickAsync(null, null);
                    PlayTrackFromContext(playlistID, trackID);

                   // await MusicController.LaunchPlayerAsync(new options(null,playlistID,trackID));

                    // await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
                }
                
            }
            catch (Exception ex)
            {

                
            }
           
        }

        private async void PlayLikedSongs(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string playlistID       = string.Empty;
                string trackID          = string.Empty;

                options options             = new options();
                PlaylistTreeviewItem parent = null;
                PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;

                parent = PlaylistTreeviewUtil.GetSelectedTreeViewItemParent(item);

                if (parent != null)
                {
                    playlistID  = null;
                    trackID     = item.PlayListId;

                }
                else
                {
                    playlistID = item.PlayListId;

                }


                if (MusicManager.isDeviceOpened())
                {
                    await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
                }
                else
                {
                   
                    await MusicController.LaunchPlayerAsync(new options(null,playlistID,trackID));
                    
                }

            }
            catch (Exception ex)
            {


            }
        }

    

        private void WebAnaylticsClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                isConnected = MusicManager.hasSpotifyPlaybackAccess();
                if (isConnected)
                {
                    string url = "https://app.software.com/music";
                    SoftwareSpotifyManager.launchWebUrl(url);
                }
            }
            catch (Exception ex)
            {

               
            }
        }

        private void Az_Sort(object sender, RoutedEventArgs e)
        {
            SortPlaylistFlag        = true;
            isUsersPlaylistUpdated  = false;
            UpdateTreeviewAsync();
        }

        private void Latest_Sort(object sender, RoutedEventArgs e)
        {
            SortPlaylistFlag        = false;
            isUsersPlaylistUpdated  = false;
            UpdateTreeviewAsync();
        }
      
        private async void GenerateAIAsync(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
           // GeneratePlaylistLabel.Click -= GenerateAIAsync;
            GeneratePlaylistLabel.IsEnabled = false;
            if (!AIPlyalistGenerated)
            {
                Logger.Debug(" AIPlyalistGenerated");
                Playlist.PlayListID = await Playlist.generateMyAIPlaylistAsync();

                if (Playlist.PlayListID != null)
                {
                    //Saved PlaylistId to Backend
                    await MusicManager.UpdateSavedPlaylistsAsync(Playlist.PlayListID, Constants.PERSONAL_TOP_SONGS_PLID,
                         Constants.PERSONAL_TOP_SONGS_NAME);

                    //Added Songs to AI playlist From Backend
                    await MusicManager.SeedSongsToPlaylistAsync(Playlist.PlayListID);
                    isAIPlaylistUpdated = false;
                    UpdateTreeviewAsync();
                }

            }
            else
            {
               
                if (AIPlaylistID != null)
                   await RefreshMyAIPlaylist();
            }
            GeneratePlaylistLabel.IsEnabled = true;
            //GeneratePlaylistLabel.Click += GenerateAIAsync;
        }



      
    }
}