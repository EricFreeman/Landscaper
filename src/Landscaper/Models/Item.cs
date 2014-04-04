using System.Windows.Controls;
using System.Windows.Shapes;

namespace Landscaper.Models
{
    public class Item
    {
        public Rectangle Rectangle;
        public Image Image;

        public double X
        {
            get { return Canvas.GetLeft(Rectangle) - (Image != null ? Image.Width : 0) / 2; }
            set { Canvas.SetLeft(Rectangle, value + (Image != null ? Image.Width : 0) / 2); }
        }
        public double Y
        {
            get { return Canvas.GetTop(Rectangle) - (Image != null ? Image.Height : 0) / 2; }
            set { Canvas.SetTop(Rectangle, value + (Image != null ? Image.Height : 0) / 2); }
        }
        public float Scale;
        public float Rotation;
        public string Name;
    }
}
