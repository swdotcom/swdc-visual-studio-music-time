using SoftwareCo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Converters;

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

        public async void GatherMusicInfo(Track PlayingTrack)
        {
            if (MusicManager.hasSpotifyPlaybackAccess())
            {
              //  Track playingTrack      = new Track();
                Track playingTrack      = new Track();
                playingTrack            = PlayingTrack;//await MusicManager.GetCurrentTrackAsync();
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
                        NowTime Loacal_nowTime = SoftwareCoUtil.GetNowTime();
                        if (ExsitingTrack.end == 0)
                        {
                            ExsitingTrack.end       = Loacal_nowTime.now;
                            ExsitingTrack.local_end = Loacal_nowTime.local_now;
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
                    if (ExsitingTrack.start == 0)
                    {
                        ExsitingTrack.start = nowTime.now;
                        ExsitingTrack.local_start = nowTime.local_now;
                    }
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
          

           if(songSession.album!=null)
            {
              
            }
           
            List<string> payloads   = await getDataRows(SoftwareCoUtil.getMusicDataStoreFile());
            
            bool isValidSession     = songSession.end - songSession.start > 5;

            Logger.Debug("isvalidSession " + isValidSession.ToString());
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
            trackData           = setInitialValues(trackData);

            if (payloads.Count > 0)
            {
                List<SoftwareData> softwareData = new List<SoftwareData>();
                
                foreach (string item in payloads)
                {
                  
                    SoftwareData datas = new SoftwareData();
                    datas = JsonConvert.DeserializeObject<SoftwareData>(item);
                    softwareData.Add(datas);
                }
                
                

                trackData =  buildAggregateData(softwareData, trackData);
               
                
            }
            // send the music data, if we're online
            sendMusicDataAsync(trackData);

        }

        private TrackData setInitialValues(TrackData songSession)
        {
            #region OS_Version_Offset_Values

            if (songSession.timezone == null)
            {
                songSession.timezone = TimeZone.CurrentTimeZone.StandardName;
            }
            if (songSession.pluginId == 0)
            {
                songSession.pluginId = Constants.PluginId;
            }
            if (songSession.offset == 0)
            {
                songSession.offset = SoftwareCoUtil.GetNowTime().offset_now;
            }
            if (songSession.os == null)
            {
                songSession.os = MusicTimeCoPackage.GetOs();
            }
            if (songSession.version == null)
            {
                songSession.version = MusicTimeCoPackage.GetVersion();
            }
            return songSession;
            #endregion
        }

        private TrackData buildAggregateData(List<SoftwareData> softwareData, TrackData songSession)
        {
            long TotalKeystroke = 0;
            long add    = 0;
            long delete = 0;
            long open   = 0;
            long close  = 0;
            long linesAdded     = 0;
            long linesRemoved   = 0;
            long netkeys        = 0;

            #region OS_Version_Offset_Values

            if (songSession.timezone == null)
            {
                songSession.timezone = TimeZone.CurrentTimeZone.StandardName;
            }
            if (songSession.pluginId == null)
            {
                songSession.pluginId = Constants.PluginId;
            }
            if (songSession.offset == 0)
            {
                songSession.offset = SoftwareCoUtil.GetNowTime().offset_now;
            }
            if (songSession.os == null)
            {
                songSession.os = MusicTimeCoPackage.GetOs();
            }
            if (songSession.version == null)
            {
                songSession.version = MusicTimeCoPackage.GetVersion();
            }

            #endregion
            List<SourceData> sourceData = new List<SourceData>();
            List<sourceKey> sourceKeyData = new List<sourceKey>();
            foreach (SoftwareData item in softwareData)
            {
               
               
                TotalKeystroke  = TotalKeystroke + item.keystrokes;
                //List<JsonObject> keyValuePairs = new List<JsonObject>();
                foreach (KeyValuePair<string, object> entry in item.source)
                {
                    //sourceKey key       = new sourceKey();          
                    SourceData datas    = new SourceData();
                  //  JsonObject pair     = new JsonObject();
                    //key.key             = entry.Key;
                    datas               = JsonConvert.DeserializeObject<SourceData>(entry.Value.ToString());
                    //key.SourceData      = datas;
                  //  pair.Add(entry.Key, datas);
                    //keyValuePairs.Add(pair);
                    songSession.source.Add(entry.Key, datas);
                    
                    sourceData.Add(datas);
                   
                }

                //songSession.source.Add(keyValuePairs);
            }

            

            //songSession.source = item.source;

            if (sourceData.Count>0)
            {
                foreach (SourceData item in sourceData)
                {
                    add     = add + item.Add;
                    delete  = delete + item.Delete;
                    open    = open + item.Open;
                    close   = close + item.Close;
                    linesAdded      = linesAdded + item.LinesAdded;
                    linesRemoved    = linesRemoved + item.LinesRemoved;
                    netkeys         = netkeys + item.Netkeys;
                }
            }


            songSession.Add                 = add;
            songSession.Delete              = delete;
            songSession.Open                = open;
            songSession.Close               = close;
            songSession.LinesAdded          = linesAdded;
            songSession.LinesRemoved        = linesRemoved;
            songSession.Netkeys             = netkeys;
            songSession.keystrokes          = TotalKeystroke;

            return songSession;

        }

        private async void sendMusicDataAsync(TrackData songSessionPayload)
        {
            Logger.Debug(songSessionPayload.GetAsJson());
            string datastoreFile    = SoftwareCoUtil.getMusicDataStoreFile();
            string api              = "/music/session";
            HttpResponseMessage response = null;
            string app_jwt = SoftwareUserSession.GetJwt();
            if (app_jwt != null)
            {

                response = await SoftwareHttpManager.SendRequestAsync(HttpMethod.Post, api, songSessionPayload.GetAsJson(), app_jwt);
                if (SoftwareHttpManager.IsOk(response))
                {
                   // delete the file
                    File.Delete(datastoreFile);
                }
            }
            
         }

        private Task<List<string>> getDataRows(string FilePath)
        {
            List<string> payloads   = new List<string>() ;
            string payload          = "";
            if (File.Exists(FilePath))
            {
                payload = File.ReadAllText(FilePath);
                string[] stringSeparators = new string[] { "\r\n" };
                string[] lines            = payload.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

                foreach (string val in lines)
                {
                    payloads.Add(val);
                }
            }
            return Task.FromResult(payloads);
        }

        public async Task<ChangeStatus> getChangeStatus(Track playingTrack,NowTime LocalUTC)
        {
            changeStatus.isNewSong      = false;

            bool isValidExistingTrack   = ExsitingTrack != null ? true : false;

            if (ExsitingTrack!=null)
            {
                 changeStatus.isNewSong = ExsitingTrack.id != playingTrack.id ? true : false;
            }
            else
            {
                ExsitingTrack = playingTrack;
            }

            bool endInRange = isEndInRange(playingTrack);

            long lastUpdatedUtc;
            if (playingTrack.state == trackState.Playing)
            {
               lastUpdatedUtc = LocalUTC.now;
            }
            else
                lastUpdatedUtc = trackProgressInfo.lastUpdateUtc;


          //  long lastUpdatedUtc = playingTrack.state == trackState.Playing ? LocalUTC.now : trackProgressInfo.lastUpdateUtc;
            //changeStatus.trackIsDone     = isTrackDone(playingTrack);
            changeStatus.isLongPaused    = isTrackLongPaused(playingTrack);
            Logger.Debug("isLongPaused " + changeStatus.isLongPaused.ToString());

            changeStatus.sendSongSession = isValidExistingTrack && (changeStatus.isNewSong || changeStatus.isLongPaused) ? true : false;
            Logger.Debug("sendsongSession " +changeStatus.sendSongSession.ToString());
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
            trackProgressInfo.endInRange    = endInRange;
            trackProgressInfo.lastUpdateUtc = lastUpdatedUtc;
            trackProgressInfo.progress_ms   = playingTrack.progress_ms;
            trackProgressInfo.duration_ms   = playingTrack.duration_ms;
            trackProgressInfo.id            = playingTrack.id;

        }

        private bool isEndInRange(Track playingTrack)
        {
            double buffer = playingTrack.duration_ms * 0.07;
            return playingTrack.progress_ms >= playingTrack.duration_ms - buffer;

        }

        private bool isTrackLongPaused(Track playingTrack)
        {
            bool hasProgress    = playingTrack.progress_ms > 0 ? true : false;

            NowTime nowTime     = SoftwareCoUtil.GetNowTime();
            double pauseThreshold = 60;

            double diff = nowTime.now - trackProgressInfo.lastUpdateUtc;

            Logger.Debug(trackProgressInfo.lastUpdateUtc.ToString());
            Logger.Debug(playingTrack.state.ToString());
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
