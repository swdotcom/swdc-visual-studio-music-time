using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicTime
{
    /// <summary>
    /// Interaction logic for LikeSongButton.xaml
    /// </summary>
    public partial class LikeSongButton : UserControl
    {
        public LikeSongButton()
        {
            InitializeComponent();           
        }

        public  async Task UpdateDisplayAsync(string label, string iconName)
        {
            await Dispatcher.BeginInvoke(new Action(() => {
               
                LikeIcon.Source = new BitmapImage(new Uri("Resources/"+ iconName, UriKind.Relative));
            }));


        }

        private  void LikeSong(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            MusicTimeCoPackage.LikeUnlikeSong();
          

        }
    }
}
