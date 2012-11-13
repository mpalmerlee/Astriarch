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
    public class Player
    {
        private static int NEXT_PLAYER_ID = 1;

        public int Id;
        
        public PlayerType Type;

        public string Name = "";

        public PlayerResources Resources;

        public Color Color = Colors.Green;

        public int TotalFoodAmount
        {
            get
            {
                int foodAmt = 0;
                foreach (Planet p in this.OwnedPlanets.Values)
                {
                    foodAmt += p.Resources.FoodAmount;
                }
                return foodAmt;
            }
        }

        public int LastTurnFoodNeededToBeShipped = 0;//this is for computers to know how much gold to keep in surplus for food shipments

        public PlayerGameOptions Options;

        public Dictionary<int, Planet> OwnedPlanets = new Dictionary<int, Planet>();
        public Dictionary<int, Planet> KnownPlanets = new Dictionary<int, Planet>();
        public Dictionary<int, LastKnownFleet> LastKnownPlanetFleetStrength = new Dictionary<int, LastKnownFleet>();

        public Dictionary<int, PlanetProductionItem> PlanetBuildGoals = new Dictionary<int, PlanetProductionItem>();

        public Planet HomePlanet = null;

        public List<Fleet> FleetsInTransit = new List<Fleet>();

        private Dictionary<int, Fleet> fleetsArrivingOnUnownedPlanets = new Dictionary<int, Fleet>();//indexed by planet id

        public Player(PlayerType playerType, string name)
        {
            this.Id = NEXT_PLAYER_ID++;
            this.Type = playerType;
            this.Name = name;
            this.Resources = new PlayerResources();
            this.Options = new PlayerGameOptions();
        }

        public void AddFleetArrivingOnUnownedPlanet(Planet p, Fleet f)
        {
            if (!fleetsArrivingOnUnownedPlanets.ContainsKey(p.Id))
            {
                fleetsArrivingOnUnownedPlanets.Add(p.Id, f);
            }
            else//merge fleet with existing
            {
                fleetsArrivingOnUnownedPlanets[p.Id].MergeFleet(f);
            }
        }

        /// <summary>
        /// returns and clears our index of fleets arriving on unowned planets
        /// </summary>
        /// <returns>list of fleets arriving on unowned planets</returns>
        public List<Fleet> GatherFleetsArrivingOnUnownedPlanets()
        {
            List<Fleet> unownedPlanetFleets = new List<Fleet>(this.fleetsArrivingOnUnownedPlanets.Count);
            foreach(Fleet f in this.fleetsArrivingOnUnownedPlanets.Values)
            {
                unownedPlanetFleets.Add(f);
            }
            this.fleetsArrivingOnUnownedPlanets = new Dictionary<int, Fleet>();
            return unownedPlanetFleets;
        }

        public bool PlanetOwnedByPlayer(Planet p)
        {
            bool planetOwnedByPlayer = false;

            if(this.OwnedPlanets.ContainsKey(p.Id))
                planetOwnedByPlayer = true;

            return planetOwnedByPlayer;
        }

        public bool PlanetKnownByPlayer(Planet p)
        {
            bool planetKnownByPlayer = GameTools.GameModel.ShowUnexploredPlanetsAndEnemyPlayerStats;
            if (this.KnownPlanets.ContainsKey(p.Id))
                planetKnownByPlayer = true;

            return planetKnownByPlayer;
        }

        public bool PlanetContainsFriendlyInboundFleet(Planet p)
        {
            foreach (Fleet f in this.FleetsInTransit)
            {
                if (f.DestinationHex.PlanetContainedInHex.Id == p.Id)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetTotalPopulation()
        {
            int totalPop = 0;

            foreach(Planet p in this.OwnedPlanets.Values)
            {
                totalPop += p.Population.Count;
            }

            return totalPop;
        }

        public int GetTotalFoodProductionPerTurn()
        {
            int totalFoodProduction = 0;

            foreach (Planet p in this.OwnedPlanets.Values)
            {
                totalFoodProduction += p.ResourcesPerTurn.FoodAmountPerTurn;
            }

            return totalFoodProduction;
        }

        public int CountPlanetsNeedingExploration()
        {
            int planetsNeedingExploration = 0;
            foreach (Planet p in GameTools.GameModel.Planets)
            {
                if (p.Owner != this && !this.PlanetContainsFriendlyInboundFleet(p))//exploring/attacking inbound fleets to unowned planets should be excluded
                {
                    if (!this.KnownPlanets.ContainsKey(p.Id) ||
                        //also check to see if we need to update intelligence
                        (this.Type != PlayerType.Computer_Easy &&
                        this.LastKnownPlanetFleetStrength.ContainsKey(p.Id) &&
                        (GameTools.GameModel.Turn.Number - this.LastKnownPlanetFleetStrength[p.Id].TurnLastExplored) > 20))//TODO: figure out best value here (could be shorter if planets are closer together)
                    {
                        planetsNeedingExploration++;
                    }
                }
            }

            return planetsNeedingExploration;
        }
    }

    public enum PlayerType
    { 
        Human = 0,
        Computer_Easy = 1,
        Computer_Normal = 2,
        Computer_Hard = 3,
        Computer_Expert = 4
    }

    public class PlayerResources
    {
        public int GoldAmount;
        public double GoldRemainder = 0.0;
        
        public int OreAmount = 0;
        public double OreRemainder = 0.0;
        
        public int IridiumAmount = 0;
        public double IridiumRemainder = 0.0;

        public PlayerResources()
        {
            //players start with some resources
            this.GoldAmount = 3;
            this.IridiumAmount = 1;
            this.OreAmount = 2;
        }

        public void AccumulateResourceRemainders()
        {
            if (this.GoldRemainder >= 1.0)
            {
                this.GoldAmount += (int)(this.GoldRemainder / 1.0);
                this.GoldRemainder = this.GoldRemainder % 1;
            }

            if (this.OreRemainder >= 1.0)
            {
                this.OreAmount += (int)(this.OreRemainder / 1.0);
                this.OreRemainder = this.OreRemainder % 1;
            }

            if (this.IridiumRemainder >= 1.0)
            {
                this.IridiumAmount += (int)(this.IridiumRemainder / 1.0);
                this.IridiumRemainder = this.IridiumRemainder % 1;
            }
        }

        public PlayerResources Clone()
        {
            PlayerResources prRet = new PlayerResources();
            prRet.GoldAmount = this.GoldAmount;
            prRet.GoldRemainder = this.GoldRemainder;
            prRet.OreAmount = this.OreAmount;
            prRet.OreRemainder = this.OreRemainder;
            prRet.IridiumAmount = this.IridiumAmount;
            prRet.IridiumRemainder = this.IridiumRemainder;
            return prRet;
        }

        /// <summary>
        /// if amount to spend is higher than total gold, subtracts gold to zero, and returns how much was spent
        /// </summary>
        /// <param name="amountToSpend">how much to subtract</param>
        /// <returns>the amount of gold actually spent</returns>
        public int SpendGoldAsPossible(int amountToSpend)
        {
            if (this.GoldAmount >= amountToSpend)
            {
                this.GoldAmount = this.GoldAmount - amountToSpend;
                return amountToSpend;
            }
            else
            {
                int spent = amountToSpend - this.GoldAmount;
                this.GoldAmount = 0;
                return spent;
            }
        }
    }

    public class PlayerGameOptions
    {
        public bool ShowHexGrid = false;
        public bool ShowPlanetaryConflictPopups = true;

        public PlayerGameOptions()
        {

        }
    }
}
