using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Landscaper.Helpers;
using Landscaper.Models;
using Microsoft.Win32;

namespace Landscaper
{
    public partial class MainWindow
    {
        #region Private Properties

        private Point startPoint;
        private Tool selectedTool = Tool.Paint;
        private bool isDragging;

        private Rectangle selectionRectangle = new Rectangle
        {
            Width = Gc.TILE_SIZE,
            Height = Gc.TILE_SIZE,
            Stroke = new SolidColorBrush(Colors.DarkRed),
            Fill = new SolidColorBrush(Colors.Transparent)
        };


        #endregion

        #region Public Properties

        // This is the choices on the tile tab, not the tiles that make up the map (those are in in TileList)
        public ObservableCollection<ItemChoice> TileChoiceList = new ObservableCollection<ItemChoice>();
        public ObservableCollection<ItemChoice> ItemChoiceList = new ObservableCollection<ItemChoice>();

        public List<Tile> TileList = new List<Tile>(); 
        public List<Wall> WallList = new List<Wall>();
        public List<Door> DoorList = new List<Door>(); 
        public List<Item> ItemList = new List<Item>();

        public ItemChoice SelectedTileChoice;
        public ItemChoice SelectedItemChoice;

        #endregion

        #region Constants

        private const int FLOOR_LAYER = 0;
        private const int ITEM_LOWER_FLOOR_LAYER = 1;
        private const int ITEM_MID_FLOOR_LAYER = 2;
        private const int ITEM_UPPER_FLOOR_LAYER = 3;
        private const int WALL_LAYER = 4;
        private const int PEOPLE_LAYER = 5;
        private const int DOOR_LAYER = 6;
        private const int ITEMS_TOP_LAYER = 7;
        private const int SELECTION_RECTANGLE_LAYER = 8;

        #endregion

        #region Setup

        public MainWindow()
        {
            InitializeComponent();

            Canvas.SetZIndex(selectionRectangle, SELECTION_RECTANGLE_LAYER);
            Map.Children.Add(selectionRectangle);
            LoadTiles();
            LoadItems();
        }

        private void LoadTiles()
        {
            IO.LoadImagesFromDirectory("Content/Tiles", TileChoiceList);
            TilesListBox.ItemsSource = TileChoiceList;
            SelectedTileChoice = TileChoiceList.FirstOrDefault();
        }

        private void LoadItems()
        {
            IO.LoadImagesFromDirectory("Content/Items", ItemChoiceList);
            ItemsListBox.ItemsSource = ItemChoiceList;
            SelectedItemChoice = ItemChoiceList.FirstOrDefault();
        }

        #endregion

        #region Mouse Handlers

        public bool WasDown = false;

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            WasDown = true;
            startPoint = e.GetPosition(Map);

            switch (selectedTool)
            {
                case Tool.Selection:
                    SelectItem(startPoint);
                    return;
                case Tool.Item:
                    PlaceItem(startPoint);
                    return;
                case Tool.Door:
                    PlaceDoor(startPoint);
                    return;
            }

            isDragging = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!WasDown)
                return;

            WasDown = false;

            var currentPos = e.GetPosition(Map);

            var leftClick = e.ChangedButton == MouseButton.Left;

            switch (selectedTool)
            {
                case Tool.Move:
                    var x = (int)(startPoint.ConvertToTilePosition(Gc.TILE_SIZE).X -
                                  currentPos.ConvertToTilePosition(Gc.TILE_SIZE).X);
                    var y = (int)(startPoint.ConvertToTilePosition(Gc.TILE_SIZE).Y - 
                                  currentPos.ConvertToTilePosition(Gc.TILE_SIZE).Y);

                    TranslateMap(x, y);
                           
                    break;
                case Tool.Paint:
                    if (leftClick) PlaceTile(startPoint, currentPos);
                    else RemoveExistingTilesBetween(startPoint, currentPos);
                    break;
                case Tool.Remove:
                    RemoveExistingTilesBetween(startPoint, currentPos);
                    RemoveWallsBetween(startPoint.ConvertToTilePosition(Gc.TILE_SIZE), currentPos.ConvertToTilePosition(Gc.TILE_SIZE));
                    break;
                case Tool.Wall:
                    PlaceWall(startPoint, currentPos);
                    break;
            }

