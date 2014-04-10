using Landscaper.Helpers;
using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public class Remove : ITool
    {
        public bool IsDragging { get { return true; } }
        public bool IsLine { get { return false; } }

        public void OnMouseUp(Editor editor, UIView view) { }

        public void OnMouseDown(Editor editor, UIView view)
        {
            editor.RemoveExistingTilesBetween(editor.startPoint, editor.currentPoint);
            editor.RemoveWallsBetween(
                editor.startPoint.ConvertToTilePosition(Gc.TILE_SIZE),
                editor.currentPoint.ConvertToTilePosition(Gc.TILE_SIZE));
        }
    }
}
