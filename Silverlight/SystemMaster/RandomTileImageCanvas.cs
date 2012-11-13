using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace SystemMaster
{
    public class RandomTileImageCanvas
    {
        List<ImageBrush> imageBrushes = new List<ImageBrush>();

        List<Color> colors = new List<Color>();

        private const int BACKGROUND_COUNT = 7;

        private double RECT_WIDTH = 80.0;
        private double RECT_HEIGHT = 60.0;

        private const int BACKGROUND_DARKNESS = 0;//this number shouldn't be over 40

        public RandomTileImageCanvas(Canvas cBase)
        {
            colors.Add(Colors.White);
            colors.Add(Colors.Yellow);
            colors.Add(Colors.Orange);
            colors.Add(Colors.Blue);
            colors.Add(Colors.Green);
            colors.Add(Colors.Red);
            colors.Add(Colors.Purple);

            this.populateBackgroundImages(cBase);

            this.populateStars(cBase);

        }

        private void populateBackgroundImages(Canvas cBase)
        {
            Dictionary<int, int> chanceTable = new Dictionary<int,int>();
            for(int i = 0; i < BACKGROUND_COUNT; i++)
            {
                ImageBrush ib = new ImageBrush();
                ib.ImageSource = new BitmapImage(new Uri(@"img/backgrounds/" + i + ".jpg", UriKind.Relative));
                ib.Stretch = Stretch.None;
                imageBrushes.Add(ib);

                chanceTable.Add(i, 100);
            }

            int rows = (int)(cBase.Height / RECT_HEIGHT);
            int cols = (int)(cBase.Width / RECT_WIDTH);

            for(int row = 0; row < rows; row += 2)
            {
                
                for(int col = 0; col < cols; col+=2)
                {
                    if (SystemMaster.Library.GameTools.Randomizer.Next(0, 2) == 0)
                    {
                        int offsetY = SystemMaster.Library.GameTools.Randomizer.Next(0, (int)RECT_HEIGHT);
                        int offsetX = SystemMaster.Library.GameTools.Randomizer.Next(0, (int)RECT_WIDTH);
                        Rectangle rect = new Rectangle();
                        rect.Width = RECT_WIDTH;
                        rect.Height = RECT_HEIGHT;
                        int imageIndex = pickRandomTile(chanceTable);
                        //int imageIndex = SystemMaster.Library.GameTools.Randomizer.Next(0, this.imageBrushes.Count);
                        rect.Fill = this.imageBrushes[imageIndex];

                        rect.Opacity = ((double)SystemMaster.Library.GameTools.Randomizer.Next(70 - BACKGROUND_DARKNESS, 101 - BACKGROUND_DARKNESS)) / 100;

                        cBase.Children.Add(rect);
                        rect.SetValue(Canvas.TopProperty, (row * RECT_HEIGHT) + offsetY);
                        rect.SetValue(Canvas.LeftProperty, (col * RECT_WIDTH) + offsetX);
                    }
                }
            }
        }

        private int pickRandomTile(Dictionary<int, int> chanceTable)
        {
            
            int chanceSum = 1;
            foreach(int i in chanceTable.Values)
                chanceSum += i;

            int pickedNumber = SystemMaster.Library.GameTools.Randomizer.Next(0, chanceSum);

            int currRangeLow = 0;

            int imageIndex = 0;
            foreach (int index in chanceTable.Keys)
            {
                int thisChance = chanceTable[index];
                if (pickedNumber < (thisChance + currRangeLow))
                {
                    imageIndex = index;
                    chanceTable[index] = (int)(thisChance / 2.0);
                    break;
                }
                currRangeLow += thisChance;
            }

            return imageIndex;
        }

        private void populateStars(Canvas cBase)
        {
            double size = 40.0;
            int rows = (int)(cBase.Height / size) - 1;
            int cols = (int)(cBase.Width / size) - 1;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    int starCount = SystemMaster.Library.GameTools.Randomizer.Next(0, 20);
                    for (int starNum = 0; starNum < starCount; starNum++)
                    {

                        int offsetY = SystemMaster.Library.GameTools.Randomizer.Next(0, (int)size);
                        int offsetX = SystemMaster.Library.GameTools.Randomizer.Next(0, (int)size);

                        double starSize = 1.0;
                        if (starNum >= 6 || starNum % 2 == 0)//prefer small white stars
                        {
                            Ellipse e = new Ellipse();
                            e.Width = starSize;
                            e.Height = starSize;
                            e.Fill = DrawnPlanet.WHITE_BRUSH;
                            e.Opacity = ((double)SystemMaster.Library.GameTools.Randomizer.Next(40 - BACKGROUND_DARKNESS, 101 - BACKGROUND_DARKNESS)) / 100;

                            e.SetValue(Canvas.TopProperty, (row * size) + offsetY);
                            e.SetValue(Canvas.LeftProperty, (col * size) + offsetX);
                            cBase.Children.Add(e);
                        }
                        else
                        {
                            Rectangle rect = new Rectangle();
                            rect.Width = size;
                            rect.Height = size;

                            starSize = SystemMaster.Library.GameTools.Randomizer.Next(1, 31) / 100.0;
                            double pointAndRadius = 0.5;

                            RadialGradientBrush radialGradient = new RadialGradientBrush();

                            // Set the GradientOrigin to the center of the area being painted.
                            radialGradient.GradientOrigin = new Point(pointAndRadius, pointAndRadius);

                            // Set the gradient center to the center of the area being painted.
                            radialGradient.Center = new Point(pointAndRadius, pointAndRadius);

                            // Set the radius of the gradient circle so that it extends to
                            // the edges of the area being painted.
                            radialGradient.RadiusX = pointAndRadius;
                            radialGradient.RadiusY = pointAndRadius;

                            // Create gradient stops
                            GradientStop gs1 = new GradientStop();
                            gs1.Color = Colors.White;
                            gs1.Offset = 0.0;
                            radialGradient.GradientStops.Add(gs1);

                            GradientStop gs2 = new GradientStop();
                            int colorIndex = SystemMaster.Library.GameTools.Randomizer.Next(0, this.colors.Count);
                            gs2.Color = this.colors[colorIndex];

                            gs2.Offset = starSize / 5;
                            radialGradient.GradientStops.Add(gs2);

                            GradientStop gs3 = new GradientStop();
                            gs3.Color = Colors.Black;
                            gs3.Offset = gs2.Offset * 2;// +(starSize / 4);
                            radialGradient.GradientStops.Add(gs3);

                            GradientStop gs4 = new GradientStop();
                            gs4.Color = Colors.Transparent;
                            gs4.Offset = gs3.Offset;// +starSize / 2;
                            radialGradient.GradientStops.Add(gs4);

                            rect.Fill = radialGradient;
                            rect.Opacity = ((double)SystemMaster.Library.GameTools.Randomizer.Next(50 - BACKGROUND_DARKNESS, 101 - BACKGROUND_DARKNESS)) / 100;
                            //rect.Stroke = new SolidColorBrush(Colors.White);

                            cBase.Children.Add(rect);
                            rect.SetValue(Canvas.TopProperty, (row * size) + offsetY);
                            rect.SetValue(Canvas.LeftProperty, (col * size) + offsetX);
                        }//not a small white star
                    }//foreach star in starcount
                }//foreach column
            }//foreach row
        }//populate stars

    }
}
