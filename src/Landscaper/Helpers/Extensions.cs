using System.Windows;

namespace Landscaper
{
    public static class Extensions
    {
        public static bool WithinBounds(this Rect b, Point s, Point e)
        {
            return new Rect(s, e).Contains(b) || b.Contains(new Rect(s, e));
        }

        public static Point ConvertToTilePosition(this Point p, int size)
        {
            return new Point((((int)p.X) / size * size), (((int)p.Y) / size * size));
        }
    }
}
