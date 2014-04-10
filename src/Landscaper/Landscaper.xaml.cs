using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Landscaper.Helpers;
using Landscaper.Models;
using Landscaper.Views;
using Microsoft.Win32;

namespace Landscaper
{
    public partial class MainWindow : UIView
    {
        private Editor editor;

        #region Interface Properties

        public double ItemX { get; set; }
        public double ItemY { get; set; }
        public float ItemRotation { get; set; }
        public bool IsItemEditEnabled { get; set; }

        #endregion

        #region Setup

        public MainWindow()
        {
            InitializeComponent();
            editor = new Editor(Map);

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

        #region Mouse Handlers

        public bool WasDown = false;

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            WasDown = true;
            editor.startPoint = e.GetPosition(Map);

            switch (editor.selectedTool)
            {
                case Tool.Selection:
                    editor.SelectItem(editor.startPoint);
                    return;
                case Tool.Item:
                    editor.PlaceItem(editor.startPoint);
                    return;
                case Tool.Door:
                    editor.PlaceDoor(editor.startPoint);
                    return;
            }

            editor.isDragging = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!WasDown)
                return;

            WasDown = false;

            var currentPos = e.GetPosition(Map);

            var leftClick = e.ChangedButton == MouseButton.Left;

            switch (editor.selectedTool)
            {
                case Tool.Move:
                    var x = (int)(editor.startPoint.ConvertToTilePosition(Gc.TILE_SIZE).X -
                                  currentPos.ConvertToTilePosition(Gc.TILE_SIZE).X);
                    var y = (int)(editor.startPoint.ConvertToTilePosition(Gc.TILE_SIZE).Y - 
                                  currentPos.ConvertToTilePosition(Gc.TILE_SIZE).Y);

                    TranslateMap(x, y);
                           
                    break;
                case Tool.Paint:
                    if (leftClick) editor.PlaceTile(editor.startPoint, currentPos);
                    else editor.RemoveExistingTilesBetween(editor.startPoint, currentPos);
                    break;
                case Tool.Remove:
                    editor.RemoveExistingTilesBetween(editor.startPoint, currentPos);
                    editor.RemoveWallsBetween(editor.startPoint.ConvertToTilePosition(Gc.TILE_SIZE), currentPos.ConvertToTilePosition(Gc.TILE_SIZE));
                    break;
                case Tool.Wall:
                    editor.PlaceWall(editor.startPoint, currentPos);
                    break;
            }

            editor.isDragging = false;
            editor.selectionRectangle.Width = Gc.TILE_SIZE;
            editor.selectionRectangle.Height = Gc.TILE_SIZE;
        }

        private void TranslateMap(int x, int y)
        {
            editor.DeselectItem();

            foreach (var child in Map.Children.OfType<Shape>())
            {
                if (child is Rectangle)
                {
                    Canvas.SetLeft(child, Canvas.GetLeft(child) + x);
                    Canvas.SetTop(child, Canvas.GetTop(child) + y);

                    var t = editor.TileList.FirstOrDefault(z => z.Rectangle == child);
                    if (t != null)
                    {
                        t.X += x / Gc.TILE_SIZE;
                        t.Y += y / Gc.TILE_SIZE;
                    }
                }
                else if (child is Line)
                {
                    var c = child as Line;
                    c.X1 += x;
                    c.X2 += x;
                    c.Y1 += y;
                    c.Y2 += y;
                }
            }
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
                if (editor.selectedTool == Tool.Wall)
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
            Enum.TryParse(b.Name, out editor.selectedTool);
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