using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Landscaper.Models;

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

        /// <summary>
        /// Hack to keep walls either horizontal or diagonal
        /// </summary>
        /// <param name="b">The 'Bounds' object you're applying this function to</param>
        /// <returns></returns>
        public static Bounds KeepBoundsWidthOf1(this Bounds b)
        {
            if (b.LowerX != b.UpperX && b.LowerY != b.UpperY)
            {
                // First find longest distance because that's the way they're dragging
                var hor = b.UpperX - b.LowerX;
                var ver = b.UpperY - b.LowerY;

                if (hor > ver) // Dragging horizontally
                    b.UpperY = b.LowerY;
                else
                    b.UpperX = b.LowerX;
            }

            return b;
        }
    }
}
