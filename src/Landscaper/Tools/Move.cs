using System.Linq;
using System.Windows.Controls;
using System.Windows.Shapes;
using Landscaper.Helpers;
using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public class Move : ITool
    {
        public bool IsDragging { get { return true; } }
        public bool IsLine { get { return false; } }

        public void OnMouseDown(Editor editor, UIView view) { }

        public void OnMouseUp(Editor editor, UIView view)
        {
            var x = (int)(editor.startPoint.ConvertToTilePosition(Gc.TILE_SIZE).X -
                          editor.currentPoint.ConvertToTilePosition(Gc.TILE_SIZE).X);
            var y = (int)(editor.startPoint.ConvertToTilePosition(Gc.TILE_SIZE).Y -
                          editor.currentPoint.ConvertToTilePosition(Gc.TILE_SIZE).Y);

            TranslateMap(x, y, editor);
        }

        private void TranslateMap(int x, int y, Editor editor)
        {
            editor.DeselectItem();

            foreach (var child in editor.Map.Children.OfType<Shape>())
            {
                if (child is Rectangle)
                {
                    Canvas.SetLeft(child, Canvas.GetLeft(child) + x);
                    Canvas.SetTop(child, Canvas.GetTop(child) + y);

                    var t = editor.TileList.FirstOrDefault(z => z.Rectangle == child);
                    if (t != null)
                    {
                        t.X += x / Gc.TILE_SIZE;
                        t.Y += y / Gc.TILE_SIZE;
                    }
                }
                else if (child is Line)
                {
                    var c = child as Line;
                    c.X1 += x;
                    c.X2 += x;
                    c.Y1 += y;
                    c.Y2 += y;
                }
            }
        }
    }
}
