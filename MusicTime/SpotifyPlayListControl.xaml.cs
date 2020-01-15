﻿namespace MusicTime
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
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
        private static bool isConnected = false;
        private static Playlist _Playlist = Playlist.getInstance;
        private TreeViewItem SoftwareTop40treeItem      = null;
        private TreeViewItem LikedSongtreeItem          = null;
        public static Boolean isAIPlaylistUpdated       = false;
        public static Boolean isUsersPlaylistUpdated    = false;
        public static List<Track> LikedSongIds          = null;
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


            //System.Windows.Forms.Timer UpdateTreeviewTimer = new System.Windows.Forms.Timer();
            //UpdateTreeviewTimer.Interval = 25000;//5 seconds
            //UpdateTreeviewTimer.Tick += new System.EventHandler(UpdateTreeCallBack);
            //UpdateTreeviewTimer.Start();

        }
        private void UpdateCallBack(object sender, EventArgs e)
        {
            UpdateTreeviewAsync();
        }
        //private void UpdateTreeCallBack(object sender, EventArgs e)
        //{
        //    RefreshAsync(null, null);
        //}

        //checks user is connected or not ,sets static boolean variable 
        private async Task CheckUserStatusAsync()
        {
            bool online = MusicTimeCoPackage.isOnline;
            SoftwareUserSession.UserStatus status = await SoftwareUserSession.GetSpotifyUserStatusTokenAsync(online);
            isConnected = status.loggedIn;

        }


        private async void RefreshAsync(object sender, RoutedEventArgs e)
        {
            await CheckUserStatusAsync();
            if (isConnected)
            {
                try
                {
                    btnRefresh.IsEnabled = false;
                    await LikedSongsPlaylistAsync();
                    await SoftwareTop40PlaylistAsync();
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
                    GenerateAIContent();
                    
                    if (!isAIPlaylistUpdated)
                    {
                        await LikedSongsPlaylistAsync();
                        await SoftwareTop40PlaylistAsync();
                    }
                 
                    if (!isUsersPlaylistUpdated) { await UsersPlaylistAsync(); }
                        
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
                SetConnectContent();
                SetWebAnalyticsContent();
                SetDeviceDetectionContent();
                SeperatorContent();
                GenerateAIContent();
                UsersPlaylistTV.Items.Clear();
                SoftwarePlaylistTV.Items.Clear();
                LikePlaylistTV.Items.Clear(); 
                isUsersPlaylistUpdated  = false;
                isAIPlaylistUpdated     = false;
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

                    if(MusicManager.getDevices()!= null )
                    {
                        devices = MusicManager.getDevices();
                        
                        if(devices.Count>1)
                        {
                            DeviceLabel.Content = "Available on" + MusicManager.getDeviceNames();
                        }
                        else
                        {
                            string Active_Device = MusicManager.getActiveDeviceName();
                            if(!string.IsNullOrEmpty(Active_Device))
                            {
                                DeviceLabel.Content = "Listening on " + MusicManager.getActiveDeviceName();
                            }
                            else { DeviceLabel.Content = "No device is active"; }

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
            //Playlist.generateMyAIPlaylistAsync();
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
       
      
        private async Task LikedSongsPlaylistAsync()
        {
            try
            {
                TreeViewItem treeItem = null;
                if (isConnected)
                {
                    

                    List<PlaylistItem> playlistItems    = await Playlist.getPlaylistsAsync();
                    List<Track> LikedTracks             = new List<Track>();
                    LikedSongIds                        = new List<Track>();
                    LikedTracks = await Playlist.getSpotifyLikedSongsAsync();
                    treeItem    = GetTreeView("LikedSongs", "spotify.png", null);

                    if (LikedTracks.Count > 0)
                    {
                        foreach (Track item in LikedTracks)
                        {
                            LikedSongIds.Add(item);
                            TreeViewItem playlistTreeviewItem      = GetTrackTreeView(item.name, "share.png", item.id);

                            playlistTreeviewItem.MouseLeftButtonUp += PlayLikedSongs;

                            treeItem.Items.Add(playlistTreeviewItem);

                        }
                        if (LikePlaylistTV.Items.Count > 0)
                        {
                            LikePlaylistTV.Items.Clear();
                        }

                        LikePlaylistTV.Items.Add(treeItem);
                        isAIPlaylistUpdated = true;
                    }
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

        private async void PlayLikedSongs(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string playlistID   = string.Empty;
                string trackID      = string.Empty;

                PlaylistTreeviewItem parent = null;
                PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;
               
                parent = GetSelectedTreeViewItemParent(item);

                if (parent != null)
                {
                    playlistID  = parent.PlayListId;
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
                    await MusicController.LaunchPlayerAsync();

                    await MusicManager.getDevicesAsync();
                    await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
                }

            }
            catch (Exception ex)
            {


            }
        }

        private async Task SoftwareTop40PlaylistAsync()
        {
            try
            {
                TreeViewItem treeItem = null;

                if (isConnected)
                {
                    List<PlaylistItem> playlistItems      = await Playlist.getPlaylistsAsync();
                    List<Track> Swtoptracks               = new List<Track>();

                    Swtoptracks = await Playlist.getPlaylistTracksAsync(Constants.SOFTWARE_TOP_40_ID);
                    treeItem    = GetTreeView("Software top 40", "PAW.png", Constants.SOFTWARE_TOP_40_ID);
                   
                    if (Swtoptracks.Count > 0)
                    {
                        treeItem.MouseLeftButtonUp += PlayPlaylist;

                        foreach (Track item in Swtoptracks)
                        {
                            TreeViewItem playlistTreeviewItem = GetTrackTreeView(item.name, "share.png", item.id);
                          
                            playlistTreeviewItem.MouseLeftButtonUp += PlaySelectedSongAsync;

                            treeItem.Items.Add(playlistTreeviewItem);

                        }
                        if (SoftwarePlaylistTV.Items.Count > 0)
                        {
                            SoftwarePlaylistTV.Items.Clear();
                        }
                        SoftwarePlaylistTV.Items.Add(treeItem);
                        isAIPlaylistUpdated = true;
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
        private async Task UsersPlaylistAsync()
        {
            try
            {
                if (isConnected)
                {
                    List<TreeViewItem> treeItemList     = new List<TreeViewItem>();
                    List<PlaylistItem> playlistItems    = await Playlist.getPlaylistsAsync();
                    List<Track> tracks                  = new List<Track>();
                   
                    foreach (PlaylistItem playlists in playlistItems)
                    {
                        TreeViewItem treeItem   = null;
                        treeItem                = GetTreeView(playlists.name, "spotify.png", playlists.id);
                        treeItem.MouseLeftButtonUp += PlayPlaylist;
                        tracks                  = await Playlist.getPlaylistTracksAsync(playlists.id);

                        foreach (Track item in tracks)
                        {
                            TreeViewItem playlistTreeviewItem       = GetTrackTreeView(item.name, "share.png", item.id);

                            playlistTreeviewItem.MouseLeftButtonUp += PlaySelectedSongAsync;

                            treeItem.Items.Add(playlistTreeviewItem);
                        }

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

        private async void PlayPlaylist(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string playlistID       = string.Empty;
                string trackID          = string.Empty;


                PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;
                playlistID                  = item.PlayListId;
                

                if (MusicManager.isDeviceOpened())
                {
                    await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
                }
                else
                {
                    await MusicController.LaunchPlayerAsync();

                    await MusicManager.getDevicesAsync();

                    await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
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
                parent                      = GetSelectedTreeViewItemParent(item);
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
                        await MusicController.LaunchPlayerAsync();

                        await MusicManager.getDevicesAsync();
                        await MusicManager.SpotifyPlayPlaylistAsync(playlistID, trackID);
                    }
                
            }
            catch (Exception ex)
            {

                
            }
           
        }

        private PlaylistTreeviewItem GetSelectedTreeViewItemParent(PlaylistTreeviewItem item)
        {
            DependencyObject parent = null;
            try
            {
                parent = VisualTreeHelper.GetParent(item);

                while (!(parent is PlaylistTreeviewItem))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }


               
            }
            catch ( Exception ex)
            {

                
            }
            return parent as PlaylistTreeviewItem;

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