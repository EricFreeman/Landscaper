using System.Windows;

namespace Landscaper.Models
{
    public class Bounds
    {
        public int LowerX;
        public int UpperX;
        public int LowerY;
        public int UpperY;

        public Bounds(Point start, Point end)
        {
            LowerX = (int)(start.X < end.X ? start.X : end.X);
            UpperX = (int)(start.X > end.X ? start.X : end.X);
            LowerY = (int)(start.Y < end.Y ? start.Y : end.Y);
            UpperY = (int)(start.Y > end.Y ? start.Y : end.Y);
        }
    }
}
