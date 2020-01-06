namespace MusicTime
{
    using System;
    using System.Collections.Generic;
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
        private static bool isConnected;
        private static Playlist _Playlist = Playlist.getInstance;
        private static int THIRTY_SECONDS = 1000 * 30;

        public SpotifyPlayListControl()
        {
            this.InitializeComponent();
            CheckUserStatusAsync();
            //SoftwareCoUtil.SetTimeout(THIRTY_SECONDS/3, UpdateTreeviewAsync, false);
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
            UpdateTreeviewAsync();
        }

        public  async void UpdateTreeviewAsync()
        {
            await CheckUserStatusAsync();

            if (isConnected)
            {
                SetConnectContent();
                SetWebAnalyticsContent();
                SetDeviceDetectionContent();
                SeperatorContent();
                GenerateAIContent();
                //LikedSongsPlaylist();
                SoftwareTop40Playlist();
               // UsersPlaylist();

            }
            else
            {
                clearAll();
            }
        }

        private void clearAll()
        {
            SetConnectContent();
            SetWebAnalyticsContent();
            SetDeviceDetectionContent();
            SeperatorContent();
            GenerateAIContent();
            LikedSongsPlaylist();
            SoftwareTop40Playlist();
            UsersPlaylist();
        }


        //UI Setters for button /label /icons
        private void SetConnectContent()
        {
            try
            {
                if (isConnected)
                {

                    ConnectLabel.Content = "Spotify Connected";
                    ConnectImage.Source = new BitmapImage(new Uri("Resources/Connected.png", UriKind.Relative));
                }
                else
                {
                    ConnectLabel.Content = "Connect to spotify";
                    ConnectImage.Source = new BitmapImage(new Uri("Resources/Connected.png", UriKind.Relative));
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

                AnalyticLabel.Content = "See web Analytics";
                AnalyticImage.Source = new BitmapImage(new Uri("Resources/PAW.png", UriKind.Relative));
            }
            else
            {
                AnalyticLabel.Content = null;
                AnalyticImage.Source = null;
            }

        }
        private void SetDeviceDetectionContent()
        {
            if (isConnected)
            {

                if (MusicManager.isDeviceOpened())
                {
                    DeviceImage.Source = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                    DeviceLabel.Content = MusicManager.getDeviceName();
                }
                else
                {
                    DeviceImage.Source = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
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

                GeneratePlaylistLabel.Content = "Generate AI Playlist";
                GeneratePlaylistImage.Source = new BitmapImage(new Uri("Resources/settings.png", UriKind.Relative));
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
        //Default Playlists Functions


        private async void LikedSongsPlaylist()
        {
            TreeView PlaylistTree = new TreeView();
            TreeViewItem treeItem = null;
            if (isConnected)
            {
                if (PlaylistTree.Items.Count > 0)
                {
                    PlaylistTree.Items.Clear();
                }

                List<Track> LikedSongs  = null;
                treeItem                = GetTreeView("LikedSongs", "spotify.png");
                LikedSongs              = await Playlist.getSpotifyLikedSongsAsync();
               
                foreach (Track item in LikedSongs)
                {
                   
                    PlaylistTreeviewItem playlistTreeviewItem = new PlaylistTreeviewItem(null,item.id, item.name);
                    playlistTreeviewItem.Header               = item.name;
                    
                    playlistTreeviewItem.Foreground           = System.Windows.Media.Brushes.RoyalBlue;
                    playlistTreeviewItem.MouseDoubleClick     += PlaySelectedSong;
                    treeItem.Items.Add(playlistTreeviewItem);
                }
                PlaylistTree.Items.Add(treeItem);
                PlaylistTreeview.Children.Add(PlaylistTree);
            }
            else
            {
                PlaylistTree.Items.Clear();
                treeItem = null;
            }

        }

      

        private async void SoftwareTop40Playlist()
        {
            TreeView SpotifPlaylistTreeview = new TreeView();
            
            TreeViewItem treeItem = null;
            if (isConnected)
            {
                if (SpotifPlaylistTreeview.Items.Count > 0)
                {
                    SpotifPlaylistTreeview.Items.Clear();
                }

               
                List<Track> SoftwareTop40           = null;

                treeItem                            = GetTreeView("Software top 40", "PAW.png");

                SoftwareTop40                       = await Playlist.getPlaylistTracksAsync(Constants.SOFTWARE_TOP_40_ID); ;

                if (SoftwareTop40.Count > 0)
                {
                    foreach (Track item in SoftwareTop40)
                    {
                        PlaylistTreeviewItem playlistTreeviewItem = new PlaylistTreeviewItem(null,item.id, item.name);
                        playlistTreeviewItem.Header               = item.name;
                      
                        playlistTreeviewItem.MouseDoubleClick     += PlaySelectedSong;
                        playlistTreeviewItem.Foreground = System.Windows.Media.Brushes.DarkCyan;
                        treeItem.Items.Add(playlistTreeviewItem);
                    }

                    SpotifPlaylistTreeview.Items.Add(treeItem);
                    PlaylistTreeview.Children.Add(SpotifPlaylistTreeview);
                }
            }
            else
            {
                SpotifPlaylistTreeview.Items.Clear();
                treeItem = null;
            }
        }


        private void PlaySelectedSong(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PlaylistTreeviewItem item = sender as PlaylistTreeviewItem;
            
        }

       
        private async void UsersPlaylist()
        {
            TreeView UsersPlaylists     = new TreeView();
            TreeViewItem treeItem       = null;
            if (isConnected)
            {
                List<PlaylistItem> playlistItems = await Playlist.getPlaylistsAsync();
                List<Track> tracks               = new List<Track>();
                

                //if (UsersPlaylists.Items.Count > 0)
                //{
                //    UsersPlaylists.Items.Clear();
                //}


                foreach (PlaylistItem playlists in playlistItems)
                {
                    treeItem    = GetTreeView(playlists.name, "spotify.png");

                    tracks      = await Playlist.getPlaylistTracksAsync(playlists.id);

                    foreach (Track item in tracks)
                    {
                        PlaylistTreeviewItem playlistTreeviewItem = new PlaylistTreeviewItem(null,item.id, item.name);
                        playlistTreeviewItem.Header               = item.name;
                        
                        playlistTreeviewItem.MouseDoubleClick     += PlaySelectedSong;

                        treeItem.Items.Add(playlistTreeviewItem);
                    }

                    StackPanel UsersPlaylistTreeview = new StackPanel();
                    UsersPlaylists.Items.Add(treeItem);
                    //UserPlaylistTreeview.Content = UsersPlaylists;
                   // UsersPlaylistTreeview.Children.Add(UsersPlaylists);
                }
            }
            else
            {
             //   UsersPlaylists.Items.Clear();
               // treeItem = null;
            }

        }

        private TreeViewItem GetTreeView(string text, string imagePath)
        {
            TreeViewItem item = new TreeViewItem();
            
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