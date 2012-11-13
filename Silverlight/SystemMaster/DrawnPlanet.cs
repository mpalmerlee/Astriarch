using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SystemMaster.Library;

namespace SystemMaster
{
    public class DrawnPlanet
    {
        public Planet Planet;

        public Ellipse Ellipse;

        public TextBlock TextBlock;
        public TextBlock TextBlockPlanetStrength;

        public Rectangle FleetRectangle;
        public Rectangle SpacePlatformRectangle;

        private static ImageBrush ibClass2 = null;
        public static ImageBrush BRUSH_CLASS_2
        {
            get
            {
                if (DrawnPlanet.ibClass2 == null)
                {
                    DrawnPlanet.ibClass2 = new ImageBrush();
                    DrawnPlanet.ibClass2.ImageSource = new BitmapImage(new Uri(@"img/PlanetClass2Tile.png", UriKind.Relative));
                }
                return ibClass2;
            }
        }
        //ibClass2.Stretch = Stretch.None;

        private static ImageBrush ibClass1 = null;
        public static ImageBrush BRUSH_CLASS_1
        {
            get
            {
                if (DrawnPlanet.ibClass1 == null)
                {
                    DrawnPlanet.ibClass1 = new ImageBrush();
                    DrawnPlanet.ibClass1.ImageSource = new BitmapImage(new Uri(@"img/PlanetClass1Tile.png", UriKind.Relative));
                }
                return ibClass1;
            }
        }

        private static ImageBrush ibDeadPlanet = null;
        public static ImageBrush BRUSH_DEAD_PLANET
        {
            get
            {
                if (DrawnPlanet.ibDeadPlanet == null)
                {
                    DrawnPlanet.ibDeadPlanet = new ImageBrush();
                    DrawnPlanet.ibDeadPlanet.ImageSource = new BitmapImage(new Uri(@"img/PlanetDeadTile.png", UriKind.Relative));
                }
                return ibDeadPlanet;
            }
        }

        private static ImageBrush ibAsteroid = null;
        public static ImageBrush BRUSH_ASTEROID
        {
            get
            {
                if (DrawnPlanet.ibAsteroid == null)
                {
                    DrawnPlanet.ibAsteroid = new ImageBrush();
                    DrawnPlanet.ibAsteroid.ImageSource = new BitmapImage(new Uri(@"img/PlanetAsteroidTile.png", UriKind.Relative));
                    DrawnPlanet.ibAsteroid.Stretch = Stretch.None;
                }
                return ibAsteroid;
            }
        }

        private static ImageBrush ibFleet = null;
        public static ImageBrush BRUSH_FLEET
        {
            get
            {
                if (DrawnPlanet.ibFleet == null)
                {
                    DrawnPlanet.ibFleet = new ImageBrush();
                    DrawnPlanet.ibFleet.ImageSource = new BitmapImage(new Uri(@"img/starship.png", UriKind.Relative));
                    DrawnPlanet.ibFleet.Stretch = Stretch.None;
                }
                return ibFleet;
            }
        }

        private static ImageBrush ibSpacePlatform = null;
        public static ImageBrush BRUSH_SPACEPLATFORM
        {
            get
            {
                if (DrawnPlanet.ibSpacePlatform == null)
                {
                    DrawnPlanet.ibSpacePlatform = new ImageBrush();
                    DrawnPlanet.ibSpacePlatform.ImageSource = new BitmapImage(new Uri(@"img/spaceplatform.png", UriKind.Relative));
                    DrawnPlanet.ibSpacePlatform.Stretch = Stretch.None;
                }
                return ibSpacePlatform;
            }
        }

        public static SolidColorBrush WHITE_BRUSH = new SolidColorBrush(Colors.White);
        public static SolidColorBrush BLACK_BRUSH = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        public static SolidColorBrush GRAY_BRUSH = new SolidColorBrush(Colors.LightGray);
        public static SolidColorBrush GREEN_BRUSH = new SolidColorBrush(Colors.Green);
        public static SolidColorBrush TRANSPARENT_BRUSH = new SolidColorBrush(Colors.Transparent);

