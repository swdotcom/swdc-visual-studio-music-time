namespace MusicTime
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Interaction logic for SpotifyPlayListControl.
    /// </summary>
    public partial class SpotifyPlayListControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpotifyPlayListControl"/> class.
        /// </summary>
        private static bool isConnected     = false;
        private static Playlist _Playlist   = Playlist.getInstance;
        private static int ONE_SECOND       = 1000;
        private TreeViewItem SoftwareTop40treeItem  = null;
        private TreeViewItem LikedSongtreeItem      = null;
    
        public static Boolean isPlaylistUpdated     = false;
        public SpotifyPlayListControl()
        {
            this.InitializeComponent();
            Init();
        }

        private void Init()
        {
            SetConnectContent();
            System.Windows.Forms.Timer timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 5000;//5 seconds
            timer1.Tick += new System.EventHandler(UpdateCallBack);
            timer1.Start();

        }
        private void UpdateCallBack(object sender, EventArgs e)
        {
            UpdateTreeviewAsync();
        }
       
        //checks user is connected or not ,sets static boolean variable 
        private async Task CheckUserStatusAsync()
        {
            bool online = MusicTimeCoPackage.isOnline;
            SoftwareUserSession.UserStatus status = await SoftwareUserSession.GetSpotifyUserStatusTokenAsync(online);
            isConnected = status.loggedIn;
            
        }

        
        private async void RefreshAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                LikedSongsPlaylist();
                SoftwareTop40Playlist();
                UsersPlaylist();
                Logger.Debug("refrsh");
            }
            catch (Exception ex)
            {

                
            }
           
        }

        public async void UpdateTreeviewAsync()
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
                    GenerateAIContent();
                    
                    if (!isPlaylistUpdated)
                    {
                        LikedSongsPlaylist();
                        SoftwareTop40Playlist();
                        UsersPlaylist();
                        Logger.Debug("Update");
                    }
                        
                }
                else
                {
                    clearAll();
                }
            }
            catch (Exception ex)
            {

                
            }
           
        }
       
        private void clearAll()
        {
            try
            {
                SetConnectContent();
                SetWebAnalyticsContent();
                SetDeviceDetectionContent();
                SeperatorContent();
                GenerateAIContent();
                LikedSongsPlaylist();
                SoftwareTop40Playlist();
                UsersPlaylist();
                isPlaylistUpdated = false;
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
                    ConnectLabel.Content    = "Connect to spotify";
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
                    DeviceImage.Source  = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                    List<Device> devices = null;
                    if(MusicManager.getDevices()!= null )
                    {
                        devices = MusicManager.getDevices();
                        
                        if(devices.Count>1)
                        {
                            DeviceLabel.Content = "Connected on " + MusicManager.getDeviceNames();
                        }
                        else
                        {
                            DeviceLabel.Content = "Listening on " + MusicManager.getActiveDeviceName();
                        }

                    }
                    else
                    {
                        DeviceLabel.Content = "Device is not detected";
                    }

                   
                }
                else
                {
                    DeviceImage.Source  = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                    DeviceLabel.Content = "Device is not detected";
                }
            }
            else
            {
                DeviceLabel.Content = null;
                DeviceImage.Source = null;
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
                GeneratePlaylistLabel.Content   = null;
                GeneratePlaylistImage.Source    = null;
            }


        }
        private void RefreshAIContent()
        {
            GeneratePlaylistLabel.Content = "Refresh MY AI Playlist";
            GeneratePlaylistImage.Source = new BitmapImage(new Uri("Resources/settings.png", UriKind.Relative));
        }

 
        //AI Playlists Functions
        public async void RefreshAIPLaylist()
        {

        }

        private void GenerateAIPLaylist(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Playlist.generateMyAIPlaylistAsync();
        }
       
        private TreeViewItem getSoftwareTopTree()
        {
            if (SoftwareTop40treeItem == null)
            {
                SoftwareTop40treeItem = GetTreeView("Software top 40", "PAW.png", Constants.SOFTWARE_TOP_40_ID);
            }
            
            return SoftwareTop40treeItem;
        }
        private TreeViewItem getLikedTree()
        {
            if (LikedSongtreeItem == null)
            {
                LikedSongtreeItem = GetTreeView("LikedSongs", "spotify.png", null);
            }

            return LikedSongtreeItem;
        }
       
        private async void LikedSongsPlaylistold()
        {
            try
            {
                if (isConnected)
                {
                    if (SoftwarePlaylistTV.Items.Count > 0)
                    {
                        SoftwarePlaylistTV.Items.Clear();
                    }

                    getLikedTree();

                    if (LikedSongtreeItem.Items.Count > 0)
                    {
                        LikedSongtreeItem.Items.Clear();
                    }
                    if (Playlist.Liked_Playlist != null && Playlist.Liked_Playlist.Count > 0)
                    {

                        foreach (Track item in Playlist.Liked_Playlist)
                        {
                            TreeViewItem playlistTreeviewItem = GetTrackTreeView(item.name, "share.png", item.id);
                            playlistTreeviewItem.MouseDoubleClick += PlaySelectedSongAsync;
                            LikedSongtreeItem.Items.Add(playlistTreeviewItem);
                        }
                        SoftwarePlaylistTV.Items.Add(LikedSongtreeItem);
                        isPlaylistUpdated = true;
                    }
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

        private async void SoftwareTop40Playlistold()
        {
            try
            {
                if (isConnected)
                {
                    getSoftwareTopTree();
                    if (SoftwareTop40treeItem.Items.Count > 0)
                    {
                        SoftwareTop40treeItem.Items.Clear();
                    }
                    if (Playlist.Software_Playlists != null && Playlist.Software_Playlists.Count > 0)
                    {
                        foreach (Track item in Playlist.Software_Playlists)
                        {
                            TreeViewItem playlistTreeviewItem = GetTrackTreeView(item.name, "share.png", item.id);

                            playlistTreeviewItem.MouseDoubleClick += PlaySelectedSongAsync;

                            SoftwareTop40treeItem.Items.Add(playlistTreeviewItem);

                        }

                        SoftwarePlaylistTV.Items.Add(SoftwareTop40treeItem);
                        isPlaylistUpdated = true;
                    }
                }
                else
                {
                    SoftwarePlaylistTV.Items.Clear();
                    SoftwareTop40treeItem = null;
                }
            }
            catch (Exception e)
            {

                
            }           

          
        }
        private async void LikedSongsPlaylist()
        {
            try
            {
                TreeViewItem treeItem = null;
                if (isConnected)
                {
                    if (SoftwarePlaylistTV.Items.Count > 0)
                    {
                        SoftwarePlaylistTV.Items.Clear();
                    }

                    List<PlaylistItem> playlistItems = await Playlist.getPlaylistsAsync();
                    List<Track> LikedTracks = new List<Track>();

                    LikedTracks = await Playlist.getSpotifyLikedSongsAsync();
                    treeItem = GetTreeView("LikedSongs", "spotify.png", null);

                    if (LikedTracks.Count > 0)
                    {
                        foreach (Track item in LikedTracks)
                        {
                            TreeViewItem playlistTreeviewItem = GetTrackTreeView(item.name, "share.png", item.id);

                            playlistTreeviewItem.MouseDoubleClick += PlaySelectedSongAsync;

                            treeItem.Items.Add(playlistTreeviewItem);

                        }

                        SoftwarePlaylistTV.Items.Add(treeItem);
                        isPlaylistUpdated = true;
                    }
                }
                else
                {
                    SoftwarePlaylistTV.Items.Clear();
                    SoftwareTop40treeItem = null;
                }
            }
            catch (Exception e)
            {


            }


        }

        private async void SoftwareTop40Playlist()
        {
            try
            {
                TreeViewItem treeItem = null;
                if (isConnected)
                {
                    List<PlaylistItem> playlistItems      = await Playlist.getPlaylistsAsync();
                    List<Track> Swtoptracks               = new List<Track>();

                    Swtoptracks = await Playlist.getPlaylistTracksAsync(Constants.SOFTWARE_TOP_40_ID);
                    treeItem = GetTreeView("Software top 40", "PAW.png", Constants.SOFTWARE_TOP_40_ID);
                   
                    if (Swtoptracks.Count > 0)
                    {
                        foreach (Track item in Swtoptracks)
                        {
                            TreeViewItem playlistTreeviewItem = GetTrackTreeView(item.name, "share.png", item.id);

                            playlistTreeviewItem.MouseDoubleClick += PlaySelectedSongAsync;

                            treeItem.Items.Add(playlistTreeviewItem);

                        }

                        SoftwarePlaylistTV.Items.Add(treeItem);
                        isPlaylistUpdated = true;
                    }
                }
                else
                {
                    SoftwarePlaylistTV.Items.Clear();
                    SoftwareTop40treeItem = null;
                }
            }
            catch (Exception e)
            {


            }


        }
        private async void PlaySelectedSongAsync(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //PlaylistTreeviewItem item = sender as PlaylistTreeviewItem;
            
            //PlaylistTreeviewItem parent       = GetSelectedTreeViewItemParent(item);
            //if (MusicManager.isDeviceOpened())
            //{
            //    await MusicManager.SpotifyPlayPlaylist(parent.PlayListId, item.PlayListId);
            //}
            //else
            //{
            //    await MusicController.LaunchPlayerAsync();

            //    await MusicManager.getDevicesAsync();
            //}
        }

       
        private async void UsersPlaylist()
        {
            try
            {

               

                if (isConnected)
                {
                    List<PlaylistItem> playlistItems    = await Playlist.getPlaylistsAsync();
                    List<Track> tracks                  = new List<Track>();

                    Logger.Debug(playlistItems.Count.ToString());
                    if (UsersPlaylistTV.Items.Count > 0)
                    {
                        UsersPlaylistTV.Items.Clear();
                    }

                    foreach (PlaylistItem playlists in playlistItems)
                    {
                        TreeViewItem treeItem = null;
                        treeItem = GetTreeView(playlists.name, "spotify.png", playlists.id);

                        tracks = await Playlist.getPlaylistTracksAsync(playlists.id);

                        foreach (Track item in tracks)
                        {
                            TreeViewItem playlistTreeviewItem = GetTrackTreeView(item.name, "share.png", item.id);

                            playlistTreeviewItem.MouseDoubleClick += PlaySelectedSongAsync;

                            treeItem.Items.Add(playlistTreeviewItem);
                        }
                        UsersPlaylistTV.Items.Add(treeItem);
                        Logger.Debug("up");
                        isPlaylistUpdated = true;

                    }
                  
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
        private async void UsersPlaylist2()
        {

            TreeViewItem treeItem = null;

            if (isConnected)
            {
                PlaylistItem playlistItems = null;
                List<Track> tracks = null;


                if (UsersPlaylistTV.Items.Count > 0)
                {
                    UsersPlaylistTV.Items.Clear();
                }

                foreach (KeyValuePair<PlaylistItem, List<Track>> entry in Playlist.Users_Playlist)
                {
                    playlistItems   = entry.Key;
                    tracks          = entry.Value;
                    treeItem        = GetTreeView(playlistItems.name, "spotify.png", playlistItems.id);

                    foreach (Track item in tracks)
                    {
                        TreeViewItem playlistTreeviewItem = GetTrackTreeView(item.name, "share.png", item.id);

                        playlistTreeviewItem.MouseDoubleClick += PlaySelectedSongAsync;

                        treeItem.Items.Add(playlistTreeviewItem);
                    }

                    UsersPlaylistTV.Items.Add(treeItem);
                }
                
            }
            else
            {
                UsersPlaylistTV.Items.Clear();

            }

        }
        private TreeViewItem GetTreeView(string text, string imagePath,string id)
        {
            PlaylistTreeviewItem item = new PlaylistTreeviewItem(id);
            
            // create stack panel
            StackPanel stack  = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create Image
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = new BitmapImage(new Uri("Resources/"+imagePath, UriKind.Relative));

            // Label
            Label lbl   = new Label();
            lbl.Content = text;
            lbl.Foreground = System.Windows.Media.Brushes.DarkCyan;

            // Add into stack
            stack.Children.Add(image);
            stack.Children.Add(lbl);

            // assign stack to header
            item.Header = stack;
            return item;
        }

        private TreeViewItem GetTrackTreeView(string text, string imagePath, string id)
        {
            PlaylistTreeviewItem item = new PlaylistTreeviewItem(id);

            // create stack panel
            StackPanel stack    = new StackPanel();
            stack.Orientation   = Orientation.Horizontal;

            // create Image
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = new BitmapImage(new Uri("Resources/" + imagePath, UriKind.Relative));
            
            // Label
            Label lbl = new Label();
            lbl.Content = ResizeSongName(text);
           // lbl.Content = text;
            lbl.Width   = 150;
            
            lbl.Foreground = System.Windows.Media.Brushes.DarkCyan;

            // Add into stack
            
            stack.Children.Add(lbl);
            stack.Children.Add(image);
            // assign stack to header
            item.Header = stack;
            item.Background = System.Windows.Media.Brushes.Transparent;
            return item;
        }

        private string ResizeSongName(string text)
        {
            string result = string.Empty;
            if (text.Length > 20)
            {
                result = string.Concat(text.Substring(0, 20), "...");
            }
            else
            {
                result = text;
            }
            return result;
        }

        private void ConnectStatusClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isConnected = MusicManager.hasSpotifyPlaybackAccess();
            if (!isConnected)
            {
                 SoftwareSpotifyManager.ConnectToSpotifyAsync();
            }
        }
    }
}