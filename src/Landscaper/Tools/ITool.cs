using Landscaper.Models;
using Landscaper.Views;

namespace Landscaper.Tools
{
    public interface ITool
    {
        bool IsDragging { get; }
        bool IsLine { get; }

        void OnMouseDown(Editor e, UIView view);
        void OnMouseUp(Editor editor, UIView view);
    }
}
