using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public class Wall : ITool
    {
        public bool IsDragging { get { return true; } }
        public bool IsLine { get { return true; } }

        public void OnMouseUp(Editor editor, UIView view) { }

        public void OnMouseDown(Editor editor, UIView view)
        {
            editor.PlaceWall(editor.startPoint, editor.currentPoint);
        }
    }
}
