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
                    // await LikedSongsPlaylistAsync();
                    SetGenerateAIContent();
                    await SoftwareTop40PlaylistAsync();
                    //await AIPlaylistAsync();
                    await UsersPlaylistAsync();
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
                    
                    SetConnectContent();
                    SetWebAnalyticsContent();
                    SetDeviceDetectionContent();
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
                UsersPlaylistTV.Items.Clear();
                SoftwarePlaylistTV.Items.Clear();
                LikePlaylistTV.Items.Clear();
                AIPlaylistTV.Items.Clear();
                isUsersPlaylistUpdated  = false;
                isAIPlaylistUpdated     = false;
                isMusicTimePlaylistUpdated = false;
                SetConnectContent();
                SetWebAnalyticsContent();
                SetDeviceDetectionContent();
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

                AnalyticLabel.Content   = "See web Analytics";
                AnalyticImage.Source    = new BitmapImage(new Uri("Resources/PAW.png", UriKind.Relative));
            }
            else
            {
                AnalyticLabel.Content   = null;
                AnalyticImage.Source    = null;
            }

        }
        private void SetDeviceDetectionContent()
        {
            if (isConnected)
            {

                if (MusicManager.isDeviceOpened())
                {
                    DeviceImage.Source   = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                    List<Device> devices = null;

                    
                    devices = MusicManager.getDevices();

                    if(devices.Count>0)
                    {
                       
                        string Active_Device = MusicManager.getActiveDeviceName();

                            if (!string.IsNullOrEmpty(Active_Device))
                            {
                                DeviceLabel.Content     = "Listening on " + MusicManager.getActiveDeviceName();
                                DeviceLabel.ToolTip     = "Listening on a Spotify device";
                            }
                            else
                            {
                                DeviceLabel.Content     = "Connected on " + MusicManager.getDeviceNames();
                            if (devices.Count > 1)
                                DeviceLabel.ToolTip     = "Multiple Spotify devices connected";
                            else
                                DeviceLabel.ToolTip     = "Spotify device connected";
                            }  

                    }
                        
                        
                }
                else
                {

                    DeviceImage.Source      = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                    DeviceLabel.Content     = "No device detected";
                    DeviceLabel.ToolTip     = null;
                }
            }
            else
            {
                DeviceLabel.Content = null;
                DeviceImage.Source  = null;
            }

        }
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
                GeneratePlaylistLabel.Content = "Generate AI Playlist";
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
                GeneratePlaylistLabel.Content = "Refresh My AI Playlist";
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
        private async Task SoftwareTop40PlaylistAsync()
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

                    item.Items.Add(playlistTreeviewItem);
                }
            }
            catch (Exception ex)
            {

                
            }
           
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
                    Logger.Debug(playlistID +":" + trackID);
                }
                else
                {
                    playlistID = item.PlayListId;
                    Logger.Debug(playlistID + ":" + trackID);
                }

                
                    if (MusicManager.isDeviceOpened())
                    {
                        await MusicManager.SpotifyPlayPlaylistAsync(playlistID,trackID);
                    }
                    else
                    {
                     
                        await MusicController.LaunchPlayerAsync(new options(null,playlistID,trackID));
                       
                      //  await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
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

        //private static PlaylistTreeviewItem GetSelectedTreeViewItemParent(PlaylistTreeviewItem item)
        //{
        //    DependencyObject parent = null;
        //    try
        //    {
        //        parent = VisualTreeHelper.GetParent(item);

        //        while (!(parent is PlaylistTreeviewItem))
        //        {
        //            parent = VisualTreeHelper.GetParent(parent);
        //        }


               
        //    }
        //    catch ( Exception ex)
        //    {

                
        //    }
        //    return parent as PlaylistTreeviewItem;

        //}

        //private static TreeViewItem GetTreeView(string text, string imagePath,string id)
        //{
        //    PlaylistTreeviewItem item = new PlaylistTreeviewItem(id);
                       
        //    // create stack panel
        //    StackPanel stack  = new StackPanel();
        //    stack.Orientation = Orientation.Horizontal;

        //    if (!string.IsNullOrEmpty(imagePath))
        //    {
        //        // create Image
        //        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
        //        image.Source = new BitmapImage(new Uri("Resources/" + imagePath, UriKind.Relative));
        //        stack.Children.Add(image);
        //    }
        //    // Label

        //    Label lbl   = new Label();
        //    lbl.Content = text;
        //    lbl.Foreground = System.Windows.Media.Brushes.DarkCyan;

        //    // Add into stack
           
        //    stack.Children.Add(lbl);

        //    // assign stack to header
        //    item.Header = stack;
        //    return item;
        //}

        //private static TreeViewItem GetTrackTreeView(string text, string imagePath, string id)
        //{
        //    PlaylistTreeviewItem item = new PlaylistTreeviewItem(id);

        //    // create stack panel
        //    StackPanel stack    = new StackPanel();
        //    stack.Orientation   = Orientation.Horizontal;

        //    // create Image
        //    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
        //    image.Source = new BitmapImage(new Uri("Resources/" + imagePath, UriKind.Relative));
            
        //    // Label
        //    Label lbl = new Label();
        //    lbl.Content = PlaylistTreeviewUtil.ResizeSongName(text);
        //   // lbl.Content = text;
        //    lbl.Width   = 150;
            
        //    lbl.Foreground = System.Windows.Media.Brushes.DarkCyan;

        //    // Add into stack
            
        //    stack.Children.Add(lbl);
        //    stack.Children.Add(image);
        //    // assign stack to header
        //    item.Header = stack;
        //    item.Background = System.Windows.Media.Brushes.Transparent;
        //    return item;
        //}

        //private static string ResizeSongName(string text)
        //{
        //    string result = string.Empty;
        //    if (text.Length > 20)
        //    {
        //        result = string.Concat(text.Substring(0, 20), "...");
        //    }
        //    else
        //    {
        //        result = text;
        //    }
        //    return result;
        //}

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
        //private async void GenerateAIPlaylist(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    e.Handled = true;

        //    GeneratePlaylistLabel.IsEnabled = false;
        //    if (!AIPlyalistGenerated)
        //    {
        //        Logger.Debug(" AIPlyalistGenerated");
        //        Playlist.PlayListID = await Playlist.generateMyAIPlaylistAsync();

        //        if (Playlist.PlayListID != null)
        //        {
        //            //Saved PlaylistId to Backend
        //            await MusicManager.UpdateSavedPlaylistsAsync(Playlist.PlayListID, Constants.PERSONAL_TOP_SONGS_PLID,
        //                 Constants.PERSONAL_TOP_SONGS_NAME);

        //            //Added Songs to AI playlist From Backend
        //            await MusicManager.SeedSongsToPlaylistAsync(Playlist.PlayListID);
        //            isAIPlaylistUpdated = false;
        //            UpdateTreeviewAsync();
        //        }

        //    }
        //    else
        //    {
        //        Logger.Debug("before RefreshMyAIPlaylist");
        //        if (AIPlaylistID != null)
        //            RefreshMyAIPlaylist();
        //    }
        //    GeneratePlaylistLabel.IsEnabled = true;

        //}
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