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
    public class Fleet
    {
        public Dictionary<StarShipType, List<StarShip>> StarShips;

        public bool HasSpacePlatform = false;
        public int SpacePlatformDamage = 0;

        public const int SPACE_PLATFORM_STRENGTH = 64;//TODO: twice the strength of a battleship, is this good?
        
        public Hexagon DestinationHex = null;
        
        private int turnsToDestination = 0;
        public int TurnsToDestination
        {
            get { return this.turnsToDestination; }
        }

        public Hexagon LocationHex = null;

        //TODO: move these later
        private Hexagon travelingFromHex = null;

        public Rectangle TravelFleetRect = new Rectangle();
        public Line TravelLine = new Line();
        public TextBlock TravelETATextBlock = new TextBlock();
        private Point travelDistancePoint = new Point();
        private int totalTravelDistance = 0;
        static SolidColorBrush travelBrush = new SolidColorBrush();

        public delegate void FleetMoved(Fleet f);
        public event FleetMoved OnFleetMoved;

        public delegate void FleetMergedOrDestroyed(Fleet f);
        public event FleetMergedOrDestroyed OnFleetMergedOrDestroyed;


        public Fleet()
        {
            this.StarShips = new Dictionary<StarShipType, List<StarShip>>();
            this.StarShips[StarShipType.SystemDefense] = new List<StarShip>();
            this.StarShips[StarShipType.Scout] = new List<StarShip>();
            this.StarShips[StarShipType.Destroyer] = new List<StarShip>();
            this.StarShips[StarShipType.Cruiser] = new List<StarShip>();
            this.StarShips[StarShipType.Battleship] = new List<StarShip>();

            //setup visual elements

            this.TravelFleetRect.Stroke = null;
            this.TravelFleetRect.Fill = null;
            this.TravelFleetRect.Height = 10;
            this.TravelFleetRect.Width = 10;
            travelBrush.Color = Colors.Green;
            this.TravelLine.StrokeThickness = 2;
            this.TravelLine.Stroke = travelBrush;

            //setup TravelETATextBlock
            this.TravelETATextBlock.TextAlignment = TextAlignment.Center;
            this.TravelETATextBlock.FontSize = 8.0;
            this.TravelETATextBlock.Foreground = travelBrush;

            this.SetFleetHasSpacePlatform();
        }

        public void SetFleetHasSpacePlatform()
        {
            this.HasSpacePlatform = false;
            if (this.LocationHex != null && this.LocationHex.PlanetContainedInHex != null && this.LocationHex.PlanetContainedInHex.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count > 0)
                this.HasSpacePlatform = true;
        }

        public void SetDestination(Hexagon travelingFromHex, Hexagon destinationHex)
        {
            this.HasSpacePlatform = false;

            this.travelingFromHex = travelingFromHex;
            this.DestinationHex = destinationHex;
            this.totalTravelDistance = GameTools.GameModel.GameGrid.GetHexDistance(this.travelingFromHex, this.DestinationHex);
            this.turnsToDestination = this.totalTravelDistance;
            this.updateTravelLine();
        }

        private void updateTravelLine()
        {
            if (turnsToDestination != 0)//draw the line
            {
                this.TravelLine.X2 = this.DestinationHex.MidPoint.X;
                this.TravelLine.Y2 = this.DestinationHex.MidPoint.Y;

                double traveled = (double)(this.totalTravelDistance - this.turnsToDestination) / (double)this.totalTravelDistance;

                this.travelDistancePoint.X = this.travelingFromHex.MidPoint.X - ((this.travelingFromHex.MidPoint.X - this.TravelLine.X2) * traveled);
                this.travelDistancePoint.Y = this.travelingFromHex.MidPoint.Y - ((this.travelingFromHex.MidPoint.Y - this.TravelLine.Y2) * traveled);

                this.TravelLine.X1 = this.travelDistancePoint.X;
                this.TravelLine.Y1 = this.travelDistancePoint.Y;

                string turns = this.turnsToDestination > 1 ? " Turns" : " Turn";
                this.TravelETATextBlock.Text = this.turnsToDestination + turns;

                int offsetX = 6;
                //if the travel line points from left to right, we will instead show the TravelETATextBlock to the left of the ship indicator
                if (this.TravelLine.X2 > this.travelingFromHex.MidPoint.X)
                {
                    if (this.turnsToDestination > 9)
                        offsetX = -42;
                    else if (this.turnsToDestination == 1)
                        offsetX = -33;
                    else
                        offsetX = -37;
                }

                this.TravelETATextBlock.SetValue(Canvas.LeftProperty, this.travelDistancePoint.X + offsetX);
                this.TravelETATextBlock.SetValue(Canvas.TopProperty, this.travelDistancePoint.Y - 5);

                this.TravelFleetRect.SetValue(Canvas.LeftProperty, this.travelDistancePoint.X - 5);
                this.TravelFleetRect.SetValue(Canvas.TopProperty, this.travelDistancePoint.Y - 5);
            }
            else
            {
                this.totalTravelDistance = 0;

                this.TravelLine.Stroke = null;
                this.TravelETATextBlock.Foreground = null;
            }
        }

        public void MoveFleet()
        {
            this.LocationHex = null;
            //TODO: should this update the location hex to a closer hex as well?
            this.turnsToDestination -= 1;

            this.updateTravelLine();

            
            if (this.OnFleetMoved != null)
            {
                this.OnFleetMoved(this);
            }
        }

        public void LandFleet(Fleet landingFleet, Hexagon newLocation)
        {
            this.travelingFromHex = null;
            this.DestinationHex = null;
            this.turnsToDestination = 0;
            this.LocationHex = newLocation;

            this.MergeFleet(landingFleet);

            this.SetFleetHasSpacePlatform();
        }

        public void MergeFleet(Fleet mergingFleet)
        {
            this.StarShips[StarShipType.SystemDefense].AddRange(mergingFleet.StarShips[StarShipType.SystemDefense]);

            this.StarShips[StarShipType.Scout].AddRange(mergingFleet.StarShips[StarShipType.Scout]);

            this.StarShips[StarShipType.Destroyer].AddRange(mergingFleet.StarShips[StarShipType.Destroyer]);

            this.StarShips[StarShipType.Cruiser].AddRange(mergingFleet.StarShips[StarShipType.Cruiser]);

            this.StarShips[StarShipType.Battleship].AddRange(mergingFleet.StarShips[StarShipType.Battleship]);

            mergingFleet.SendFleetMergedOrDestroyed();
        }

        public List<StarShip> GetAllStarShips()
        {
            List<StarShip> ships = new List<StarShip>();
            ships.AddRange(this.StarShips[StarShipType.Battleship]);
            ships.AddRange(this.StarShips[StarShipType.Cruiser]);
            ships.AddRange(this.StarShips[StarShipType.Destroyer]);
            ships.AddRange(this.StarShips[StarShipType.Scout]);
            ships.AddRange(this.StarShips[StarShipType.SystemDefense]);
            return ships;
        }

        public void SendFleetMergedOrDestroyed()
        {
            if (this.OnFleetMergedOrDestroyed != null)
            {
                this.OnFleetMergedOrDestroyed(this);
            }
        }

        /*
        private void reduceStarShipsInFleetByType(StarShipType type, int damageAmount, out int damageAmountRemaining)
        {
            //TODO: notify user of destroyed ships
            int amountDamaged = 0;
            for (int i = this.StarShips[type].Count - 1; i >= 0; i--)
            {
                StarShip sd = this.StarShips[type][i];
                bool destroyed = sd.DamageShip(damageAmount, out amountDamaged);
                damageAmount -= amountDamaged;
                if (destroyed)
                    this.StarShips[type].RemoveAt(i);
            }
            damageAmountRemaining = damageAmount;
        }
         * */

        /*
        public bool ReduceFleetBasedOnStrength(int thisFleetStrength, int enemyFleetStrength)
        {
            bool thisFleetStronger = thisFleetStrength > enemyFleetStrength;
            double strengthMultiplier = 1.0;
            int strengthReduction = 0;
            if (thisFleetStronger)
            {
                if (enemyFleetStrength != 0)
                    strengthMultiplier = (thisFleetStrength) / (double)enemyFleetStrength;
            }
            else//same or enemy stronger
            {
                if (thisFleetStrength != 0)
                    strengthMultiplier = (enemyFleetStrength) / (double)thisFleetStrength;
            }
            strengthReduction = (int)Math.Round(enemyFleetStrength / strengthMultiplier);
            int halfStrengthReduction = (int)Math.Round(strengthReduction/2.0);
            strengthReduction += GameTools.Randomizer.Next((-1) * halfStrengthReduction, halfStrengthReduction + 1);
            
            this.ReduceFleet(strengthReduction, thisFleetStrength);
            if (this.DetermineFleetStrength(true) == 0)
                return true;//the fleet was destroyed
            return false;
        }
         * */

        /// <summary>
        /// Simply remove ships with strength = 0
        /// </summary>
        /// <param name="spacePlatformDamage">amount the space platform was damaged</param>
        public void ReduceFleet(int spacePlatformDamage)
        {
            //start by reducing the weakest ships first (they are sent to the front lines)
            //if a starship's damage gets to it's strength level, it's destroyed
            foreach(StarShipType type in this.StarShips.Keys)
            {
                for (int i = this.StarShips[type].Count - 1; i >= 0; i--)
                {
                    if (this.StarShips[type][i].Strength <= 0)
                        this.StarShips[type].RemoveAt(i);
                }
            }

            this.SpacePlatformDamage += spacePlatformDamage;

            if (this.SpacePlatformDamage != 0 && this.HasSpacePlatform && this.SpacePlatformDamage >= Fleet.SPACE_PLATFORM_STRENGTH)
            {
                //destroy the space platform
                if (this.LocationHex != null && this.LocationHex.PlanetContainedInHex != null)
                {
                    this.LocationHex.PlanetContainedInHex.BuiltImprovements[PlanetImprovementType.SpacePlatform].Clear();
                    this.HasSpacePlatform = false;
                }
            }
        }

        /*
        public void ReduceFleetOld(int damageAmount, int totalStrength)
        {
            //start by reducing the weakest ships first (they are sent to the front lines)
            //if a starship's damage gets to it's strength level, it's destroyed

            //make sure strengthReduction is less than thisFleetStrength
            if (damageAmount > totalStrength)
                damageAmount = totalStrength;

            //TODO: notify user of destroyed ships

            this.reduceStarShipsInFleetByType(StarShipType.SystemDefense, damageAmount, out damageAmount);

            if (damageAmount == 0)
                return;
            this.reduceStarShipsInFleetByType(StarShipType.Scout, damageAmount, out damageAmount);

            if (damageAmount == 0)
                return;
            this.reduceStarShipsInFleetByType(StarShipType.Destroyer, damageAmount, out damageAmount);

            if (damageAmount == 0)
                return;
            this.reduceStarShipsInFleetByType(StarShipType.Cruiser, damageAmount, out damageAmount);

            if (damageAmount == 0)
                return;
            this.reduceStarShipsInFleetByType(StarShipType.Battleship, damageAmount, out damageAmount);

            if (damageAmount != 0 && this.HasSpacePlatform && damageAmount >= Fleet.SPACE_PLATFORM_STRENGTH)
            {
                //destroy the space platform
                if (this.LocationHex != null && this.LocationHex.PlanetContainedInHex != null)
                {
                    this.LocationHex.PlanetContainedInHex.BuiltImprovements[PlanetImprovementType.SpacePlatform].Clear();
                    this.HasSpacePlatform = false;
                }
            }
        }
         * */

        public int DetermineFleetStrength()
        {
            return this.DetermineFleetStrength(true);
        }

        public int DetermineFleetStrength(bool includeSpacePlatformDefence)
        {
            int strength = 0;

            foreach (StarShip sd in this.StarShips[StarShipType.SystemDefense])
            {
                strength += sd.Strength;
            }

            foreach (StarShip s in this.StarShips[StarShipType.Scout])
            {
                strength += s.Strength;
            }

            foreach (StarShip d in this.StarShips[StarShipType.Destroyer])
            {
                strength += d.Strength;
            }

            foreach (StarShip c in this.StarShips[StarShipType.Cruiser])
            {
                strength += c.Strength;
            }

            foreach (StarShip b in this.StarShips[StarShipType.Battleship])
            {
                strength += b.Strength;
            }

            if (includeSpacePlatformDefence && this.HasSpacePlatform)
                strength += Fleet.SPACE_PLATFORM_STRENGTH - SpacePlatformDamage;

            return strength;
        }

        /// <summary>
        /// Creates a new fleet with the number of ships specified, removing the ships from this fleet
        /// </summary>
        /// <param name="scoutCount"></param>
        /// <param name="destoyerCount"></param>
        /// <param name="cruiserCount"></param>
        /// <param name="battleshipCount"></param>
        /// <returns>the new fleet</returns>
        public Fleet SplitFleet(int scoutCount, int destoyerCount, int cruiserCount, int battleshipCount)
        {
            Fleet newFleet = new Fleet();
            newFleet.LocationHex = this.LocationHex;

            if (scoutCount > this.StarShips[StarShipType.Scout].Count ||
                destoyerCount > this.StarShips[StarShipType.Destroyer].Count ||
                cruiserCount > this.StarShips[StarShipType.Cruiser].Count ||
                battleshipCount > this.StarShips[StarShipType.Battleship].Count)
            {
                throw new InvalidOperationException("Cannot send more ships than in the fleet!");
            }

            for (int i = 0; i < scoutCount; i++)
            {
                newFleet.StarShips[StarShipType.Scout].Add(this.StarShips[StarShipType.Scout][0]);
                this.StarShips[StarShipType.Scout].RemoveAt(0);
            }
            for (int i = 0; i < destoyerCount; i++)
            {
                newFleet.StarShips[StarShipType.Destroyer].Add(this.StarShips[StarShipType.Destroyer][0]);
                this.StarShips[StarShipType.Destroyer].RemoveAt(0);
            }
            for (int i = 0; i < cruiserCount; i++)
            {
                newFleet.StarShips[StarShipType.Cruiser].Add(this.StarShips[StarShipType.Cruiser][0]);
                this.StarShips[StarShipType.Cruiser].RemoveAt(0);
            }
            for (int i = 0; i < battleshipCount; i++)
            {
                newFleet.StarShips[StarShipType.Battleship].Add(this.StarShips[StarShipType.Battleship][0]);
                this.StarShips[StarShipType.Battleship].RemoveAt(0);
            }

            return newFleet;
        }

        public Fleet SplitOffSmallestPossibleFleet()
        {
            Fleet newFleet = null;//if we can't find any to send
            int scoutCount = 0;
            int destroyerCount = 0;
            int cruiserCount = 0;
            int battleshipCount = 0;

            if (this.StarShips[StarShipType.Scout].Count != 0)
                scoutCount = 1;
            else if (this.StarShips[StarShipType.Destroyer].Count != 0)
                destroyerCount = 1;
            else if (this.StarShips[StarShipType.Cruiser].Count != 0)
                cruiserCount = 1;
            else if (this.StarShips[StarShipType.Battleship].Count != 0)
                battleshipCount = 1;

            if (scoutCount != 0 || destroyerCount != 0 || cruiserCount != 0 || battleshipCount != 0)
                newFleet = this.SplitFleet(scoutCount, destroyerCount, cruiserCount, battleshipCount);

            return newFleet;
        }

        public int GetPlanetaryFleetMobileStarshipCount()
        {
            int mobileStarships = 0;

            mobileStarships += this.StarShips[StarShipType.Scout].Count;
            mobileStarships += this.StarShips[StarShipType.Destroyer].Count;
            mobileStarships += this.StarShips[StarShipType.Cruiser].Count;
            mobileStarships += this.StarShips[StarShipType.Battleship].Count;

            return mobileStarships;
        }

        public Fleet CloneFleet()
        {
            Fleet f = new Fleet();

            f.LocationHex = this.LocationHex;
            f.HasSpacePlatform = this.HasSpacePlatform;

            foreach (StarShip s in this.StarShips[StarShipType.SystemDefense])
            {
                f.StarShips[StarShipType.SystemDefense].Add(s.CloneStarShip());
            }
            foreach (StarShip s in this.StarShips[StarShipType.Scout])
            {
                f.StarShips[StarShipType.Scout].Add(s.CloneStarShip());
            }
            foreach (StarShip s in this.StarShips[StarShipType.Destroyer])
            {
                f.StarShips[StarShipType.Destroyer].Add(s.CloneStarShip());
            }
            foreach (StarShip s in this.StarShips[StarShipType.Cruiser])
            {
                f.StarShips[StarShipType.Cruiser].Add(s.CloneStarShip());
            }
            foreach (StarShip s in this.StarShips[StarShipType.Battleship])
            {
                f.StarShips[StarShipType.Battleship].Add(s.CloneStarShip());
            }

            return f;
        }

        public void RepairFleet()
        {
            this.SpacePlatformDamage = 0;
            foreach (StarShip s in this.StarShips[StarShipType.SystemDefense])
            {
                s.DamageAmount = 0;
            }
            foreach (StarShip s in this.StarShips[StarShipType.Scout])
            {
                s.DamageAmount = 0;
            }
            foreach (StarShip s in this.StarShips[StarShipType.Destroyer])
            {
                s.DamageAmount = 0;
            }
            foreach (StarShip s in this.StarShips[StarShipType.Cruiser])
            {
                s.DamageAmount = 0;
            }
            foreach (StarShip s in this.StarShips[StarShipType.Battleship])
            {
                s.DamageAmount = 0;
            }
        }

        public override string ToString()
        {
            string fleetSummary = "";
            int count = -1;
            count = this.StarShips[StarShipType.SystemDefense].Count;
            if (count > 0)
            {
                fleetSummary = count + " Defender" + (count > 1 ? "s" : "");
            }
            count = this.StarShips[StarShipType.Scout].Count;
            if (count > 0)
            {
                if (fleetSummary != "")
                    fleetSummary += ", ";
                fleetSummary += count + " Scout" + (count > 1 ? "s" : "");
            }
            count = this.StarShips[StarShipType.Destroyer].Count;
            if (count > 0)
            {
                if (fleetSummary != "")
                    fleetSummary += ", ";
                fleetSummary += count + " Destroyer" + (count > 1 ? "s" : "");
            }
            count = this.StarShips[StarShipType.Cruiser].Count;
            if (count > 0)
            {
                if (fleetSummary != "")
                    fleetSummary += ", ";
                fleetSummary += count + " Cruiser" + (count > 1 ? "s" : "");
            }
            count = this.StarShips[StarShipType.Battleship].Count;
            if (count > 0)
            {
                if (fleetSummary != "")
                    fleetSummary += ", ";
                fleetSummary += count + " Battleship" + (count > 1 ? "s" : "");
            }

            if (this.HasSpacePlatform)
            {
                if (fleetSummary != "")
                    fleetSummary += ", ";
                fleetSummary += "1 Space Platform";
            }

            if (fleetSummary == "")
                fleetSummary = "No Ships";

            return fleetSummary;
        }
    }

    public class LastKnownFleet
    {
        public int TurnLastExplored = 0;

        public Player LastKnownOwner = null;

        public Fleet Fleet = null;

        public LastKnownFleet(Fleet fleet, Player owner)
        {
            this.Fleet = fleet;
            this.LastKnownOwner = owner;
        }
    }

    public class StarShip
    {
        public StarShipType Type;

        public int BaseStarShipStrength = 0;
        //TODO: could eventually allow ship upgrades? to improve on base strength

        public int DamageAmount = 0;//starships will auto-heal between turns but if there are multiple battles in one turn involving a starship then this comes into play

        public int Strength
        {
            get { return BaseStarShipStrength - DamageAmount; }
        }

        public StarShip(StarShipType type)
        {
            this.Type = type;

            //ship strength is based on ship cost
            //  right now it is double the value of the next lower ship class
            //maybe later: + 50% (rounded up) of the next lower ship cost
            //each system defender is worth 2
            //each scout is worth 4 points
            //each destroyer is worth 8
            //each cruiser is worth 16
            //each battleship is worth 32

            switch (this.Type)
            {
                case StarShipType.SystemDefense:
                    this.BaseStarShipStrength = 2;
                    break;
                case StarShipType.Scout:
                    this.BaseStarShipStrength = 4;
                    break;
                case StarShipType.Destroyer:
                    this.BaseStarShipStrength = 8;
                    break;
                case StarShipType.Cruiser:
                    this.BaseStarShipStrength = 16;
                    break;
                case StarShipType.Battleship:
                    this.BaseStarShipStrength = 32;
                    break;
            }
        }

        /*
        /// <summary>
        /// increase damage amount, if starship is destroyed return true
        /// </summary>
        /// <param name="damageAmount"></param>
        /// <param name="amountDamaged"></param>
        /// <returns></returns>
        public bool DamageShip(int damageAmount, out int amountDamaged)
        {
            bool bRet = false;

            if ((this.BaseStarShipStrength - this.DamageAmount) > damageAmount)
            {
                this.DamageAmount += damageAmount;
                amountDamaged = damageAmount;
            }
            else//ship is destroyed
            {
                this.DamageAmount = this.BaseStarShipStrength;
                amountDamaged = BaseStarShipStrength;
                bRet = true;
            }

            return bRet;
        }
        */

        public StarShip CloneStarShip()
        {
            StarShip s = new StarShip(this.Type);
            s.DamageAmount = this.DamageAmount;
            return s;
        }
    }

    public class StarShipAdvantageStrengthComparer : IComparer<StarShip>
    {
        private StarShipType type;
        private bool isSpacePlatform;
        private Dictionary<StarShip, int> fleetDamagePending;

        public StarShipAdvantageStrengthComparer(StarShipType type, bool isSpacePlatform, Dictionary<StarShip, int> fleetDamagePending)
        {
            this.type = type;
            this.isSpacePlatform = isSpacePlatform;
            this.fleetDamagePending = fleetDamagePending;
        }

        int IComparer<StarShip>.Compare(StarShip a, StarShip b)
        {

            int ret = 0;
            int strengthA = this.getStarShipAdvantageDisadvantageAdjustedStrength(a);
            int strengthB = this.getStarShipAdvantageDisadvantageAdjustedStrength(b);

            if (strengthA == strengthB)
                ret = 0;
            else if (strengthA < strengthB)
                ret = -1;
            else
                ret = 1;
       

            return ret;
        }

        int getStarShipAdvantageDisadvantageAdjustedStrength(StarShip enemy)
        {
            int adjustedStrength = enemy.Strength;
            if (this.fleetDamagePending.ContainsKey(enemy))
                adjustedStrength -= this.fleetDamagePending[enemy];

            if (BattleSimulator.StarshipHasAdvantageBasedOnType(this.isSpacePlatform, this.type, false, enemy.Type))
                adjustedStrength *= 1;//this just ensures we're always attaking the enemy we have an advantage over
            else if (BattleSimulator.StarshipHasDisadvantageBasedOnType(this.isSpacePlatform, this.type, false, enemy.Type))
                adjustedStrength *= 10000;
            else
                adjustedStrength *= 100;

            return adjustedStrength;
        }
    }

    public static class StarShipFactoryHelper
    {

        public static Fleet GenerateShips(StarShipType type, int number, Hexagon locationHex)
        {
            Fleet f = new Fleet();
            f.LocationHex = locationHex;
            for (int i = 0; i < number; i++)
                f.StarShips[type].Add(new StarShip(type));
            return f;
        }

        public static Fleet GenerateFleetWithShipCount(int defenders, int scouts, int destroyers, int cruisers, int battleships, Hexagon locationHex)
        {
            Fleet f = new Fleet();
            f.LocationHex = locationHex;

            for (int i = 0; i < defenders; i++)
                f.StarShips[StarShipType.SystemDefense].Add(new StarShip(StarShipType.SystemDefense));
            for (int i = 0; i < scouts; i++)
                f.StarShips[StarShipType.Scout].Add(new StarShip(StarShipType.Scout));
            for (int i = 0; i < destroyers; i++)
                f.StarShips[StarShipType.Destroyer].Add(new StarShip(StarShipType.Destroyer));
            for (int i = 0; i < cruisers; i++)
                f.StarShips[StarShipType.Cruiser].Add(new StarShip(StarShipType.Cruiser));
            for (int i = 0; i < battleships; i++)
                f.StarShips[StarShipType.Battleship].Add(new StarShip(StarShipType.Battleship));

            return f;
        }
    }

    public enum StarShipType
    {
        SystemDefense,//System defense ships are not equiped with hyperdrive and cannot leave the system they are in
        Scout,
        Destroyer,
        Cruiser,
        Battleship
    }
}
