using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace SystemMaster.Library
{
    public class Grid
    {
        private const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public List<Hexagon> Hexes = new List<Hexagon>();

        public Hexagon SelectedHex = null;//for debugging for now
        

        public Grid(double width, double height)
        {
            //setup a dictionary for use later for assigning the Y CoOrd
            Dictionary<int, List<Hexagon>> HexagonsByXCoOrd = new Dictionary<int, List<Hexagon>>();

            int row = 0;
            for (double y = 0; y + Hexagon.HEIGHT <= height; y += Hexagon.HEIGHT / 2)
            {
                int col = 0;
                int colId = 0;

                double offset = 0;
                if (row % 2 == 1)
                {
                    offset = Hexagon.HEIGHT;
                    colId = 1;
                }
                
                for (double x = offset; x + Hexagon.WIDTH <= width; x += Hexagon.WIDTH + Hexagon.SIDE)
                {
                    Hexagon h = new Hexagon(letters[row].ToString() + (colId + 1), x, y);
                    h.PathCoOrdX = colId;//the column is the x coordinate of the hex, for the y coordinate we need to get more fancy
                    this.Hexes.Add(h);

                    if (!HexagonsByXCoOrd.ContainsKey(colId))
                        HexagonsByXCoOrd.Add(colId, new List<Hexagon>());
                    HexagonsByXCoOrd[colId].Add(h);

                    col++;
                    colId+=2;
                }
                row++;
            }

            //finally go through our list of hexagons by their x co-ordinate to assign the y co-ordinate
            foreach (KeyValuePair<int, List<Hexagon>> hexagonsByX in HexagonsByXCoOrd)
            {
                int yCoOrd = (hexagonsByX.Key / 2) + (hexagonsByX.Key % 2);
                foreach (Hexagon h in hexagonsByX.Value)
                {
                    h.PathCoOrdY = yCoOrd++;
                }
            }
        }

        public void SelectHex(Hexagon h)
        {
            //deselect if we've got one selected
            if (this.SelectedHex != null)
            {
                this.SelectedHex.Deselect();
                this.SelectedHex = null;
            }

            h.Select();
            this.SelectedHex = h;
        }

        public Hexagon GetHexAt(Point p)
        {
            //find the hex that contains this point
            foreach (Hexagon h in this.Hexes)
            {
                if (h.Contains(p))
                {
                    
                    return h;
                }
            }

            return null;
        }

        public void ShowHexGrid()
        {
            //turn on the Stroke Brush for each Polygon
            foreach (Hexagon h in this.Hexes)
            {
                h.PolyBase.Stroke = new SolidColorBrush(Colors.LightGray);
            }
        }

        public void HideHexGrid()
        {
            //turn off the Stroke Brush for each Polygon
            foreach (Hexagon h in this.Hexes)
            {
                h.PolyBase.Stroke = null;
            }
        }

        public int GetHexDistance(Hexagon h1, Hexagon h2)
        {
            //a good explanation of this calc can be found here:
            //http://playtechs.blogspot.com/2007/04/hex-grids.html
            int deltaX = h1.PathCoOrdX - h2.PathCoOrdX;
            int deltaY = h1.PathCoOrdY - h2.PathCoOrdY;
            return (Math.Abs(deltaX) + Math.Abs(deltaY) + Math.Abs(deltaX - deltaY)) / 2;
        }
    }

    public class Hexagon
    {
        public Polygon PolyBase;
        public const double HEIGHT = 40.0;
        public const double WIDTH = 55.0;
        public const double SIDE = 25.0;//hexagons will have 25 unit sides for now

        private double x;
        private double y;

        private string id;
        public string Id
        {
            get { return this.id; }
        }

        private Point topLeftPoint;
        public Point TopLeftPoint
        {
            get { return this.topLeftPoint; }
        }
        private Point bottomRightPoint;
        public Point BottomRightPoint
        {
            get { return this.bottomRightPoint; }
        }

        private Point midPoint;
        public Point MidPoint
        {
            get { return this.midPoint; }
        }

        public int PathCoOrdX;//x co-ordinate for distance finding
        public int PathCoOrdY;//y co-ordinate for distance finding

        private int zIndex;

        public Planet PlanetContainedInHex = null;//for backreference if exists

        public Hexagon(string id, double x, double y)
        {
            this.id = id;

            this.PolyBase = new Polygon();
            //this.PolyBase.Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            this.PolyBase.Points.Add(new Point(HEIGHT - SIDE, 0));
            this.PolyBase.Points.Add(new Point(HEIGHT, 0));
            this.PolyBase.Points.Add(new Point(WIDTH, HEIGHT / 2));
            this.PolyBase.Points.Add(new Point(HEIGHT, HEIGHT));
            this.PolyBase.Points.Add(new Point(HEIGHT - SIDE, HEIGHT));
            this.PolyBase.Points.Add(new Point(0, HEIGHT / 2));

            //this.PolyBase.Stroke = new SolidColorBrush(Colors.White);
            this.PolyBase.Stroke = null;
            this.PolyBase.Fill = null;
            //this.PolyBase.Opacity = 0.0;

            this.x = x;
            this.y = y;

            this.PolyBase.SetValue(Canvas.LeftProperty, x);
            this.PolyBase.SetValue(Canvas.TopProperty, y);

            this.topLeftPoint = new Point(this.x, this.y);
            this.bottomRightPoint = new Point(this.x + WIDTH, this.y + HEIGHT);
            this.midPoint = new Point(this.x + (WIDTH / 2), this.y + (HEIGHT / 2));

            this.PolyBase.SetValue(Canvas.ZIndexProperty, this.zIndex);
        }

        public void Select()
        {
            //Canvas.SetZIndex(this.PolyBase, 0);
            //save our original zIndex
            this.zIndex = (int)this.PolyBase.GetValue(Canvas.ZIndexProperty);
            this.PolyBase.SetValue(Canvas.ZIndexProperty, 1);

            this.PolyBase.Stroke = new SolidColorBrush(Colors.Green);
        }

        public void Deselect()
        {
            this.PolyBase.Stroke = null;//new SolidColorBrush(Colors.White);
            this.PolyBase.SetValue(Canvas.ZIndexProperty, this.zIndex);
        }

        //quick contains
        private bool isInBounds(Point p)
        {
            if(this.topLeftPoint.X < p.X && this.topLeftPoint.Y < p.Y &&
               p.X < this.bottomRightPoint.X && p.Y < this.bottomRightPoint.Y)
                return true;
            return false;
        }

        //grabbed from:
        //http://www.developingfor.net/c-20/testing-to-see-if-a-point-is-within-a-polygon.html
        //and
        //http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html#The%20C%20Code
        public bool Contains(Point p)
        {
            bool isIn = false;
            if (this.isInBounds(p))
            {
                //turn our absolute point into a relative point for comparing with the polygon's points
                Point pRel = new Point(p.X - this.x, p.Y - this.y);
                int i, j = 0;
                for (i = 0, j = this.PolyBase.Points.Count - 1; i < this.PolyBase.Points.Count; j = i++)
                {
                    Point iP = this.PolyBase.Points[i];
                    Point jP = this.PolyBase.Points[j];
                    if (
                        (
                         ((iP.Y <= pRel.Y) && (pRel.Y < jP.Y)) ||
                         ((jP.Y <= pRel.Y) && (pRel.Y < iP.Y))
                        //((iP.Y > pRel.Y) != (jP.Y > pRel.Y))
                        ) &&
                        (pRel.X < (jP.X - iP.X) * (pRel.Y - iP.Y) / (jP.Y - iP.Y) + iP.X)
                       )
                    {
                        isIn = !isIn;
                    }
                }
            }
            return isIn;
        }

    }
}
