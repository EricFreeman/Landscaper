using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public interface ITool
    {
        bool IsDragging { get; }
        bool IsLine { get; }

        void OnMouseUp(Editor editor, UIView view);
        void OnMouseDown(Editor e, UIView view);
    }
}
