using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Landscaper.Helpers
{
    public static class Extensions
    {
        public static bool WithinBounds(this Rectangle b, Point s, Point e)
        {
            return new Rect(s, e).IntersectsWith(new Rect(Canvas.GetLeft(b) - 1, Canvas.GetTop(b) - 1, b.Width, b.Height));
        }

        public static Point ConvertToTilePosition(this Point p, int size)
        {
            return new Point((((int)p.X) / size * size), (((int)p.Y) / size * size));
        }
    }
}