            isDragging = false;
            selectionRectangle.Width = Gc.TILE_SIZE;
            selectionRectangle.Height = Gc.TILE_SIZE;
        }

        private void TranslateMap(int x, int y)
        {
            DeselectItem();

            foreach (var child in Map.Children.OfType<Shape>())
            {
                if (child is Rectangle)
                {
                    Canvas.SetLeft(child, Canvas.GetLeft(child) + x);
                    Canvas.SetTop(child, Canvas.GetTop(child) + y);

                    var t = TileList.FirstOrDefault(z => z.Rectangle == child);
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
            var start = isDragging ? startPoint.ConvertToTilePosition(Gc.TILE_SIZE) : end;
            var b = new Bounds(start, end);
                
            Canvas.SetLeft(selectionRectangle, b.LowerX);
            Canvas.SetTop(selectionRectangle, b.LowerY);

            if (isDragging)
            {
                if (selectedTool == Tool.Wall)
                    b = b.KeepBoundsWidthOf1();

                selectionRectangle.Width = b.UpperX - b.LowerX + Gc.TILE_SIZE;
                selectionRectangle.Height = b.UpperY - b.LowerY + Gc.TILE_SIZE;
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
            Enum.TryParse(b.Name, out selectedTool);
        }

        private void SelectNewTileBrush(object sender, MouseButtonEventArgs e)
        {
            SelectedTileChoice = (ItemChoice)((ListBox) sender).SelectedItem;
        }

        private void SelectNewItemBrush(object sender, MouseButtonEventArgs e)
        {
            SelectedItemChoice = (ItemChoice)((ListBox)sender).SelectedItem;
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
            Clear();
        }

        public void Clear()
        {
            Map.Children.Clear();
            Map.Children.Add(selectionRectangle);
            WallList.Clear();
            DoorList.Clear();
            TileList.Clear();
            ItemList.Clear();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            var save = sfd.ShowDialog();

            isDragging = false;

            if(save ?? false)
                IO.Save(this, sfd.FileName);
        }

        private void OnOpen(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            var open = ofd.ShowDialog();

            isDragging = false;

            if(open ?? false)
                IO.Load(this, ofd.FileName);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Tiles

        public void PlaceTile(Point start)
        {
            PlaceTile(start, start);
        }

        private void PlaceTile(Point start, Point end)
        {
            start = start.ConvertToTilePosition(Gc.TILE_SIZE);
            end = end.ConvertToTilePosition(Gc.TILE_SIZE);

            RemoveExistingTilesBetween(start, end);
            PlaceTilesBetween(start, end);
        }

        private void RemoveExistingTilesBetween(Point start, Point end)
        {
            var tiles = Map.Children.OfType<Rectangle>()
                .Where(x => x != selectionRectangle)
                .Where(x => x.WithinBounds(start.ConvertToTilePosition(Gc.TILE_SIZE), end.ConvertToTilePosition(Gc.TILE_SIZE)))
                .ToList();

            for (int i = tiles.Count() - 1; i >= 0; i--)
            {
                Map.Children.Remove(tiles[i]);
                TileList.Remove(TileList.FirstOrDefault(x => x.Rectangle == tiles[i]));
            }
        }

        private void PlaceTilesBetween(Point start, Point end)
        {
            var b = new Bounds(start, end);
            var im = new Image {Width = Gc.TILE_SIZE, Height = Gc.TILE_SIZE, Source = SelectedTileChoice.Image.Source};
            
            for (int x = b.LowerX; x <= b.UpperX; x += Gc.TILE_SIZE)
                for (int y = b.LowerY; y <= b.UpperY; y += Gc.TILE_SIZE)
                {
                    var t = new Tile {
                        Rectangle = new Rectangle
                        {
                            Width = Gc.TILE_SIZE,
                            Height = Gc.TILE_SIZE,
                            Fill = new ImageBrush(im.Source)
                        },
                        X = x / Gc.TILE_SIZE,
                        Y = y / Gc.TILE_SIZE,
                        Name = SelectedTileChoice.Name
                    };

                    Canvas.SetLeft(t.Rectangle, x);
                    Canvas.SetTop(t.Rectangle, y);
                    Canvas.SetZIndex(t.Rectangle, FLOOR_LAYER);

                    Map.Children.Add(t.Rectangle);
                    TileList.Add(t);
                }
        }

        #endregion

        #region Items

        public Item EditingItem { get; set; }

        private void SelectItem(Point start)
        {
            DeselectItem();

            foreach (var item in Map.Children.OfType<Rectangle>().Where(x => ItemList.Contains(ItemList.FirstOrDefault(y => y.Rectangle == x)))) // get all items on canvas TODO: Make this not so stupid
            {
                if (item.WithinBounds(start, start))
                {
                    EditingItem = ItemList.FirstOrDefault(x => x.Rectangle == item);
                    SetItemEditor();
                    return;
                }
            }
        }

        private void DeselectItem()
        {
            EditingItem = null;
            ItemEditor.IsEnabled = false;
            ClearItemEditor();
        }

        private void ClearItemEditor()
        {
            EditX.Text = EditY.Text = EditRotation.Text = string.Empty;
        }

        public void SetItemEditor()
        {
            if (EditingItem == null) return;

            EditX.Text = EditingItem.X.ToEditorCoordX(this).ToString();
            EditY.Text = EditingItem.Y.ToEditorCoordY(this).ToString();
            EditRotation.Text = EditingItem.Rotation.ToString();
            ItemEditor.IsEnabled = true;
        }

        private void UpdateItemEditor(object sender, TextChangedEventArgs args)
        {
            float x, y, rot;
            if (!float.TryParse(EditX.Text, out x)) return;
            if (!float.TryParse(EditY.Text, out y)) return;
            if (!float.TryParse(EditRotation.Text, out rot)) return;

            EditingItem.X = x.FromEditorCoordX(this);
            EditingItem.Y = y.FromEditorCoordY(this);
            EditingItem.Rotation = rot;

            EditingItem.Rectangle.RenderTransform = new RotateTransform(EditingItem.Rotation, EditingItem.Image.Width / 2, EditingItem.Image.Height / 2);
        }

        public void PlaceItem(Point start)
        {
            var im = new Image
            {
                Width = SelectedItemChoice.Image.Source.Width,
                Height = SelectedItemChoice.Image.Source.Height,
                Source = SelectedItemChoice.Image.Source,
            };

            var t = new Item
            {
                Rectangle = new Rectangle
                {
                    Width = im.Width,
                    Height = im.Height,
                    Fill = new ImageBrush(im.Source)
                },
                X = (float)start.X,
                Y = (float)start.Y,
                Name = SelectedItemChoice.Name,
                Image = im
            };

            Canvas.SetLeft(t.Rectangle, t.X);    // have to do width / 2 because unity origin is center of object
            Canvas.SetTop(t.Rectangle, t.Y);
            Canvas.SetZIndex(t.Rectangle, ITEM_MID_FLOOR_LAYER);

            Map.Children.Add(t.Rectangle);
            ItemList.Add(t);
            EditingItem = t;
        }

        public void PlaceItem(Point start, float rot)
        {
            var im = new Image
            {
                Width = SelectedItemChoice.Image.Source.Width,
                Height = SelectedItemChoice.Image.Source.Height,
                Source = SelectedItemChoice.Image.Source,
            };

            var t = new Item
            {
                Rectangle = new Rectangle
                {
                    Width = im.Width,
                    Height = im.Height,
                    Fill = new ImageBrush(im.Source)
                },
                X = (float)start.X,
                Y = (float)start.Y,
                Name = SelectedItemChoice.Name,
                Image = im
            };

            Canvas.SetLeft(t.Rectangle, t.X + im.Width);    // have to do width / 2 because unity origin is center of object
            Canvas.SetTop(t.Rectangle, t.Y + im.Height);
            Canvas.SetZIndex(t.Rectangle, ITEM_MID_FLOOR_LAYER);

            t.Rectangle.RenderTransform = new RotateTransform(rot, t.Image.Source.Width / 2, t.Image.Source.Height / 2);

            Map.Children.Add(t.Rectangle);
            ItemList.Add(t);
            EditingItem = t;
        }

        #endregion

        #region Walls

        public void PlaceWall(Point start, Point end)
        {
            start = start.ConvertToTilePosition(Gc.TILE_SIZE);
            end = end.ConvertToTilePosition(Gc.TILE_SIZE);
            var b = new Bounds(start, end).KeepBoundsWidthOf1();

            //hack to add last tile of wall since walls start at 0,0 but should go the full length of last tile
            if (b.LowerX == b.UpperX) b.UpperY += Gc.TILE_SIZE;
            else if (b.LowerY == b.UpperY) b.UpperX += Gc.TILE_SIZE;

            var w = new Wall
            {
                Line = new Line
                {
                    X1 = b.LowerX,
                    Y1 = b.LowerY,
                    X2 = b.UpperX,
                    Y2 = b.UpperY,
                    Stroke = new SolidColorBrush(Colors.Gray),
                    StrokeThickness = 3
                },
            };

            Canvas.SetZIndex(w.Line, WALL_LAYER);
            Map.Children.Add(w.Line);
            WallList.Add(w);
        }

        public void RemoveWallOrDoorByLine(Line l)
        {
            Map.Children.Remove(l);
            WallList.Remove(WallList.FirstOrDefault(x => x.Line == l));
            DoorList.Remove(DoorList.FirstOrDefault(x => x.Line == l));
        }

        //TODO: Future Eric, make this not so duplicated and unreadable
        private void RemoveWallsBetween(Point start, Point end)
        {
            var walls = Map.Children.OfType<Line>()
                .Where(x => x.WithinBounds(start, end))
                .ToList();

            var b = new Bounds(start, end); // removal bounds

            for (int i = walls.Count() - 1; i >= 0; i--)
            {
                var w = walls[i];
                if (Math.Abs(w.X2 - w.X1) > Math.Abs(w.Y2 - w.Y1)) // is a horizontal wall
                {
                    b.UpperX += Gc.TILE_SIZE;

                    if (b.LowerX <= w.X1 && b.UpperX <= w.X2) // covers only left side of wall
                        w.X1 = b.UpperX;
                    else if (b.LowerX <= w.X2 && b.LowerX > w.X1 && b.UpperX >= w.X2) // covers only right side of wall
                        w.X2 = b.LowerX;
                    else if(b.LowerX <= w.X1 && b.UpperX >= w.X2) // covers entire wall
                        RemoveWallOrDoorByLine(w);
                    else if(b.LowerX > w.X1 && b.UpperX < w.X2) // covers middle of wall
                    {
                        var oldEnd = w.X2;
                        w.X2 = b.LowerX;
                        PlaceWall(new Point(b.UpperX, w.Y1), new Point(oldEnd - Gc.TILE_SIZE, w.Y1));
                    }

                    b.UpperX -= Gc.TILE_SIZE;
                }
                else
                {
                    b.UpperY += Gc.TILE_SIZE;

                    if (b.LowerY <= w.Y1 && b.UpperY <= w.Y2) // covers only left side of wall
                        w.Y1 = b.UpperY;
                    else if (b.LowerY <= w.Y2 && b.LowerY > w.Y1 && b.UpperY >= w.Y2) // covers only right side of wall
                        w.Y2 = b.LowerY;
                    else if (b.LowerY <= w.Y1 && b.UpperY >= w.Y2) // covers entire wall
                        RemoveWallOrDoorByLine(w);
                    else if (b.LowerY > w.Y1 && b.UpperY < w.Y2) // covers middle of wall
                    {
                        var oldEnd = w.Y2;
                        w.Y2 = b.LowerY;
                        PlaceWall(new Point(w.X1, b.UpperY), new Point(w.X1, oldEnd - Gc.TILE_SIZE));
                    }

                    b.UpperY -= Gc.TILE_SIZE;
                }

                if (w.X1 - w.X2 == 0 && w.Y1 - w.Y2 == 0)
                        RemoveWallOrDoorByLine(w);

            }
        }

        #endregion

        #region Doors

        public void PlaceDoor(Point start)
        {
            //figure out if they meant to put door on left or top of tile
            var left = start.X % Gc.TILE_SIZE;
            var top = start.Y % Gc.TILE_SIZE;
            start = start.ConvertToTilePosition(Gc.TILE_SIZE);

            var d = new Door
            {
                Line = new Line
                {
                    Stroke = new SolidColorBrush(Colors.DarkGray),
                    StrokeThickness = 5,
                    X1 = start.X,
                    X2 = start.X,
                    Y1 = start.Y,
                    Y2 = start.Y
                }
            };

            if (left < top) // they were closer to left side of tile
            {
                d.Rotation = 90;
                d.Line.Y2 += Gc.TILE_SIZE;
            }
            else // else they were closer to top, or were equal distances at which point I'm putting it here because the user is an indecisive bastard
            {
                d.Rotation = 0;
                d.Line.X2 += Gc.TILE_SIZE;
            }

            Canvas.SetZIndex(d.Line, DOOR_LAYER);
            Map.Children.Add(d.Line);
            DoorList.Add(d);
        }

        #endregion

        #region Shortcuts

        public void SelectTileChoice(string Name)
        {
            SelectedTileChoice = TileChoiceList.FirstOrDefault(x => x.Name == Name);
        }

        public void SelectItemChoice(string Name)
        {
            SelectedItemChoice = ItemChoiceList.FirstOrDefault(x => x.Name == Name);
        }

        #endregion
    }
}