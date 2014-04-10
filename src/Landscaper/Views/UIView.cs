namespace Landscaper.Views
{
    public interface UIView
    {
        double ItemX { get; set; }
        double ItemY { get; set; }
        float ItemRotation { get; set; }
        bool IsItemEditEnabled { get; set; }

        bool LeftClick { get; set; }

        void ClearItemEditor();
        void TranslateMap(int x, int y);
    }
}
