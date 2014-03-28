using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        #endregion

        #region Constants

        private const int TILE_SIZE = 16;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            Map.Children.Add(selectionRectangle);
        }

        #endregion

        #region Mouse Handlers

        private void OnMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

        private void OnMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var currentPos = e.GetPosition(Map);

            switch (selectedTool)
            {
                case Tool.Paint:
                    PlaceTile(startPoint, currentPos);
                    break;
            }

            isDragging = false;
            selectionRectangle.Width = TILE_SIZE;
            selectionRectangle.Height = TILE_SIZE;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var end = e.GetPosition(Map).ConvertToTilePosition(TILE_SIZE);
            var start = isDragging ? startPoint.ConvertToTilePosition(TILE_SIZE) : end;
            var b = new Bounds(start, end);
                
            Canvas.SetLeft(selectionRectangle, b.LowerX);
            Canvas.SetTop(selectionRectangle, b.LowerY);

            if (isDragging)
            {
                selectionRectangle.Width =  b.UpperX - b.LowerX + TILE_SIZE;
                selectionRectangle.Height = b.UpperY - b.LowerY + TILE_SIZE;
            }
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
            var tiles = Map.Children.OfType<Rectangle>().Where(x => x.RenderedGeometry.Bounds.WithinBounds(start, end));
            
            foreach (var tile in tiles)
                Map.Children.Remove(tile);
        }

        private void PlaceTilesBetween(Point start, Point end)
        {
            var b = new Bounds(start, end);

            for (int x = b.LowerX; x <= b.UpperX; x += TILE_SIZE)
                for (int y = b.LowerY; y <= b.UpperY; y += TILE_SIZE)
                {
                    var im = new Image {Width = TILE_SIZE, Height = TILE_SIZE, Source = (ImageSource) FindResource("1")};
                    var rec = new Rectangle
                    {
                        Width = TILE_SIZE,
                        Height = TILE_SIZE,
                        Fill = new ImageBrush(im.Source)
                    };

                    Canvas.SetLeft(rec, x);
                    Canvas.SetTop(rec, y);
                    Map.Children.Add(rec);
                }
        }

        #endregion
    }
}