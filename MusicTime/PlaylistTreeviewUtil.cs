using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MusicTime
{
    class PlaylistTreeviewUtil
    {
        public static PlaylistTreeviewItem GetSelectedTreeViewItemParent(PlaylistTreeviewItem item)
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
            catch (Exception ex)
            {


            }
            return parent as PlaylistTreeviewItem;

        }
      

        public static TreeViewItem GetTreeView(string text, string imagePath, string id)
        {
            PlaylistTreeviewItem item = new PlaylistTreeviewItem(id);

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            if (!string.IsNullOrEmpty(imagePath))
            {
                // create Image
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                image.Source = new BitmapImage(new Uri("Resources/" + imagePath, UriKind.Relative));
                stack.Children.Add(image);
            }
            // Label

            Label lbl = new Label();
            lbl.Content = text;
            lbl.Foreground = System.Windows.Media.Brushes.DarkCyan;

            // Add into stack

            stack.Children.Add(lbl);

            // assign stack to header
            item.Header = stack;
            return item;
        }

        public static TreeViewItem GetTrackTreeView(string text, string imagePath, string id)
        {
            PlaylistTreeviewItem item = new PlaylistTreeviewItem(id);

            // create stack panel
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;

            // create Image
            System.Windows.Controls.Image image = new System.Windows.Controls.Image();
            image.Source = new BitmapImage(new Uri("Resources/" + imagePath, UriKind.Relative));

            // Label
            Label lbl = new Label();
            lbl.Content = ResizeSongName(text);
            // lbl.Content = text;
            lbl.Width = 150;

            lbl.Foreground = System.Windows.Media.Brushes.DarkCyan;
           
            // Add into stack
            
            
            stack.Children.Add(image);
            stack.Children.Add(lbl);
            // assign stack to header
            item.Header = stack;
            item.Background = System.Windows.Media.Brushes.Transparent;
            return item;
        }
        
        public static string ResizeSongName(string text)
        {
            string result = string.Empty;
            if (text.Length > 20)
            {
                result = string.Concat(text.Substring(0, 40), "...");
            }
            else
            {
                result = text;
            }
            return result;
        }

       
    }
}
