using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicTime
{
    class MusicStateManager
    {
        private static MusicStateManager instance = null;
        private static MusicManager musicManager = MusicManager.getInstance;
        private static Track ExsitingTrack;
        private static trackProgressInfo trackProgressInfo = new trackProgressInfo();
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
             
            Track playingTrack  = new Track();
            playingTrack        = await musicManager.GetCurrentTrackAsync();
            NowTime nowTime     = SoftwareCoUtil.GetNowTime();
            

        }
        public async Task<ChangeStatus> getChangeStatus(Track playingTrack,NowTime LocalUTC)
        {
            ChangeStatus changeStatus = new ChangeStatus();
            if(ExsitingTrack!=null)
            {
                bool isNewSong = ExsitingTrack.id != playingTrack.id ? true : false;
            }
            else
            {
                ExsitingTrack = playingTrack;
            }


            bool trackIsDone =  isTrackDone(playingTrack);
            bool isLongPaused = isTrackLongPaused(playingTrack);

            return changeStatus;
            
        }

        private bool isTrackLongPaused(Track playingTrack)
        {
            return true;   
        }

        private bool isTrackDone(Track playingTrack)
        {
            return true;
        }
    }
    class ChangeStatus
    {

    }
    class trackProgressInfo
    {
        public bool endInRange = false;
        public double duration_ms = 0;
        public double progress_ms = 0;
        public string id = null;
        public double lastUpdateUtc = 0;
        public string playlistId = null;
    }
}
