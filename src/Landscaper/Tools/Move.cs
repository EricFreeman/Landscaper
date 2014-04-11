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

            view.TranslateMap(x, y);
        }
    }
}
