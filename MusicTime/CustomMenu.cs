using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MusicTime
{
    public class CustomMenu : MenuItem
    {
        public string PlaylistId { get; set; }
        public string SlackChannelId { get; set; }
    }
    public class DeviceContextMenu : MenuItem
    {
        public string deviceId { get; set; }
        public bool isActive { get; set; }
        public string deviceName { get; set; }
        public string playlist_id { get; set; }
        public string track_id { get; set; }
    }
}
