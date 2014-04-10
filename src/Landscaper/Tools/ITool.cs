using Landscaper.Models;

namespace Landscaper.Tools
{
    public interface ITool
    {
        void OnMouseUp(Editor e);
        void OnMouseDown(Editor e);
    }
}