        public DrawnPlanet(Planet p)
        {
            this.Planet = p;

            this.Ellipse = new Ellipse();

            this.Ellipse.Width = Planet.PLANET_SIZE;
            this.Ellipse.Height = Planet.PLANET_SIZE;

            this.Ellipse.Stroke = DrawnPlanet.WHITE_BRUSH;
            this.Ellipse.StrokeThickness = 1;

            //add a title for each planet
            this.TextBlock = new TextBlock();
            //tb.Background = Brushes.AntiqueWhite;

            this.TextBlock.TextAlignment = TextAlignment.Center;
            this.TextBlock.Text = p.BoundingHex.Id;
            this.TextBlock.FontWeight = FontWeights.Bold;
            this.TextBlock.FontSize = 9.0;
            //tb.Inlines.Add(

            this.TextBlock.Foreground = new SolidColorBrush(Colors.Yellow);

            this.TextBlockPlanetStrength = new TextBlock();
            this.TextBlockPlanetStrength.TextAlignment = TextAlignment.Center;
            this.TextBlockPlanetStrength.Text = "";
            this.TextBlockPlanetStrength.FontSize = 8.0;
            this.TextBlockPlanetStrength.Foreground = new SolidColorBrush(Colors.Yellow);

            this.FleetRectangle = new Rectangle();
            this.FleetRectangle.Stroke = null;
            this.FleetRectangle.Fill = null;

            this.SpacePlatformRectangle = new Rectangle();
            this.SpacePlatformRectangle.Stroke = null;
            this.SpacePlatformRectangle.Fill = null;

            this.updateEllipseAndTextLocation();
        }

        private void updateEllipseAndTextLocation()
        {
            this.Ellipse.SetValue(Canvas.TopProperty, this.Planet.OriginPoint.Y);
            this.Ellipse.SetValue(Canvas.LeftProperty, this.Planet.OriginPoint.X);

            this.TextBlock.Width = this.Planet.Width;
            this.TextBlock.Height = this.Planet.Height;
            this.TextBlock.SetValue(Canvas.LeftProperty, this.Planet.OriginPoint.X);
            this.TextBlock.SetValue(Canvas.TopProperty, this.Planet.OriginPoint.Y + this.Ellipse.Height / 6);

            this.TextBlockPlanetStrength.Width = this.Planet.Width + 10;
            this.TextBlockPlanetStrength.Height = 20;
            this.TextBlockPlanetStrength.SetValue(Canvas.LeftProperty, this.Planet.OriginPoint.X - 5);
            this.TextBlockPlanetStrength.SetValue(Canvas.TopProperty, this.Planet.OriginPoint.Y - 12);

            this.FleetRectangle.Width = 10;
            this.FleetRectangle.Height = 10;

            this.FleetRectangle.SetValue(Canvas.LeftProperty, this.Planet.BoundingHex.MidPoint.X + (Planet.PLANET_SIZE/2) - 2);
            this.FleetRectangle.SetValue(Canvas.TopProperty, this.Planet.BoundingHex.MidPoint.Y + (Planet.PLANET_SIZE/2) - 2);

            this.SpacePlatformRectangle.Width = 10;
            this.SpacePlatformRectangle.Height = 10;
            this.SpacePlatformRectangle.SetValue(Canvas.LeftProperty, this.Planet.BoundingHex.MidPoint.X - (Planet.PLANET_SIZE/2) - 8);
            this.SpacePlatformRectangle.SetValue(Canvas.TopProperty, this.Planet.BoundingHex.MidPoint.Y + (Planet.PLANET_SIZE/2) - 2);
        }

