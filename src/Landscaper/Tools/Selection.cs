using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public class Selection : ITool
    {
        public bool IsDragging { get { return false; } }
        public bool IsLine { get { return false; } }

        public void OnMouseUp(Editor editor, UIView view)
        {
            editor.SelectItem(editor.startPoint);
        }

        public void OnMouseDown(Editor editor, UIView view) { }
    }
}
