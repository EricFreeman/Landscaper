using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public class Door : ITool
    {
        public bool IsDragging { get { return false; } }
        public bool IsLine { get { return false; } }

        public void OnMouseDown(Editor editor, UIView view)
        {
            editor.PlaceDoor(editor.startPoint);
        }

        public void OnMouseUp(Editor editor, UIView view) { }
    }
}
