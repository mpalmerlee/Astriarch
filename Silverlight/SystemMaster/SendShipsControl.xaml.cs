using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using SystemMaster.Library;
using SystemMaster.ControlLibrary;

namespace SystemMaster
{
    public partial class SendShipsControl : UserControl, IDialogWindowControl
    {
        private Planet pSource;
        public Planet SourcePlanet
        {
            get { return this.pSource; }
        }

        private Planet pDest;
        private int distance;

        public Fleet CreatedFleet = null;

        public SendShipsControl(Planet pSource, Planet pDest, int distance)
        {
            InitializeComponent();

            this.pSource = pSource;
            this.pDest = pDest;
            this.distance = distance;

            this.SendShipsStatus.Text = distance + " parsecs from " + pSource.Name + " to " + pDest.Name;

            Fleet pf = pSource.PlanetaryFleet;

            this.SliderScouts.Maximum = pf.StarShips[StarShipType.Scout].Count;
            if (this.SliderScouts.Maximum > 0)
            {
                this.SliderScouts.IsEnabled = true;
                this.SliderScouts.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderScouts_ValueChanged);
            }

            this.SliderDestroyers.Maximum = pf.StarShips[StarShipType.Destroyer].Count;
            if (this.SliderDestroyers.Maximum > 0)
            {
                this.SliderDestroyers.IsEnabled = true;
                this.SliderDestroyers.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderDestroyers_ValueChanged);
            }

            this.SliderCruisers.Maximum = pf.StarShips[StarShipType.Cruiser].Count;
            if (this.SliderCruisers.Maximum > 0)
            {
                this.SliderCruisers.IsEnabled = true;
                this.SliderCruisers.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderCruisers_ValueChanged);
            }

            this.SliderBattleships.Maximum = pf.StarShips[StarShipType.Battleship].Count;
            if (this.SliderBattleships.Maximum > 0)
            {
                this.SliderBattleships.IsEnabled = true;
                this.SliderBattleships.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderBattleships_ValueChanged);
            }

            this.ButtonSendNoShips.Click += new RoutedEventHandler(ButtonSendNoShips_Click);
            this.ButtonSendAllShips.Click += new RoutedEventHandler(ButtonSendAllShips_Click);
        }

        void ButtonSendAllShips_Click(object sender, RoutedEventArgs e)
        {
            this.SliderScouts.Value = this.SliderScouts.Maximum;
            this.SliderDestroyers.Value = this.SliderDestroyers.Maximum;
            this.SliderCruisers.Value = this.SliderCruisers.Maximum;
            this.SliderBattleships.Value = this.SliderBattleships.Maximum;
        }

        void ButtonSendNoShips_Click(object sender, RoutedEventArgs e)
        {
            this.SliderScouts.Value = 0;
            this.SliderDestroyers.Value = 0;
            this.SliderCruisers.Value = 0;
            this.SliderBattleships.Value = 0;
        }

        void SliderBattleships_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.TextBoxBattleships.Text = this.SliderBattleships.Value.ToString();
        }

        void SliderCruisers_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.TextBoxCruisers.Text = this.SliderCruisers.Value.ToString();
        }

        void SliderDestroyers_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.TextBoxDestroyers.Text = this.SliderDestroyers.Value.ToString();
        }

        void SliderScouts_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.TextBoxScouts.Text = this.SliderScouts.Value.ToString();
        }

        public void OKClose()
        {
            //split the planetary fleet into a new fleet to send on it's way

            Fleet planetaryFleet = this.pSource.PlanetaryFleet;
            int scouts = Int32.Parse(this.TextBoxScouts.Text);
            int destroyers = Int32.Parse(this.TextBoxDestroyers.Text);
            int cruisers = Int32.Parse(this.TextBoxCruisers.Text);
            int battleships = Int32.Parse(this.TextBoxBattleships.Text);

            if (scouts != 0 || destroyers != 0 || cruisers != 0 || battleships != 0)
            {
                this.CreatedFleet = planetaryFleet.SplitFleet(scouts, destroyers, cruisers, battleships);

                this.CreatedFleet.SetDestination(this.pSource.BoundingHex, this.pDest.BoundingHex);

                this.pSource.OutgoingFleets.Add(this.CreatedFleet);
            }
            else
            {
                this.CreatedFleet = null;//just to be sure because we check this in MainPage.xaml.cs
            }
        }

        public void CancelClose()
        {
            //do nothing
        }
    }
}
