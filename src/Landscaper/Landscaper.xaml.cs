using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Landscaper.Helpers;
using Landscaper.Models;
using Microsoft.Win32;

namespace Landscaper
{
    public partial class MainWindow
    {
        #region Private Properties

        private TileChoice _selectedTileChoice;
        private Point startPoint;
        private Tool selectedTool = Tool.Paint;
        private bool isDragging;

        private Rectangle selectionRectangle = new Rectangle
        {
            Width = TILE_SIZE,
            Height = TILE_SIZE,
            Stroke = new SolidColorBrush(Colors.DarkRed),
            Fill = new SolidColorBrush(Colors.Transparent)
        };


        #endregion

        #region Public Properties

        // This is the choices on the tile tab, not the tiels that make up the map (those are in in TileList)
        public ObservableCollection<TileChoice> TileChoiceList = new ObservableCollection<TileChoice>();

        public List<Tile> TileList = new List<Tile>(); 
        public List<Wall> WallList = new List<Wall>();
        public List<Door> DoorList = new List<Door>(); 


        #endregion

        #region Constants

        private const int TILE_SIZE = 32;

        private const int FLOOR_LAYER = 0;
        private const int WALL_LAYER = 1;
        private const int ITEM_FLOOR_LAYER = 2;
        private const int PEOPLE_LAYER = 3;
        private const int DOOR_LAYER = 4;
        private const int ITEMS_TOP_LAYER = 5;
        private const int SELECTION_RECTANGLE_LAYER = 6;

        #endregion

        #region Setup

        public MainWindow()
        {
            InitializeComponent();

            Canvas.SetZIndex(selectionRectangle, SELECTION_RECTANGLE_LAYER);
            Map.Children.Add(selectionRectangle);
            LoadTiles();
        }

        private void LoadTiles()
        {
            var di = new DirectoryInfo("Content/Tiles/");
            foreach (var file in di.GetFiles())
            {
                TileChoiceList.Add(new TileChoice
                {
                    Name = file.Name.Replace(file.Extension, string.Empty),
                    Image = new Image
                    {
                        Source = new BitmapImage(new Uri(file.FullName))
                    }
                });
            }
            TilesListBox.ItemsSource = TileChoiceList;
            _selectedTileChoice = TileChoiceList.FirstOrDefault();
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
                case Tool.Draw:
                    PlaceTile(startPoint);
                    return;
                case Tool.Door:
                    PlaceDoor(startPoint);
                    return;
            }

            isDragging = true;
        }

        public int MapOffsetX = 0;
        public int MapOffsetY = 0;
        public double MapZoom = 1;

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
                    MapOffsetX += (int)(startPoint.ConvertToTilePosition(TILE_SIZE).X -
                                  currentPos.ConvertToTilePosition(TILE_SIZE).X);
                    MapOffsetY += (int)(startPoint.ConvertToTilePosition(TILE_SIZE).Y - 
                                  currentPos.ConvertToTilePosition(TILE_SIZE).Y);

                    Map.RenderTransform = new TranslateTransform(MapOffsetX, MapOffsetY);
                            
