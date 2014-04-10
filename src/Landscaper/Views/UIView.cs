namespace Landscaper.Views
{
    public interface UIView
    {
        double ItemX { get; set; }
        double ItemY { get; set; }
        float ItemRotation { get; set; }
        bool IsItemEditEnabled { get; set; }

        void ClearItemEditor();
    }
}
