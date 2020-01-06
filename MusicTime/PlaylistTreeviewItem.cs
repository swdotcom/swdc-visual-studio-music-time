using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MusicTime
{
    class PlaylistTreeviewItem : TreeViewItem
    {
        public string TrackId { get; set; }
        public string TrackName { get; set; }
        public string PlayListId { get; set; }
        public PlaylistTreeviewItem(string PlaylistId, string TrackId ,string TrackName)
        {
            this.PlayListId = PlaylistId;
            this.TrackId    = TrackId;
            this.TrackName  = TrackName;
        }
    }
}