                    break;
                case Tool.Paint:
                    if (leftClick) PlaceTile(startPoint, currentPos);
                    else RemoveExistingTilesBetween(startPoint, currentPos);
                    break;
                case Tool.Remove:
                    RemoveExistingTilesBetween(startPoint, currentPos);
                    RemoveWallsBetween(startPoint.ConvertToTilePosition(TILE_SIZE), currentPos.ConvertToTilePosition(TILE_SIZE));
                    break;
                case Tool.Wall:
                    PlaceWall(startPoint, currentPos);
                    break;
            }

            isDragging = false;
            selectionRectangle.Width = TILE_SIZE;
            selectionRectangle.Height = TILE_SIZE;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var end = e.GetPosition(Map).ConvertToTilePosition(TILE_SIZE);
            var start = isDragging ? startPoint.ConvertToTilePosition(TILE_SIZE) : end;
            var b = new Bounds(start, end);
                
            Canvas.SetLeft(selectionRectangle, b.LowerX);
            Canvas.SetTop(selectionRectangle, b.LowerY);

            if (isDragging)
            {
                if (selectedTool == Tool.Wall)
                    b = b.KeepBoundsWidthOf1();

                selectionRectangle.Width = b.UpperX - b.LowerX + TILE_SIZE;
                selectionRectangle.Height = b.UpperY - b.LowerY + TILE_SIZE;
            }
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
            _selectedTileChoice = (TileChoice)((ListBox) sender).SelectedItem;
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
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            var save = sfd.ShowDialog();

            isDragging = false;

            if(save ?? false)
                IO.Save(this, sfd.FileName, TILE_SIZE);
        }

        private void OnOpen(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            var open = ofd.ShowDialog();

            isDragging = false;

            if(open ?? false)
                IO.Load(this, ofd.FileName, TILE_SIZE);
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
            start = start.ConvertToTilePosition(TILE_SIZE);
            end = end.ConvertToTilePosition(TILE_SIZE);

            RemoveExistingTilesBetween(start, end);
            PlaceTilesBetween(start, end);
        }

        private void RemoveExistingTilesBetween(Point start, Point end)
        {
            var tiles = Map.Children.OfType<Rectangle>()
                .Where(x => x != selectionRectangle)
                .Where(x => x.WithinBounds(start.ConvertToTilePosition(TILE_SIZE), end.ConvertToTilePosition(TILE_SIZE)))
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
            var im = new Image {Width = TILE_SIZE, Height = TILE_SIZE, Source = _selectedTileChoice.Image.Source};
            
            for (int x = b.LowerX; x <= b.UpperX; x += TILE_SIZE)
                for (int y = b.LowerY; y <= b.UpperY; y += TILE_SIZE)
                {
                    var t = new Tile {
                        Rectangle = new Rectangle
                        {
                            Width = TILE_SIZE,
                            Height = TILE_SIZE,
                            Fill = new ImageBrush(im.Source)
                        },
                        X = x / TILE_SIZE,
                        Y = y / TILE_SIZE,
                        Name = _selectedTileChoice.Name
                    };

                    Canvas.SetLeft(t.Rectangle, x);
                    Canvas.SetTop(t.Rectangle, y);
                    Canvas.SetZIndex(t.Rectangle, FLOOR_LAYER);

                    Map.Children.Add(t.Rectangle);
                    TileList.Add(t);
                }
        }

        #endregion

        #region Walls

        public void PlaceWall(Point start, Point end)
        {
            start = start.ConvertToTilePosition(TILE_SIZE);
            end = end.ConvertToTilePosition(TILE_SIZE);
            var b = new Bounds(start, end).KeepBoundsWidthOf1();

            //hack to add last tile of wall since walls start at 0,0 but should go the full length of last tile
            if (b.LowerX == b.UpperX) b.UpperY += TILE_SIZE;
            else if (b.LowerY == b.UpperY) b.UpperX += TILE_SIZE;

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
                    b.UpperX += TILE_SIZE;

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
                        PlaceWall(new Point(b.UpperX, w.Y1), new Point(oldEnd - TILE_SIZE, w.Y1));
                    }

                    b.UpperX -= TILE_SIZE;
                }
                else
                {
                    b.UpperY += TILE_SIZE;

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
                        PlaceWall(new Point(w.X1, b.UpperY), new Point(w.X1, oldEnd - TILE_SIZE));
                    }

                    b.UpperY -= TILE_SIZE;
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
            var left = start.X % TILE_SIZE;
            var top = start.Y % TILE_SIZE;
            start = start.ConvertToTilePosition(TILE_SIZE);

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
                d.Line.Y2 += TILE_SIZE;
            }
            else // else they were closer to top, or were equal distances at which point I'm putting it here because the user is an indecisive bastard
            {
                d.Rotation = 0;
                d.Line.X2 += TILE_SIZE;
            }

            Canvas.SetZIndex(d.Line, DOOR_LAYER);
            Map.Children.Add(d.Line);
            DoorList.Add(d);
        }

        #endregion

        #region Shortcuts

        public void SelectTile(string Name)
        {
            _selectedTileChoice = TileChoiceList.FirstOrDefault(x => x.Name == Name);
        }

        #endregion
    }
}