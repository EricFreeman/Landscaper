using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public class Paint : ITool
    {
        public bool IsDragging { get { return true; } }
        public bool IsLine { get { return false; } }

        public void OnMouseUp(Editor editor, UIView view) { }

        public void OnMouseDown(Editor editor, UIView view)
        {
            if (view.LeftClick) editor.PlaceTile(editor.startPoint, editor.currentPoint);
            else editor.RemoveExistingTilesBetween(editor.startPoint, editor.currentPoint);
        }
    }
}
