using SoftwareCo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MusicTime
{
    class MusicStateManager
    {
        private static MusicStateManager instance = null;
        private static MusicManager musicManager = MusicManager.getInstance;
        private static Track ExsitingTrack;
        private static trackProgressInfo trackProgressInfo      = new trackProgressInfo();
        public static ChangeStatus changeStatus                 = new ChangeStatus();
        private MusicStateManager()
        {
        }

        public static MusicStateManager getInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MusicStateManager();
                }
                return instance;
            }
        }

        public async void GatherMusicInfo()
        {
            if (MusicManager.hasSpotifyPlaybackAccess())
            {
                Track playingTrack      = new Track();
                playingTrack            = await musicManager.GetCurrentTrackAsync();
                NowTime nowTime         = SoftwareCoUtil.GetNowTime();

                if (playingTrack != null)
                {
                    
                    if (playingTrack.uri == null && playingTrack.id != null)
                    {
                        playingTrack.uri = MusicUtil.createUriFromTrackId(playingTrack.id);
                    }
                    else
                        playingTrack.id = MusicUtil.CreateSpotifyIdFromUri(playingTrack.id);



                    changeStatus = await getChangeStatus(playingTrack, nowTime);

                    if (changeStatus.isNewSong)
                    {
                       

                    }


                    if (changeStatus.sendSongSession)
                    {
                        ExsitingTrack.state = trackState.Playing;

                        if (ExsitingTrack.end == 0)
                        {
                            ExsitingTrack.end       = nowTime.now;
                            ExsitingTrack.local_end = nowTime.local_now;
                        }

                        Track SongSession = ExsitingTrack;

                        gatherCodingDataAndSendSongSessionAsync(SongSession);

                        ExsitingTrack       = null;
                        if (playingTrack    != null)
                        {
                            ExsitingTrack = new Track();
                        }


                        ExsitingTrack.start         = nowTime.now;
                        ExsitingTrack.local_start   = nowTime.local_now;

                        resetTrackProgressInfo(playingTrack);
                    }

                    if(ExsitingTrack.id != playingTrack.id)
                    {
                        ExsitingTrack = playingTrack;
                    }
                    if(ExsitingTrack.state !=playingTrack.state)
                    {
                        ExsitingTrack.state = playingTrack.state;
                    }

                    ExsitingTrack.start = nowTime.now;
                    ExsitingTrack.local_start = nowTime.local_now;
                    ExsitingTrack.end = 0;

                }
            }
        }

        private void resetTrackProgressInfo(Track playingTrack)
        {
            trackProgressInfo.endInRange = false;
            trackProgressInfo.lastUpdateUtc = 0;
            trackProgressInfo.progress_ms = 0;
            trackProgressInfo.duration_ms = 0;
            trackProgressInfo.id = null;
        }

        

        private async void gatherCodingDataAndSendSongSessionAsync(Track songSession)
        {
           // TrackData songSession = new TrackData(trackData);

           if(songSession.album!=null)
            {
              
            }
           
            List<string> payloads   = await getDataRows(SoftwareCoUtil.getMusicDataStoreFile());
            
            bool isValidSession     = songSession.end - songSession.start > 5;

            if (!isValidSession)
            {
                return;
            }

            string genre = songSession.genre;
            string genreP;
            Track fullTrackP = new Track();

            // fetch the full track or genre
            if (songSession.type == "Spotify")
            {
                // just fetch the entire track
                fullTrackP = await MusicManager.GetSpotifyTrackByIdAsync(
                    songSession.id,
                    true /*includeFullArtist*/,
                    true /*includeAudioFeatures*/,
                    true /*includeGenre*/
                );
            }
            else if (!string.IsNullOrEmpty(genre))
            {
                // fetch the genre
                string artistName = MusicManager.getArtist(
                    songSession
                );
                string  songName = songSession.name;
                string artistId = songSession.artists[0].id;
                    
                genreP = await MusicManager.getGenreAsync(artistName, songName, artistId);
            }

           

           
            if (payloads.Count > 0)
            {
               List<SoftwareData> softwareData = new List<SoftwareData>();

                foreach (string item in payloads)
                {
                    SoftwareData datas = new SoftwareData();
                    datas = JsonConvert.DeserializeObject<SoftwareData>(item);
                    softwareData.Add(datas);
                }

                foreach (SoftwareData item in softwareData)
                {

                    foreach (var keyValue in item.source)
                    {


                    }


                    songSession.source.Add(item.source);
                }

                
                
            }

            if (fullTrackP!=null)
            {
                songSession.album               = fullTrackP.album;
                songSession.features            = fullTrackP.features;
                songSession.artists             = fullTrackP.artists;
                songSession.artist              = fullTrackP.artist;
                songSession.artist_names        = fullTrackP.artist_names;
             
                
                if (songSession.genre==null)
                {
                    songSession.genre = fullTrackP.genre;
                }
                
                
            }

           

            TrackData trackData = new TrackData(songSession);

            // send the music data, if we're online
            sendMusicData(trackData);

        }

        private void sendMusicData(TrackData songSessionPayload)
        {
            Logger.Debug(songSessionPayload.GetAsJson());
         
        }

        private Task<List<string>> getDataRows(string FilePath)
        {
            List<string> payloads   = new List<string>() ;
            string payload          = "";
            if (File.Exists(FilePath))
            {
                payload = File.ReadAllText(FilePath);
                string[] stringSeparators = new string[] { "\r\n" };
                string[] lines = payload.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string val in lines)
                {
                    payloads.Add(val);
                }
            }
            return Task.FromResult(payloads);
        }

        public async Task<ChangeStatus> getChangeStatus(Track playingTrack,NowTime LocalUTC)
        {
            changeStatus.isNewSong = false;

            bool isValidExistingTrack = ExsitingTrack != null ? true : false;

            if (ExsitingTrack!=null)
            {
                 changeStatus.isNewSong = ExsitingTrack.id != playingTrack.id ? true : false;
            }
            else
            {
                ExsitingTrack = playingTrack;
            }

            bool endInRange = isEndInRange(playingTrack);


            long lastUpdatedUtc = playingTrack.state == trackState.Playing ? LocalUTC.now : trackProgressInfo.lastUpdateUtc;
            //changeStatus.trackIsDone     = isTrackDone(playingTrack);
            changeStatus.isLongPaused    = isTrackLongPaused(playingTrack);


            changeStatus.sendSongSession = isValidExistingTrack && (changeStatus.isNewSong || changeStatus.isLongPaused) ? true : false;

            if (changeStatus.isLongPaused)
            {
                if (changeStatus.sendSongSession)
                {
                   
                    ExsitingTrack.end = lastUpdatedUtc + 5;
                    long local = (long)(lastUpdatedUtc - LocalUTC.offset_now);
                    ExsitingTrack.local_end = local + 5;
                }
                lastUpdatedUtc = LocalUTC.now;
            }

            SetTrackProgressInfo(playingTrack, endInRange, lastUpdatedUtc);
 

            return changeStatus;
            
        }

        private void SetTrackProgressInfo(Track playingTrack, bool endInRange, long lastUpdatedUtc)
        {
            trackProgressInfo.endInRange = endInRange;
            trackProgressInfo.lastUpdateUtc = lastUpdatedUtc;
            trackProgressInfo.progress_ms = playingTrack.progress_ms;
            trackProgressInfo.duration_ms = playingTrack.duration_ms;
            trackProgressInfo.id = playingTrack.id;

        }

        private bool isEndInRange(Track playingTrack)
        {
            double buffer = playingTrack.duration_ms * 0.07;
            return playingTrack.progress_ms >= playingTrack.duration_ms - buffer;

        }

        private bool isTrackLongPaused(Track playingTrack)
        {
            bool hasProgress = playingTrack.progress_ms > 0 ? true : false;

            NowTime nowTime = SoftwareCoUtil.GetNowTime();
            double pauseThreshold = 60;

            double diff = nowTime.now - trackProgressInfo.lastUpdateUtc;

            if (hasProgress && trackProgressInfo.lastUpdateUtc > 0 && diff > pauseThreshold )
            {
                return true;
            }
            else
                return false;
        }

        private bool isTrackDone(Track playingTrack)
        {
            return true;
        }
    }
    class ChangeStatus
    {
        public bool isNewSong { get; set; }
        public bool sendSongSession { get; set; }
        public bool trackIsDone { get; set; }
        public bool isLongPaused { get; set; }
    }
    class trackProgressInfo
    {
        public bool endInRange = false;
        public double duration_ms = 0;
        public double progress_ms = 0;
        public string id = null;
        public long lastUpdateUtc = 0;
        public string playlistId = null;
    }
}
