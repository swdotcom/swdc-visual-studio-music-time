using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MusicTime
{
    public class PlaylistTreeviewItem : TreeViewItem
    {
        
        public string PlayListId { get; set; }
        public PlaylistTreeviewItem(string PlaylistId)
        {
            PlayListId = PlaylistId;
            
        }
    }

}
