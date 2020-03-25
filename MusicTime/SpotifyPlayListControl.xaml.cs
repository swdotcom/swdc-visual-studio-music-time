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
        public static bool isRecommendPlaylistUpdated   = false;
        public static bool isMusicTimePlaylistUpdated   = false;
        public static List<Track> LikedSongIds          = null;
        public static bool SortPlaylistFlag             = false;
        public static bool AIPlyalistGenerated          = false;
        public static string AIPlaylistID               = null;
        public static List<Track> recommendedSongs  = new List<Track>();
        public static List<Track> likedSongs        = new List<Track>();
        public static List<Device> WebDevices       = new List<Device>();
        public static List<Device> ComputerDevices  = new List<Device>();
        public static PlaylistItem AIPlaylistItem       = null;
        public string recommendedType = "Familiar";
        public static bool isOffsetChange           = false;
     
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
            if (isMusicTimeConnected())
            {
                setLoading();
            }
            else
                SetConnectContent();

            System.Windows.Forms.Timer UpdateCallBackTimer = new System.Windows.Forms.Timer();
            UpdateCallBackTimer.Interval = 5000;//5 seconds
            UpdateCallBackTimer.Tick += new System.EventHandler(UpdateCallBack);
            UpdateCallBackTimer.Start();
            
        }

        private void setLoading()
        {
            try
            {
                
                    ConnectLabel.Content = "Loading";
                    ConnectImage.Source = new BitmapImage(new Uri("Resources/loading_blue.png", UriKind.Relative));
                

            }
            catch (Exception e)
            {


            }
        }

        private bool isMusicTimeConnected()
        {
            bool flag = false;
            try
            {
                string accestoken =(string)SoftwareCoUtil.getItem("spotify_access_token");
                if (!string.IsNullOrEmpty(accestoken))
                {
                    flag = true;
                }
            }
            catch (Exception ex)
            {

                Logger.Debug(ex.Message);
            }

            return flag;
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

                if (isConnected )
                {
                    btnRefresh.Visibility = Visibility.Visible;
                    SetConnectContent();
                    SetWebAnalyticsContent();
                    SetDeviceDetectionContentAsync();
                    SeperatorContent(); 
                    SetSortContent();
                   
                    SetRecommendContentAsync();
                    if (!isUsersPlaylistUpdated) { UsersPlaylistAsync(); }
                    if (!isAIPlaylistUpdated)
                    {
                        SetGenerateAIContent();
                        AIPlaylistAsync();
                    }

                    if(!isRecommendPlaylistUpdated)
                    {
                        RecommendPlaylistAsync();
                    }
                    if(!isMusicTimePlaylistUpdated)
                    {
                       // await LikedSongsPlaylistAsync();
                        SoftwareTop40PlaylistAsync();
                    }
                 
                  
                    
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
                RecommendedPlaylistTV.Items.Clear();
                isUsersPlaylistUpdated  = false;
                isAIPlaylistUpdated     = false;
                isMusicTimePlaylistUpdated = false;
                isRecommendPlaylistUpdated = false;
                SetConnectContent();
                SetWebAnalyticsContent();
                SetRecommendContentAsync();
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
                    ConnectLabel.Content = "Spotify Connected";
                    ConnectImage.Source = new BitmapImage(new Uri("Resources/Connected.png", UriKind.Relative));
                }
                else
                {
                    if (isMusicTimeConnected())
                    {
                        setLoading();
                    }
                    else
                    {
                        ConnectLabel.Content = "Connect Spotify";
                        ConnectImage.Source = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                    }

                }
            }
            catch (Exception e)
            {


            }
        }
        private void SetWebAnalyticsContent()
        {
            if (isConnected )
            {

                AnalyticLabel.Content   = "See web analytics";
                AnalyticImage.Source    = new BitmapImage(new Uri("Resources/PAW_Circle_purple.png", UriKind.Relative));
            }
            else
            {
                AnalyticLabel.Content   = null;
                AnalyticImage.Source    = null;
            }

        }

        private async void SetRecommendContentAsync()
        {
            if (isConnected )
            {

                Lbl_recommend.Content   = "RECOMMENDATIONS";
                Img_recommend.Source    = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                btn_category.Visibility = Visibility.Visible;
                btn_mood.Visibility     = Visibility.Visible;
                btn_refresh.Visibility  = Visibility.Visible;
                btn_mood.Click      += Btn_mood_Click;
                btn_category.Click  += Btn_category_Click;
                btn_refresh.Click   += Btn_refresh_Click;
            }
            else
            {       
                Lbl_recommend.Content = null;
                Img_recommend.Source = null;
                btn_category.Visibility = Visibility.Hidden;
                btn_mood.Visibility = Visibility.Hidden;
                btn_refresh.Visibility = Visibility.Hidden;
            }

        }

        private async void Btn_refresh_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if(isOffsetChange)
            {
                isOffsetChange = false;
            } 
            else
            {
                isOffsetChange = true;
            }

            isRecommendPlaylistUpdated = false;
            await RecommendPlaylistAsync(recommendedType);

        }

        private async void Btn_category_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button item = sender as Button;

            if (item != null)
            {
                item.ContextMenu = await getCategoryContext();
            }

           
        }

        private async Task<ContextMenu> getCategoryContext()
        {
            ContextMenu contextmenu = new ContextMenu();
            customCombo CmbCategoryMenu = new customCombo();
            CmbCategoryMenu.ItemsSource = Constants.spotifyGenres;// FontColors is list<objects>
            CmbCategoryMenu.SelectedIndex = 0;
            CmbCategoryMenu.SelectionChanged += new SelectionChangedEventHandler(OnSelectionChanged); 
            contextmenu.Items.Add(CmbCategoryMenu);
            contextmenu.IsOpen = true;
            return contextmenu;
        }
        private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string cbi = (string) (sender as ComboBox).SelectedItem;
            if (cbi != null)
            {
                isRecommendPlaylistUpdated = false;
                await RecommendPlaylistAsync(cbi);
                recommendedType = cbi;
            }
        }
       

        private async void Btn_mood_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Button item = sender as Button;

            if(item!=null)
            {
                item.ContextMenu = await getMoodContext();
            }
        }

        private async Task<ContextMenu> getMoodContext()
        {
            ContextMenu moodContext = new ContextMenu();

            MoodMenu HappyMenu      = new MoodMenu("Happy");
            
            HappyMenu.Click         += MoodMenu_Click;

            MoodMenu EnergeticMenu = new MoodMenu("Energetic");

            EnergeticMenu.Click += MoodMenu_Click;

            MoodMenu DanceableMenu = new MoodMenu("Danceable");
            DanceableMenu.Click += MoodMenu_Click;
            MoodMenu InstrumentalMenu = new MoodMenu("Instrumental");
            InstrumentalMenu.Click += MoodMenu_Click;

            MoodMenu FamiliarMenu = new MoodMenu("Familiar");
            FamiliarMenu.Click += MoodMenu_Click;
            MoodMenu QuietMenu = new MoodMenu("Quiet");
            QuietMenu.Click += MoodMenu_Click;

            moodContext.Items.Add(HappyMenu);
                moodContext.Items.Add(EnergeticMenu);
                moodContext.Items.Add(DanceableMenu);
                moodContext.Items.Add(InstrumentalMenu);
                moodContext.Items.Add(FamiliarMenu);
                moodContext.Items.Add(QuietMenu);
                moodContext.IsOpen = true;
            return moodContext;
        }

        private async void MoodMenu_Click(object sender, RoutedEventArgs e)
        {
            MoodMenu item = sender as MoodMenu;

           if(item != null)
            {
                isRecommendPlaylistUpdated = false;
                await RecommendPlaylistAsync(item.moodValue);
                recommendedType = item.moodValue;
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
                        string activeDevice = MusicManager.getActiveDeviceName();

                        if(!string.IsNullOrEmpty(activeDevice))
                        {
                            DeviceLabel.Background = System.Windows.Media.Brushes.Transparent;
                            DeviceLabel.Content = "Listening on "+activeDevice;
                            DeviceLabel.ToolTip = "Listening on a Spotify device" ;
                        }
                        else
                        {
                            if(WebDevices.Count>0)
                            {
                                string deviceName =  WebDevices[0].name;
                                DeviceLabel.Content = "Available on " + deviceName;
                                DeviceLabel.Background = System.Windows.Media.Brushes.Transparent;
                                DeviceLabel.ToolTip = "Available on a Spotify device" ;
                            }
                            else if(ComputerDevices.Count>0)
                            {
                                string deviceName = ComputerDevices[0].name;
                                
                                 DeviceLabel.Background = System.Windows.Media.Brushes.Transparent;

                                DeviceLabel.Content = "Available on " + deviceName;
                            }
                            else
                            {
                                DeviceLabel.Background = System.Windows.Media.Brushes.Transparent;
                                DeviceLabel.Content = "Connect to a Spotify device";
                                DeviceLabel.ToolTip = "Click to launch web or desktop player";
                            }
                        }
                        DeviceLabel.Background = System.Windows.Media.Brushes.Transparent;
                        DeviceImage.Source = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                        DeviceLabel.Click += DeviceLabel_ClickAsync;


                    }
                }
                else
                {

                    DeviceImage.Source      = new BitmapImage(new Uri("Resources/spotify.png", UriKind.Relative));
                    DeviceLabel.Content     = "Connect to a Spotify device";
                    WebDevices              = new List<Device>();
                    ComputerDevices         = new List<Device>();
                    DeviceLabel.Click       += DeviceLabel_ClickAsync; 
                   
                }
            }
            else
            {
                DeviceLabel.Content = null;
                DeviceImage.Source  = null;
            }

        }

        public async void PlayTrackFromContext(string playlist_id ,string track_id,List<Track> tracks)
        {
            try
            {
                DeviceLabel.ContextMenu = await getDeveviceContext(WebDevices, ComputerDevices, playlist_id, track_id,tracks);
            }
            catch (Exception ex)
            {

               
            }
            
        }

        private async void DeviceLabel_ClickAsync(object sender, RoutedEventArgs e)
        {
            if(e!=null)
            e.Handled = true;

            DeviceLabel.ContextMenu = await getDeveviceContext(WebDevices, ComputerDevices,null,null,null);
          
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
                if (!item.name.Contains("Web Player") && item.type =="Computer")
                {
                    desktopDevices.Add(item);
                }
            }
            return desktopDevices;
        }

   
        private void SeperatorContent()
        {
            if (isConnected )
            {
                Seperator1.Visibility = Visibility.Visible;
                Seperator2.Visibility = Visibility.Visible;
                Seperator3.Visibility = Visibility.Visible;
            }
            else
            {
                Seperator1.Visibility = Visibility.Hidden;
                Seperator2.Visibility = Visibility.Hidden;
                Seperator3.Visibility = Visibility.Hidden;
            }
        }
        private void GenerateAIContent()
        {

            if (isConnected )
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
                if (isConnected )
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
        private async Task RecommendPlaylistAsync(string value = "Familiar" )
        {
            try
            {
                string playlistName = value;
                TreeViewItem RecommendedTreeItem = null;

                if (isConnected)
                {
                    
                    List<Track> Swtoptracks = new List<Track>();

                    
                    RecommendedTreeItem = PlaylistTreeviewUtil.GetTreeView(playlistName , "PAW.png", "Recommended Songs", value);
                    

                    RecommendedTreeItem.MouseLeftButtonUp += PlayPlaylist;
                    RecommendedTreeItem.Expanded          += AddTracksAsync;

                    RecommendedTreeItem.Items.Add(null);


                    if (RecommendedPlaylistTV.Items.Count > 0)
                    {
                        RecommendedPlaylistTV.Items.Clear();
                    }


                    RecommendedPlaylistTV.Items.Add(RecommendedTreeItem);
                    isRecommendPlaylistUpdated = true;

                }
                else
                {
                    RecommendedPlaylistTV.Items.Clear();

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
                  
                    List<Track> Swtoptracks               = new List<Track>();

                    SwtopTreeItem       = PlaylistTreeviewUtil.GetTreeView("Software top 40", "PAW_Circle.png", Constants.SOFTWARE_TOP_40_ID);
                    LikedTreeItem       = PlaylistTreeviewUtil.GetTreeView("Liked Songs", "Heart_Red.png", "Liked Songs");
                    
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
                        treeItem                        = PlaylistTreeviewUtil.GetTreeView(playlists.name, "playlist_w.png", playlists.id);
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

                    treeItem = PlaylistTreeviewUtil.GetTreeView(AIPlaylistItem.name, "PAW_Circle.png", AIPlaylistItem.id);

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
                likedSongs.Clear();
                recommendedSongs.Clear();
                e.Handled = true;
                bool isLikedSongs       = false;
                bool isrecommendedSong  = false;
                List<Track> tracks          = new List<Track>();
                PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;
                item.Items.Clear();

                if (item.PlayListId == "Liked Songs")
                {
                    
                    tracks = await Playlist.getSpotifyLikedSongsAsync();
                    likedSongs = tracks;
                    isLikedSongs = true;
                    
                }
                else if (item.PlayListId == "Recommended Songs")
                {
                   
                    tracks = await MusicManager.getRecommendationsForTracks(item.value,isOffsetChange);
                    recommendedSongs = tracks;
                    isrecommendedSong = true;
                    
                }
                else
                {
                    
                    tracks = await Playlist.getPlaylistTracksAsync(item.PlayListId);
                    
                }
                if (tracks.Count<1)
                {
                    TreeViewItem treeviewItem = PlaylistTreeviewUtil.GetTreeView("Your tracks will appear here", null, "EmptyPlaylist");
                    item.Items.Add(treeviewItem);
                }
               
                foreach (Track items in tracks)
                {
                    TreeViewItem playlistTreeviewItem = PlaylistTreeviewUtil.GetTrackTreeView(items.name, "track.png", items.id);

                    if (isLikedSongs)
                    {
                        playlistTreeviewItem.MouseLeftButtonUp += PlayLikedSongs;
                    }
                    else if (isrecommendedSong)
                    {
                        playlistTreeviewItem.MouseLeftButtonUp += PlayRecommendedSongs;
                    }
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
            e.Handled = true;
            string playlistID           = string.Empty;
            string trackID              = string.Empty;

            PlaylistTreeviewItem parent = null;
            PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;
            parent                      = PlaylistTreeviewUtil.GetSelectedTreeViewItemParent(item);
            if (parent != null)
            {
                
                    playlistID  = parent.PlayListId;
                    trackID     = item.PlayListId;
                
               
            }
            else
            {
                playlistID = item.PlayListId;

            }

            if (item != null)
            item.ContextMenu = await GetContextMenuAsync(playlistID, trackID);
            
        }

  


        private  async Task<ContextMenu> getDeveviceContext(List<Device> WebDevices ,List<Device> ComputerDevices, string playlist_id,string track_id,List<Track> tracks)
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
                webPlayerMenu.Header                = "Launch Spotify web player";
                webPlayerMenu.playlist_id           = playlist_id;
                webPlayerMenu.track_id              = track_id;
                webPlayerMenu.tracks                = tracks;
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
                desktoPlayerMenu.Header             = "Launch Spotify desktop";
                desktoPlayerMenu.playlist_id        = playlist_id;
                desktoPlayerMenu.track_id           = track_id;
                desktoPlayerMenu.tracks             = tracks;
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
               
                if (deviceContextMenu.playlist_id == "Liked Songs" || deviceContextMenu.playlist_id == "Recommended Songs" )
                {
                    await MusicManager.SpotifyPlayPlaylist(deviceContextMenu.playlist_id, deviceContextMenu.track_id, deviceContextMenu.tracks);
                }
                else
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

                if (deviceContextMenu.playlist_id == "Liked Songs" || deviceContextMenu.playlist_id == "Recommended Songs")
                {
                    await MusicManager.SpotifyPlayPlaylist(deviceContextMenu.playlist_id, deviceContextMenu.track_id, deviceContextMenu.tracks);
                }
                else
                    await MusicManager.SpotifyPlayPlaylistAsync(deviceContextMenu.playlist_id, deviceContextMenu.track_id);
            }
        }

        private static async Task<ContextMenu> GetContextMenuAsync(string playlist_Id ,string track_id)
        {
            ContextMenu contextMenu = new ContextMenu();

            CustomMenu addMenu      = new CustomMenu();
            addMenu.Header          = "Add Song";
            addMenu.PlaylistId      = playlist_Id;
            addMenu.trackId         = track_id;
            addMenu.Foreground      = System.Windows.Media.Brushes.DarkCyan;
            addMenu.Click           += AddMenu_Click;

            CustomMenu removeMenu   = new CustomMenu();
            removeMenu.Header       = "Remove Song";
            removeMenu.PlaylistId   = playlist_Id;
            removeMenu.trackId      = track_id;
            removeMenu.Foreground   = System.Windows.Media.Brushes.DarkCyan;
            removeMenu.Click        += RemoveMenu_Click;

            CustomMenu copyToClipBoardMenu = new CustomMenu();
            copyToClipBoardMenu.Header = "Copy Song Link";
            copyToClipBoardMenu.PlaylistId = playlist_Id;
            copyToClipBoardMenu.trackId = track_id;
            copyToClipBoardMenu.Foreground = System.Windows.Media.Brushes.DarkCyan;
            copyToClipBoardMenu.Click += CopyToClipBoardMenu_Click;


            CustomMenu facebookMenu     = new CustomMenu();
            facebookMenu.Header         = "Facebook";
            facebookMenu.PlaylistId     = playlist_Id;
            facebookMenu.trackId = track_id;
            facebookMenu.Foreground     = System.Windows.Media.Brushes.DarkCyan;
            facebookMenu.Click += FacebookMenu_Click;

            CustomMenu twitterMenu      = new CustomMenu();
            twitterMenu.Header          = "Twitter";
            twitterMenu.PlaylistId      = playlist_Id;
            twitterMenu.trackId = track_id;
            twitterMenu.Foreground      = System.Windows.Media.Brushes.DarkCyan;
            twitterMenu.Click           += TwitterMenu_Click; ;

            CustomMenu whatsAppMenu     = new CustomMenu();
            whatsAppMenu.Header         = "WhatsApp";
            whatsAppMenu.PlaylistId     = playlist_Id;
            whatsAppMenu.trackId = track_id;
            whatsAppMenu.Foreground     = System.Windows.Media.Brushes.DarkCyan;
            whatsAppMenu.Click += WhatsAppMenu_Click; ;

            CustomMenu tumblerMenu      = new CustomMenu();
            tumblerMenu.Header          = "Tumbler";
            tumblerMenu.PlaylistId      = playlist_Id;
            tumblerMenu.trackId = track_id;
            tumblerMenu.Foreground      = System.Windows.Media.Brushes.DarkCyan;
            tumblerMenu.Click += TumblerMenu_Click;

            CustomMenu shareMenuItem    = new CustomMenu();
            shareMenuItem.Header        = "Share Song";
            shareMenuItem.Foreground    = System.Windows.Media.Brushes.DarkCyan;

            CustomMenu slackMenuItem =  await GetSlackMenuItemAsync(playlist_Id,track_id);


            CustomMenu addMenuItem      = await getAddMenuItem(playlist_Id,"");
            addMenuItem.Header          = "Add Song";

            shareMenuItem.Items.Add(facebookMenu);
            shareMenuItem.Items.Add(twitterMenu);
            shareMenuItem.Items.Add(whatsAppMenu);
            shareMenuItem.Items.Add(tumblerMenu);
            shareMenuItem.Items.Add(slackMenuItem);

           // contextMenu.Items.Add(addMenuItem);
            contextMenu.Items.Add(removeMenu);
            contextMenu.Items.Add(copyToClipBoardMenu);
            contextMenu.Items.Add(shareMenuItem);
         
            return contextMenu;
        }

        private static async Task<CustomMenu> getAddMenuItem(string playlist_Id,string track_id)
        {
            CustomMenu addMenuItem = new CustomMenu();
            addMenuItem.Foreground = System.Windows.Media.Brushes.DarkCyan;

            CustomMenu createPlaylistMenu   = new CustomMenu();
            createPlaylistMenu.Foreground   = System.Windows.Media.Brushes.DarkCyan;
            createPlaylistMenu.Header       = "Create new playlist";
            createPlaylistMenu.PlaylistId   = playlist_Id;
            createPlaylistMenu.Click        += CreatePlaylistMenu_Click;

            CustomMenu selectMenu = new CustomMenu();
            selectMenu.Foreground = System.Windows.Media.Brushes.DarkCyan;
            selectMenu.PlaylistId = playlist_Id;
            selectMenu.trackId    = track_id;
            selectMenu.Header     = "Select playlist";
            selectMenu.Click      += SelectMenu_Click;

            addMenuItem.Items.Add(createPlaylistMenu);
            addMenuItem.Items.Add(selectMenu);

            return addMenuItem;
           

        }

        private async static void SelectMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;

                if (item != null)
                    item.ContextMenu = await getPlaylistContext();
            }
            catch (Exception ex)
            {


            }
        }

        private async static Task<ContextMenu> getPlaylistContext()
        {
            ContextMenu contextmenu = new ContextMenu();
            customCombo CmbCategoryMenu = new customCombo();
            CmbCategoryMenu.ItemsSource = Constants.spotifyGenres;// FontColors is list<objects>
            CmbCategoryMenu.SelectedIndex = 0;
          // CmbCategoryMenu.SelectionChanged += new SelectionChangedEventHandler(OnSelectionChanged);
            contextmenu.Items.Add(CmbCategoryMenu);
            contextmenu.IsOpen = true;
            return contextmenu;

        }

        private static void CreatePlaylistMenu_Click(object sender, RoutedEventArgs e)
        {
           

        }

        private static void CopyToClipBoardMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;

                if(item!=null)
                    SlackControlManager.CopylinkToClipboard(item.trackId);
            }
            catch (Exception ex)
            {

               
            }
         

        }

        private static async void AddMenu_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private static void RemoveMenu_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                e.Handled = true;
                CustomMenu item = sender as CustomMenu;

                if (item != null)
                    if (item.PlaylistId == "Liked Songs") 
                    {
                        MusicManager.removeToSpotifyLiked(item.trackId);
                    }
                else
                    Playlist.removeTracksToPlaylist(item.PlaylistId, item.trackId);
            }
            catch (Exception ex)
            {


            }
        }

        private static void FacebookMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomMenu item = sender as CustomMenu;

                if(item!=null)
                    SlackControlManager.ShareOnFacebook(item.trackId);
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
                    SlackControlManager.ShareOnTumbler(item.trackId);
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
                    SlackControlManager.ShareOnWhatsApp(item.trackId);
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
                SlackControlManager.ShareOnTwitter(item.trackId);

            }
            catch (Exception ex)
            {

                
            }
          
        }

        private static async Task<CustomMenu> GetSlackMenuItemAsync(string playlist_id,string track_id)
        {
            CustomMenu SlackMenuItem = new CustomMenu();
            SlackMenuItem.Foreground = System.Windows.Media.Brushes.DarkCyan;

          
            if (MusicTimeCoPackage.slackConnected)
            {
                SlackMenuItem.Header = "Slack";
              
                if (MusicTimeCoPackage.SlackChannels != null)
                {
                    foreach (Channel item in MusicTimeCoPackage.SlackChannels)
                    {
                        CustomMenu menuItem = new CustomMenu();
                        menuItem.Header = item.Name;
                        menuItem.PlaylistId = playlist_id;
                        menuItem.trackId = track_id;
                        menuItem.SlackChannelId = item.Id;
                        menuItem.Foreground = System.Windows.Media.Brushes.DarkCyan;
                        menuItem.Click += ShareOnSlackChannel; ;
                        SlackMenuItem.Items.Add(menuItem);

                    }
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
                SlackControlManager.ShareOnSlackChannel(item.SlackChannelId, item.trackId);
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
                    //options.playlist_id = playlistID;
                    //await MusicController.LaunchPlayerAsync(options);
                    PlayTrackFromContext(playlistID, trackID,null);
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
                    if(parent.PlayListId != "Liked Songs")
                    playlistID  = parent.PlayListId;

                    trackID     = item.PlayListId;
                    
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
                    Logger.Debug("Before playtrackFromContext"+playlistID  +":"+ trackID);
                        PlayTrackFromContext(playlistID, trackID,null);

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
                e.Handled = true;
                string playlistID   = string.Empty;
                string trackID      = string.Empty;               
                PlaylistTreeviewItem parent = null;
               
                PlaylistTreeviewItem item = sender as PlaylistTreeviewItem;
              //  List<Track> tracks =  await Playlist.getSpotifyLikedSongsAsync();
                parent = PlaylistTreeviewUtil.GetSelectedTreeViewItemParent(item);

                if (parent != null)
                {
                    trackID = item.PlayListId;
                    playlistID = parent.PlayListId;
                }
                
                if (MusicManager.isDeviceOpened())
                {
                    
                    await MusicManager.SpotifyPlayPlaylist(playlistID, trackID,likedSongs);
                }
                else
                {
                    PlayTrackFromContext(playlistID, trackID,likedSongs);

                }

            }
            catch (Exception ex)
            {


            }
        }

        private async void PlayRecommendedSongs(object sender, MouseButtonEventArgs e)
        {
            try
            {
                e.Handled = true;
                string playlistID   = string.Empty;
                string trackID      = string.Empty;
                PlaylistTreeviewItem parent = null;
                
                PlaylistTreeviewItem item   = sender as PlaylistTreeviewItem;
               // List<Track> tracks          = await MusicManager.getRecommendationsForTracks(item.value);
                parent                      = PlaylistTreeviewUtil.GetSelectedTreeViewItemParent(item);

                if (parent != null)
                {
                    playlistID = parent.PlayListId;

                    trackID = item.PlayListId;

                }

                if (MusicManager.isDeviceOpened())
                {

                    await MusicManager.SpotifyPlayPlaylist(playlistID, trackID, recommendedSongs);
                }
                else
                {
                    PlayTrackFromContext(playlistID, trackID, recommendedSongs);

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