        public void UpdatePlanetDrawingForPlayer(Player player)
        {
            bool planetKnownByPlayer = player.PlanetKnownByPlayer(this.Planet);

            this.Ellipse.Fill = DrawnPlanet.BLACK_BRUSH;

            if (planetKnownByPlayer)
            {
                if (this.Planet.Type == PlanetType.AsteroidBelt)
                    this.Planet.Width = Planet.PLANET_SIZE * 1.5;
                this.Ellipse.Width = this.Planet.Width;
                this.Ellipse.Height = this.Planet.Height;

                this.Ellipse.Stroke = null;

                switch (this.Planet.Type)
                {
                    case PlanetType.PlanetClass2:
                        this.Ellipse.Fill = DrawnPlanet.BRUSH_CLASS_2;
                        break;
                    case PlanetType.PlanetClass1:
                        this.Ellipse.Fill = DrawnPlanet.BRUSH_CLASS_1;
                        break;
                    case PlanetType.DeadPlanet:
                        this.Ellipse.Fill = DrawnPlanet.BRUSH_DEAD_PLANET;
                        break;
                    case PlanetType.AsteroidBelt:
                        this.Ellipse.Fill = DrawnPlanet.BRUSH_ASTEROID;
                        break;
                }

                this.updateEllipseAndTextLocation();

            }
            else if (this.Planet.Type != PlanetType.AsteroidBelt)
            {
                this.Ellipse.Stroke = DrawnPlanet.WHITE_BRUSH;
                this.Ellipse.StrokeThickness = 1;
            }

            this.FleetRectangle.Fill = null;
            this.SpacePlatformRectangle.Fill = null;

            Player lastKnownOwner = null;
            if (player.LastKnownPlanetFleetStrength.ContainsKey(this.Planet.Id))
                lastKnownOwner = player.LastKnownPlanetFleetStrength[this.Planet.Id].LastKnownOwner;

            if (player.PlanetOwnedByPlayer(this.Planet))
            {
                this.TextBlock.Foreground = new SolidColorBrush(player.Color);
                this.TextBlockPlanetStrength.Foreground = new SolidColorBrush(player.Color);

                //draw fleet image if we have mobile ships
                if (this.Planet.PlanetaryFleet.GetPlanetaryFleetMobileStarshipCount() > 0)
                {
                    this.FleetRectangle.Fill = DrawnPlanet.BRUSH_FLEET;
                }

                //draw spaceplatform image if we have a space platform
                if (this.Planet.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count > 0)
                {
                    this.SpacePlatformRectangle.Fill = DrawnPlanet.BRUSH_SPACEPLATFORM;
                }
                this.TextBlockPlanetStrength.Text = this.Planet.PlanetaryFleet.DetermineFleetStrength(true) + "";
            }
            else if (planetKnownByPlayer && lastKnownOwner != null)
            {
                this.TextBlock.Foreground = new SolidColorBrush(lastKnownOwner.Color);
                this.TextBlockPlanetStrength.Foreground = new SolidColorBrush(lastKnownOwner.Color);
                //if we know the enemy has a space platform and mobile fleet, we should draw those as well

                if (player.LastKnownPlanetFleetStrength[this.Planet.Id].Fleet.GetPlanetaryFleetMobileStarshipCount() > 0)
                {
                    this.FleetRectangle.Fill = DrawnPlanet.GetColorChangedImageBrush(@"img/starship.png", lastKnownOwner.Color);
                }

                if (player.LastKnownPlanetFleetStrength[this.Planet.Id].Fleet.HasSpacePlatform)
                {
                    this.SpacePlatformRectangle.Fill = DrawnPlanet.GetColorChangedImageBrush(@"img/spaceplatform.png", lastKnownOwner.Color);
                }

                this.TextBlockPlanetStrength.Text = player.LastKnownPlanetFleetStrength[this.Planet.Id].Fleet.DetermineFleetStrength(true) + "";
            }
            else
            {
                if (planetKnownByPlayer && this.Planet.Type == PlanetType.DeadPlanet)
                    this.TextBlock.Foreground = DrawnPlanet.BLACK_BRUSH;
                else
                    this.TextBlock.Foreground = new SolidColorBrush(Colors.Yellow);

                this.TextBlockPlanetStrength.Foreground = new SolidColorBrush(Colors.Yellow);
                if (planetKnownByPlayer && player.LastKnownPlanetFleetStrength.ContainsKey(this.Planet.Id))
                    this.TextBlockPlanetStrength.Text = player.LastKnownPlanetFleetStrength[this.Planet.Id].Fleet.DetermineFleetStrength(true) + "";
                else
                    this.TextBlockPlanetStrength.Text = "";
            }

            //this.TextBlock.Foreground = null;
            
        }

        private static ImageBrush GetColorChangedImageBrush(string uriRelativePath, Color colorNew)
        {
            BitmapImage bi = new BitmapImage();
            bi.CreateOptions = BitmapCreateOptions.None;

            System.Windows.Resources.StreamResourceInfo bmpStream = Application.GetResourceStream(new Uri(@"SystemMaster;component/" + uriRelativePath, UriKind.Relative));
            bi.SetSource(bmpStream.Stream);

            Image i = new Image();
            i.Stretch = Stretch.None;
            //i.Width = 11;
            //i.Height = 11;
            i.Source = bi;
            

            WriteableBitmap wb = new WriteableBitmap(i, null);
     
            //change the non-transparent pixels to the owner color
            DrawnPlanet.ChangeImageColor(i, colorNew);

            ImageBrush ib = new ImageBrush();
            ib.Stretch = Stretch.None;
            ib.ImageSource = i.Source;

            return ib;
        }

        private static void ChangeImageColor(Image img, Color colorNew)
        {
            
            WriteableBitmap bitmap = new WriteableBitmap(img, null);
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    int pixelLocation = bitmap.PixelWidth * y + x;
                    int pixel = bitmap.Pixels[pixelLocation];

                    byte[] pixelBytes = BitConverter.GetBytes(pixel);

                    if (pixelBytes[3] != 0)//if the pixel is non-transparent?
                    {
                        pixelBytes[0] = colorNew.B;//b
                        pixelBytes[1] = colorNew.G;//g
                        pixelBytes[2] = colorNew.R;//r

                        bitmap.Pixels[pixelLocation] = BitConverter.ToInt32(pixelBytes, 0);
                    }
                }
            }
            img.Source = bitmap;
        }
    }
}
