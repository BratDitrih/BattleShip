using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Морской_бой
{
    public enum ShipDirection
    {
        Horizontal,
        Vertical
    }
    public class Ship
    {
        public int Size { get; set; } = 1;
        public HashSet<Point> points { get; set; } = new HashSet<Point>();
        public Ship(Point point, int[,] map)
        {
            points.Add(point);
            for (int i = point.X + 1; Form1.IsInside(i, point.Y) && map[i, point.Y] != 0; i++)
            {
                points.Add(new Point(i, point.Y));
                Size++;
            }
            for (int i = point.X - 1; Form1.IsInside(i, point.Y) && map[i, point.Y] != 0; i--)
            {
                points.Add(new Point(i, point.Y));
                Size++;
            }
            for (int i = point.Y + 1; Form1.IsInside(point.X, i) && map[point.X, i] != 0; i++)
            {
                points.Add(new Point(point.X, i));
                Size++;
            }
            for (int i = point.Y - 1; Form1.IsInside(point.X, i) && map[point.X, i] != 0; i--)
            {
                points.Add(new Point(point.X, i));
                Size++;
            }
        }
        public bool IsDead(int[,] map)
        {
            int counter = 0;
            foreach (var point in points)
            {
                if (map[point.X, point.Y] == 2) counter++;
            }
            if (counter == Size) return true;
            return false;
        }
        public ShipDirection GetShipDirection()
        {
            ShipDirection direction = new ShipDirection();
            if (Size != 1)
            {
                if (points.First().X != points.Last().X) direction = ShipDirection.Horizontal;
                else direction = ShipDirection.Vertical;
            }
            return direction;
        }
    }
}
