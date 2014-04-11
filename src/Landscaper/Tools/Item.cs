using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public class Item : ITool
    {
        public bool IsDragging { get { return false; } }
        public bool IsLine { get { return false; } }

        public void OnMouseDown(Editor editor, UIView view)
        {
            editor.PlaceItem(editor.startPoint);
        }

        public void OnMouseUp(Editor editor, UIView view) { }
    }
}
