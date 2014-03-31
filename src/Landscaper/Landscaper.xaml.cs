using System;
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
            Width = TILE_SIZE,
            Height = TILE_SIZE,
            Stroke = new SolidColorBrush(Colors.DarkRed),
            Fill = new SolidColorBrush(Colors.Transparent)
        };

        public ObservableCollection<Tile> TileList = new ObservableCollection<Tile>();
        private Tile _selectedTile;

        #endregion

        #region Constants

        private const int TILE_SIZE = 32;

        private const int FLOOR_LAYER = 0;
        private const int WALL_LAYER = 1;
        private const int ITEM_FLOOR_LAYER = 2;
        private const int PEOPLE_LAYER = 3;
        private const int ITEMS_TOP_LAYER = 4;
        private const int SELECTION_RECTANGLE_LAYER = 5;

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
                TileList.Add(new Tile
                {
                    Name = file.Name.Replace(file.Extension, string.Empty),
                    Image = new Image
                    {
                        Source = new BitmapImage(new Uri(file.FullName))
                    }
                });
            }
            TilesListBox.ItemsSource = TileList;
            _selectedTile = TileList.FirstOrDefault();
        }

        #endregion

        #region Mouse Handlers

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(Map);

            switch (selectedTool)
            {
                case Tool.Draw:
                    PlaceTile(startPoint);
                    return;
            }

            isDragging = true;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var currentPos = e.GetPosition(Map);

            switch (selectedTool)
            {
                case Tool.Paint:
                    PlaceTile(startPoint, currentPos);
                    break;
                case Tool.Remove:
                    RemoveExistingTilesBetween(startPoint, currentPos);
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
            _selectedTile = (Tile)((ListBox) sender).SelectedItem;
        }

        #endregion

        #region Placing Tiles

        private void PlaceTile(Point start)
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
            
            for(int i = tiles.Count() - 1; i >= 0; i--)
                Map.Children.Remove(tiles[i]);
        }

        private void PlaceTilesBetween(Point start, Point end)
        {
            var b = new Bounds(start, end);

            for (int x = b.LowerX; x <= b.UpperX; x += TILE_SIZE)
                for (int y = b.LowerY; y <= b.UpperY; y += TILE_SIZE)
                {
                    var im = new Image {Width = TILE_SIZE, Height = TILE_SIZE, Source = _selectedTile.Image.Source};
                    var rec = new Rectangle
                    {
                        Width = TILE_SIZE,
                        Height = TILE_SIZE,
                        Fill = new ImageBrush(im.Source)
                    };

                    Canvas.SetLeft(rec, x);
                    Canvas.SetTop(rec, y);
                    Canvas.SetZIndex(rec, FLOOR_LAYER);
                    Map.Children.Add(rec);
                }
        }

        #endregion

        #region Placing Walls

        public void PlaceWall(Point start, Point end)
        {
            start = start.ConvertToTilePosition(TILE_SIZE);
            end = end.ConvertToTilePosition(TILE_SIZE);
            var b = new Bounds(start, end).KeepBoundsWidthOf1();

            //hack to add last tile of wall since walls start at 0,0 but should go the full length of last tile
            if (b.LowerX == b.UpperX) b.UpperY += TILE_SIZE;
            else if (b.LowerY == b.UpperY) b.UpperX += TILE_SIZE;

            var l = new Line
            {
                X1 = b.LowerX,
                Y1 = b.LowerY,
                X2 = b.UpperX,
                Y2 = b.UpperY,
                Stroke = new SolidColorBrush(Colors.Gray),
                StrokeThickness = 3
            };

            Canvas.SetZIndex(l, WALL_LAYER);
            Map.Children.Add(l);
        }

        #endregion
    }
}