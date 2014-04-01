using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace Landscaper.Helpers
{
    public static class IO
    {
        private const double defaultLastTop = -99999999999999d;

        //TODO: Support levels that aren't perfect squares...maybe?  Maybe I don't care because this is a poc anyways...
        public static void Save(MainWindow editor, string fileLoc, int ts)
        {
            var offsetX = editor.TileList.Min(x => x.X) * ts;
            var offsetY = editor.TileList.Max(x => x.Y) * ts; // max because 0,0 in unity is botom left of screen instead of top left

            var file = new XmlDocument();

            string f = "<Level>";

            var lastTop = defaultLastTop;
            foreach (var tile in editor.TileList.OrderByDescending(x => x.Y).ThenBy(x => x.X))
            {
                var top = tile.Y;
                if (Math.Abs(top - lastTop) > .01)
                    f += lastTop != defaultLastTop ? "</Row><Row>" : "<Row>";
                lastTop = top;

                f += "<Column><Tile>";
                f += tile.Name;
                f += "</Tile></Column>";
            }
            f += "</Row>";

            f += "<Walls>";
            foreach (var wall in editor.WallList)
            {
                if (wall.Line.Y1 == wall.Line.Y2)
                    wall.Line.Y1 = wall.Line.Y2 = wall.Line.Y1 - ts;

                f += "<Wall>";
                f += "{0},{1} {2},{3}".ToFormat(
                    Math.Round((wall.Line.X1 - offsetX) / ts),
                    Math.Round((offsetY - wall.Line.Y1) / ts),
                    Math.Round((wall.Line.X2 - offsetX) / ts),
                    Math.Round((offsetY - wall.Line.Y2) / ts));
                f += "</Wall>";

                if (wall.Line.Y1 == wall.Line.Y2)
                    wall.Line.Y1 = wall.Line.Y2 = wall.Line.Y1 + ts;
            }
            f+="</Walls>";

            f += "<Doors>";
            foreach (var door in editor.DoorList)
            {
                if (door.Rotation == 0)
                    door.Line.Y1 = door.Line.Y2 = door.Line.Y1 - ts;

                f += "<Door>";
                f += "{0},{1},{2}".ToFormat(
                    Math.Round((door.Line.X1 - offsetX) / ts), 
                    Math.Round((offsetY - door.Line.Y1) / ts), 
                    door.Rotation);
                f += "</Door>";

                if (door.Rotation == 0)
                    door.Line.Y1 = door.Line.Y2 = door.Line.Y1 + ts;
            }
            f += "</Doors>";

            f += "</Level>";

            file.LoadXml(f);
            file.Save(fileLoc);
        }

        public static void Load(MainWindow editor, string fileLoc, int ts)
        {
            editor.Clear(); // clean up anything the user might have been working on before

            var doc = new XmlDocument();
            doc.Load(fileLoc);
            var level = doc.SelectSingleNode("Level");

            // Floor Tiles
            var rows = level.SelectNodes("Row");
            var yOffset = rows.Count - 1;
            var x = 0;
            var y = 0;
            foreach (XmlNode row in rows)
            {
                foreach (XmlNode column in row.SelectNodes("Column"))
                {
                    var tile = column.SelectSingleNode("Tile");

                    editor.SelectTile(tile.InnerText);
                    editor.PlaceTile(new Point(x * ts, (yOffset - y) * ts));

                    x++;
                }
                y++;
                x = 0;
            }

            // Walls
            var walls = level.SelectSingleNode("Walls");
            foreach (XmlNode wall in walls.SelectNodes("Wall"))
            {
                // wall pattern is: x,y x1,y2
                string[] parts = wall.InnerText.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
                var p1 = parts[0].Split(',');
                var p2 = parts[1].Split(',');

                var isHor = p1[1] == p2[1];

                editor.PlaceWall(
                    new Point(int.Parse(p1[0]) * ts, (yOffset - int.Parse(p1[1]) + (isHor ? 1 : 0)) * ts), 
                    new Point(int.Parse(p2[0]) * ts, (yOffset - int.Parse(p2[1]) + (isHor ? 1 : 0)) * ts));
            }

            // Doors
            var doors = level.SelectSingleNode("Doors");
            foreach (XmlNode door in doors.SelectNodes("Door"))
            {
                // door is stored as x,y,rotation
                var parts = door.InnerText.Split(',');
                var dx = int.Parse(parts[0]);
                var dy = int.Parse(parts[1]);
                var drot = int.Parse(parts[2]);

                // when placing doors, you need to put it closer to left or top side of tile it's on or else it will mess up
                if(drot == 0)
                    editor.PlaceDoor(new Point(dx * ts + 1, (yOffset - dy + 1) * ts));
                if (drot == 90)
                    editor.PlaceDoor(new Point(dx * ts, (yOffset - dy) * ts + 1));
            }
        }
    }
}
