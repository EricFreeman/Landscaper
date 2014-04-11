using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Landscaper.Helpers;
using Landscaper.Models;
using Landscaper.Tools;
using Landscaper.Views;
using Microsoft.Win32;

namespace Landscaper
{
    public partial class MainWindow : UIView
    {
        private Editor editor;

        #region Interface Properties

        public double ItemX 
        {
            get
            {
                double t;
                double.TryParse(EditX.Text, out t);
                return t;
            }
            set { EditX.Text = value.ToString(); } 
        }

        public double ItemY
        {
            get
            {
                double t;
                double.TryParse(EditY.Text, out t);
                return t;
            }
            set { EditY.Text = value.ToString(); }
        }

        public float ItemRotation
        {
            get
            {
                float t;
                float.TryParse(EditRotation.Text, out t);
                return t;
            }
            set { EditRotation.Text = value.ToString(); }
        }

        public bool IsItemEditEnabled
        {
            get { return ItemEditor.IsEnabled; }
            set { ItemEditor.IsEnabled = value; }
        }

        public bool LeftClick { get; set; }

        #endregion

        #region Setup

        public MainWindow()
        {
            InitializeComponent();
            editor = new Editor(this, Map);

            LoadTiles();
            LoadItems();
        }

        private void LoadTiles()
        {
            IO.LoadImagesFromDirectory("Content/Tiles", editor.TileChoiceList);
            TilesListBox.ItemsSource = editor.TileChoiceList;
            editor.SelectedTileChoice = editor.TileChoiceList.FirstOrDefault();
        }

        private void LoadItems()
        {
            IO.LoadImagesFromDirectory("Content/Items", editor.ItemChoiceList);
            ItemsListBox.ItemsSource = editor.ItemChoiceList;
            editor.SelectedItemChoice = editor.ItemChoiceList.FirstOrDefault();
        }

        #endregion

        #region Helper Methods

        public void ClearItemEditor()
        {
            EditX.Text = EditY.Text = EditRotation.Text = string.Empty;
        }

        private void UpdateItemEditor(object sender, TextChangedEventArgs args)
        {
            if (editor.EditingItem == null) return;
            float TempFloat;
            if (!float.TryParse(EditX.Text, out TempFloat) ||
                !float.TryParse(EditY.Text, out TempFloat) ||
                !float.TryParse(EditRotation.Text, out TempFloat)) return;

            editor.EditingItem.X = ((float)ItemX).FromEditorCoordX(editor);
            editor.EditingItem.Y = ((float)ItemY).FromEditorCoordY(editor);
            editor.EditingItem.Rotation = ItemRotation;

            editor.EditingItem.Rectangle.RenderTransform = 
                new RotateTransform(
                    editor.EditingItem.Rotation, editor.EditingItem.Image.Width / 2,
                    editor.EditingItem.Image.Height / 2);
        }

        #endregion

        #region Mouse Handlers

        // This is a hack to stop visual studio calling OnMouseUp if you double click a file to open from menu
        public bool WasDown = false;

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            WasDown = true;
            editor.startPoint = e.GetPosition(Map);
            editor.selectedTool.OnMouseDown(editor, this);
            editor.isDragging = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!WasDown)
                return;

            WasDown = false;
            editor.currentPoint = e.GetPosition(Map);
            LeftClick = e.ChangedButton == MouseButton.Left;
            editor.selectedTool.OnMouseUp(editor, this);
            editor.isDragging = false;
            editor.selectionRectangle.Width = Gc.TILE_SIZE;
            editor.selectionRectangle.Height = Gc.TILE_SIZE;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var end = e.GetPosition(Map).ConvertToTilePosition(Gc.TILE_SIZE);
            var start = editor.isDragging ? editor.startPoint.ConvertToTilePosition(Gc.TILE_SIZE) : end;
            var b = new Bounds(start, end);

            Canvas.SetLeft(editor.selectionRectangle, b.LowerX);
            Canvas.SetTop(editor.selectionRectangle, b.LowerY);

            if (editor.isDragging)
            {
                if (editor.selectedTool.IsLine)
                    b = b.KeepBoundsWidthOf1();

                editor.selectionRectangle.Width = b.UpperX - b.LowerX + Gc.TILE_SIZE;
                editor.selectionRectangle.Height = b.UpperY - b.LowerY + Gc.TILE_SIZE;
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomSlider.Value += e.Delta/480.0;
        }

        #endregion

        #region Toolbar

        private void OnToolbarSelect(object sender, MouseButtonEventArgs e)
        {
            var b = (ToggleButton) sender;
            editor.selectedTool = (ITool)Assembly.GetExecutingAssembly().CreateInstance("Landscaper.Tools." + b.Name);
        }

        private void SelectNewTileBrush(object sender, MouseButtonEventArgs e)
        {
            editor.SelectedTileChoice = (ItemChoice)((ListBox)sender).SelectedItem;
        }

        private void SelectNewItemBrush(object sender, MouseButtonEventArgs e)
        {
            editor.SelectedItemChoice = (ItemChoice)((ListBox)sender).SelectedItem;
        }

        private void ZoomSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(Map != null) // because we like firing events while loading still, right wpf?
                Map.LayoutTransform = new ScaleTransform(e.NewValue, e.NewValue);
        }

        private void ZoomSlider_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ZoomSlider.Value = 1;
        }

        #endregion

        #region Menu Handlers

        private void OnNew(object sender, RoutedEventArgs e)
        {
            editor.Clear();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            var save = sfd.ShowDialog();

            editor.isDragging = false;

            if(save ?? false)
                IO.Save(editor, sfd.FileName);
        }

        private void OnOpen(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            var open = ofd.ShowDialog();

            editor.isDragging = false;

            if(open ?? false)
                IO.Load(editor, ofd.FileName);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}