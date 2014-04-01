using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using Landscaper.Models;

namespace Landscaper.Helpers
{
    public static class Extensions
    {
        public static bool WithinBounds(this Shape b, Point s, Point e)
        {
            double x, y, width, height;

            if (b is Line)
            {
                var bl = (b as Line);
                var bo = new Bounds(new Point(bl.X1, bl.Y1), new Point(bl.X2, bl.Y2));

                x = bo.LowerX;
                y = bo.LowerY;
                width = bo.UpperX - x;
                height = bo.UpperY - y;
            }
            else
            {
                x = Canvas.GetLeft(b) - 1;
                y = Canvas.GetTop(b) - 1;
                width = b.Width;
                height = b.Height;
            }

            return new Rect(s, e).IntersectsWith(new Rect(x, y, width, height));
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
                if (b.IsHorizontal) // Dragging horizontally
                    b.UpperY = b.LowerY;
                else if(b.IsVertical)
                    b.UpperX = b.LowerX;
            }

            return b;
        }

        public static string ToFormat(this string s, params object[] p)
        {
            return string.Format(s, p);
        }

        public static bool IsHorizontal(this Line l)
        {
            return Math.Abs(l.Y1 - l.Y2) < .1;
        }

        public static bool IsVertical(this Line l)
        {
            return Math.Abs(l.X1 - l.X2) < .1;
        }
    }
}
