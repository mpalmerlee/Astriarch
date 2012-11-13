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
    public class Planet
    {
        private static int NEXT_PLANET_ID = 1;

        public const double PLANET_SIZE = 20.0;//this is the default width and height

        public int Id;

        public string Name;

        public PlanetType Type;

        public List<Citizen> Population;
        private int maxPopulation;
        public int MaxPopulation
        {
            get { return this.maxPopulation + this.BuiltImprovements[PlanetImprovementType.Colony].Count; }
        }

        private int remainderProduction = 0; //if we finished building something we may have remainder to apply to the next item
        public int RemainderProduction
        {
            get { return this.remainderProduction; }
        }

        public Queue<PlanetProductionItem> BuildQueue;
        public Dictionary<PlanetImprovementType, List<PlanetImprovement>> BuiltImprovements;
        public int BuiltImprovementCount
        {
            get 
            {
                int improvementCount = 0;

                foreach (PlanetImprovementType key in this.BuiltImprovements.Keys)
                {
                    if(key != PlanetImprovementType.SpacePlatform)//space platforms don't count for a slot
                        improvementCount += BuiltImprovements[key].Count;
                }

                return improvementCount;
            }
        }

        public int MaxImprovements;

        public PlanetResources Resources;

        public Hexagon BoundingHex;

        private Point originPoint;
        public Point OriginPoint
        {
            get { return this.originPoint; }
        }

        private double width;
        public double Width
        {
            get { return this.width; }
            set
            {
                this.width = value;
                this.recomputeOriginPoint();
            }
        }

        private double height;
        public double Height
        {
            get { return this.height; }
            set
            {
                this.height = value;
                this.recomputeOriginPoint();
            }
        }

        public Player Owner = null;//null means it is ruled by natives or nobody in the case of an asteroid belt

        public Fleet PlanetaryFleet = new Fleet();//the fleet stationed at this planet
        public List<Fleet> OutgoingFleets = new List<Fleet>();

        public PlanetPerTurnResourceGeneration ResourcesPerTurn;

        public PlanetHappinessType PlanetHappiness = PlanetHappinessType.Normal;

        public StarShipType? StarShipTypeLastBuilt = null;
        public bool BuildLastStarShip = true;

        public Planet(PlanetType type, string name, Hexagon boundingHex, Player initialOwner)
        {
            this.Id = NEXT_PLANET_ID++;
            this.Name = name;
            this.Type = type;

            this.BoundingHex = boundingHex;

            this.Population = new List<Citizen>();
            //set planet owner ensures there is one citizen
            this.SetPlanetOwner(initialOwner);
            if (this.Type == PlanetType.AsteroidBelt)
            {
                //asteroids and (dead planets?) don't start with pop
                this.Population.Clear();
            }

            this.width = PLANET_SIZE;
            this.height = PLANET_SIZE;

            this.recomputeOriginPoint();

            this.BuildQueue = new Queue<PlanetProductionItem>();

            this.BuiltImprovements = new Dictionary<PlanetImprovementType, List<PlanetImprovement>>();
            //setup the build improvements dictionary for each type
            this.BuiltImprovements.Add(PlanetImprovementType.Colony, new List<PlanetImprovement>());
            this.BuiltImprovements.Add(PlanetImprovementType.Factory, new List<PlanetImprovement>());
            this.BuiltImprovements.Add(PlanetImprovementType.Farm, new List<PlanetImprovement>());
            this.BuiltImprovements.Add(PlanetImprovementType.Mine, new List<PlanetImprovement>());
            this.BuiltImprovements.Add(PlanetImprovementType.SpacePlatform, new List<PlanetImprovement>());

            this.Resources = new PlanetResources();

            if (initialOwner != null)
            {
                //initialize home planet
                //add an aditional citizen
                this.Population.Add(new Citizen(this.Type));
                this.Population.Add(new Citizen(this.Type));

                //setup resources
                this.Resources.FoodAmount = 4;
            }

            this.ResourcesPerTurn = new PlanetPerTurnResourceGeneration(this, this.Type);
            this.GenerateResources();//set inital resources

            //set our max slots for improvements and build an initial defense fleet
            switch (this.Type)
            {
                case PlanetType.AsteroidBelt:
                    this.MaxImprovements = 3;
                    break;
                case PlanetType.DeadPlanet:
                    this.MaxImprovements = 4;
                    this.PlanetaryFleet = StarShipFactoryHelper.GenerateShips(StarShipType.SystemDefense, GameTools.Randomizer.Next(2, 5), this.BoundingHex);
                    break;
                case PlanetType.PlanetClass1:
                    this.MaxImprovements = 6;
                    this.PlanetaryFleet = StarShipFactoryHelper.GenerateShips(StarShipType.SystemDefense, GameTools.Randomizer.Next(5, 10), this.BoundingHex);
                    break;
                case PlanetType.PlanetClass2:
                    this.MaxImprovements = 9;
                    this.PlanetaryFleet = StarShipFactoryHelper.GenerateShips(StarShipType.SystemDefense, GameTools.Randomizer.Next(10, 15), this.BoundingHex);
                    break;
                default:
                    throw new NotImplementedException("Planet type " + this.Type + "not supported by planet constructor.");
            }
            

            this.maxPopulation = this.MaxImprovements;

            

            boundingHex.PlanetContainedInHex = this;//fill backreference
        }

        public void RemoveBuildQueueItemForRefund(int index)
        {
            double goldRefund = 0.0;
            double oreRefund = 0.0;
            double iridiumRefund = 0.0;

            if (this.BuildQueue.Count > index)
            {
                List<PlanetProductionItem> productionItems = new List<PlanetProductionItem>(this.BuildQueue.Count);
                foreach(PlanetProductionItem item in this.BuildQueue)
                    productionItems.Add(item);

                PlanetProductionItem ppi = productionItems[index];

                ppi.GetRefundAmount(out goldRefund, out oreRefund, out iridiumRefund);

                this.Owner.Resources.GoldRemainder += goldRefund;
                this.Owner.Resources.OreRemainder += oreRefund;
                this.Owner.Resources.IridiumRemainder += iridiumRefund;

                //accumulate
                this.Owner.Resources.AccumulateResourceRemainders();

                //remove item and repopulate buildQueue
                productionItems.RemoveAt(index);
                this.BuildQueue.Clear();
                foreach(PlanetProductionItem item in productionItems)
                    this.BuildQueue.Enqueue(item);
            }
        }

        public int BuiltAndBuildQueueImprovementCount()
        {
            int count = this.BuiltImprovementCount;
            foreach (PlanetProductionItem ppi in this.BuildQueue)
            {
                //space platforms don't count for slots used
                if (ppi is PlanetImprovement && ((PlanetImprovement)ppi).Type != PlanetImprovementType.SpacePlatform)
                {
                    count++;
                }
            }
            return count;
        }

        public int BuiltAndBuildQueueImprovementTypeCount(PlanetImprovementType type)
        {
            int count = this.BuiltImprovements[type].Count;
            foreach (PlanetProductionItem ppi in this.BuildQueue)
            {
                if (ppi is PlanetImprovement && ((PlanetImprovement)ppi).Type == type)
                {
                    count++;
                }
            }
            return count;
        }

        public bool BuildQueueContainsMobileStarship(out int turnsToComplete, out int starshipStrength)
        {
            turnsToComplete = 99;
            starshipStrength = 0;//setup defaults
            foreach (PlanetProductionItem ppi in this.BuildQueue)
            {
                if (ppi is StarShipInProduction && ((StarShipInProduction)ppi).Type != StarShipType.SystemDefense)
                {
                    turnsToComplete = ppi.TurnsToComplete;
                    starshipStrength = new StarShip(((StarShipInProduction)ppi).Type).BaseStarShipStrength;
                    return true;
                }
            }
            return false;
        }

        public bool BuildQueueContainsImprovement(PlanetImprovementType type)
        {
            foreach (PlanetProductionItem ppi in this.BuildQueue)
            {
                if (ppi is PlanetImprovement && ((PlanetImprovement)ppi).Type == type)
                {
                    return true;
                }
            }
            return false;
        }

        public void EnqueueProductionItemAndSpendResources(PlanetProductionItem item, PlayerResources resources)
        {
            this.BuildQueue.Enqueue(item);
            resources.GoldAmount -= item.GoldCost;
            resources.OreAmount -= item.OreCost;
            resources.IridiumAmount -= item.IridiumCost;
        }

        private void recomputeOriginPoint()
        {
            this.originPoint = new Point(this.BoundingHex.MidPoint.X - this.width / 2, this.BoundingHex.MidPoint.Y - this.height / 2);
        }

        public void GenerateResources()
        {
            this.ResourcesPerTurn.UpdateResourcesPerTurnBasedOnPlanetStats();

            if (this.Owner != null)
            {
                double divisor = 1.0;
                if (this.PlanetHappiness == PlanetHappinessType.Unrest)//unrest causes 1/2 production
                    divisor = 2.0;
                else if(this.PlanetHappiness == PlanetHappinessType.Riots)//riots cause 1/4 production
                    divisor = 4.0;

                this.Resources.FoodAmount += (int)Math.Round(this.ResourcesPerTurn.FoodAmountPerTurn / divisor);
                this.Resources.FoodRemainder += this.ResourcesPerTurn.RemainderFoodPerTurn / divisor;
                this.Resources.AccumulateResourceRemainders();

                this.Owner.Resources.OreAmount += (int)Math.Round(this.ResourcesPerTurn.OreAmountPerTurn / divisor);
                this.Owner.Resources.OreRemainder += this.ResourcesPerTurn.RemainderOrePerTurn / divisor;

                this.Owner.Resources.IridiumAmount += (int)Math.Round(this.ResourcesPerTurn.IridiumAmountPerTurn / divisor);
                this.Owner.Resources.IridiumRemainder += this.ResourcesPerTurn.RemainderIridiumPerTurn / divisor;
                //accumulate
                this.Owner.Resources.AccumulateResourceRemainders();
            }
        }

        public List<TurnEventMessage> BuildImprovements(out bool buildQueueEmpty)
        {
            buildQueueEmpty = false;
            List<TurnEventMessage> eotMessages = new List<TurnEventMessage>();
            if (this.BuildQueue.Count > 0)
            {
                PlanetProductionItem nextItem = this.BuildQueue.Peek();

                double divisor = 1.0;
                if (this.PlanetHappiness == PlanetHappinessType.Unrest)//unrest causes 1/4 development
                    divisor = 4.0;
                else if (this.PlanetHappiness == PlanetHappinessType.Riots)//riots cause 1/8 development
                    divisor = 8.0;

                int planetProductionPerTurn = (int)Math.Round(this.ResourcesPerTurn.ProductionAmountPerTurn / divisor);

                nextItem.ProductionCostComplete += planetProductionPerTurn + this.remainderProduction;
                this.remainderProduction = 0;

                if (nextItem.ProductionCostComplete >= nextItem.BaseProductionCost)
                {
                    //build it
                    nextItem = this.BuildQueue.Dequeue();
                    nextItem.TurnsToComplete = 0;

                    string nextItemInQueueName = "Nothing";
                    if (this.BuildQueue.Count > 0)
                    {
                        PlanetProductionItem nextInQueue = this.BuildQueue.Peek();
                        if (nextInQueue is PlanetImprovement)
                            nextItemInQueueName = ((PlanetImprovement)nextInQueue).ToString();
                        else if (nextInQueue is StarShipInProduction)
                            nextItemInQueueName = ((StarShipInProduction)nextInQueue).ToString();
                        else if (nextInQueue is PlanetImprovementToDestroy)
                            nextItemInQueueName = ((PlanetImprovementToDestroy)nextInQueue).ToString();
                    }

                    if (nextItem is PlanetImprovement)
                    {
                        PlanetImprovement pi = (PlanetImprovement)nextItem;
                        this.BuiltImprovements[pi.Type].Add(pi);

                        if (pi.Type == PlanetImprovementType.SpacePlatform)
                        {
                            this.PlanetaryFleet.HasSpacePlatform = true;
                        }

                        eotMessages.Add(new TurnEventMessage(TurnEventMessageType.ImprovementBuilt, this, pi.ToString() + " built on planet: " + this.Name + ", next in queue: " + nextItemInQueueName));
                    }
                    else if (nextItem is StarShipInProduction)//it's a ship
                    {
                        StarShipInProduction ssip = (StarShipInProduction)nextItem;
                        StarShip ship = new StarShip(ssip.Type);
                        this.PlanetaryFleet.StarShips[ssip.Type].Add(ship);

                        this.StarShipTypeLastBuilt = ssip.Type;

                        eotMessages.Add(new TurnEventMessage(TurnEventMessageType.ShipBuilt, this, ssip.ToString() + " built on planet: " + this.Name + ", next in queue: " + nextItemInQueueName));
                    }
                    else if(nextItem is PlanetImprovementToDestroy)//it is a destroy improvement request
                    {
                        PlanetImprovementToDestroy pitd = (PlanetImprovementToDestroy)nextItem;
                        if (this.BuiltImprovements[pitd.TypeToDestroy].Count > 0)//just to make sure
                        {
                            this.BuiltImprovements[pitd.TypeToDestroy].RemoveAt(this.BuiltImprovements[pitd.TypeToDestroy].Count - 1);
                            eotMessages.Add(new TurnEventMessage(TurnEventMessageType.ImprovementDemolished, this, GameTools.PlanetImprovementTypeToFriendlyName(pitd.TypeToDestroy) + " demolished on planet: " + this.Name + ", next in queue: " + nextItemInQueueName));

                            //TODO: there is probably a better place to handle this check for population overages
                            //TODO: should we also notify the user he/she lost a pop due to overcrowding or do this slower?
                            while(this.MaxPopulation < this.Population.Count)//pitd.TypeToDestroy == PlanetImprovementType.Colony)
                            {
                                this.Population.RemoveAt(this.Population.Count - 1);
                            }
                        }
                    }

                    this.remainderProduction = nextItem.ProductionCostComplete - nextItem.BaseProductionCost;
                    this.ResourcesPerTurn.UpdateResourcesPerTurnBasedOnPlanetStats();//now that we've built something, recalc production
                }
                else//not done yet, estimate turns to complete
                {
                    nextItem.EstimateTurnsToComplete(planetProductionPerTurn);
                }
            }
            else//notify user of empty build queue
            {
                buildQueueEmpty = true;
                string goldProduced = "";
                if(this.ResourcesPerTurn.ProductionAmountPerTurn > 0)
                    goldProduced = ", Gold generated";
                eotMessages.Add(new TurnEventMessage(TurnEventMessageType.BuildQueueEmpty, this, "Build queue empty on planet: " + this.Name + goldProduced));
            }

            return eotMessages;
        }

        public void SetPlanetOwner(Player p)
        {
            //remove current planet owner
            if (this.Owner != null)
            {
                //if this planet has items in the build queue we should remove them now
                for (int i = this.BuildQueue.Count - 1; i >= 0; i--)
                    this.RemoveBuildQueueItemForRefund(i);

                //also remove space platforms because they were destroyed in combat (used for defense)
                this.BuiltImprovements[PlanetImprovementType.SpacePlatform].Clear();

                if (this.Owner.PlanetBuildGoals.ContainsKey(this.Id))
                    this.Owner.PlanetBuildGoals.Remove(this.Id);

                this.Owner.OwnedPlanets.Remove(this.Id);
            }

            this.Owner = p;
            if (this.Owner != null)
            {
                p.KnownPlanets[this.Id] = this;
                if (p.OwnedPlanets.Keys.Count == 0)
                {
                    p.HomePlanet = this;
                }
                p.OwnedPlanets[this.Id] = this;
            }

            if (this.Population.Count == 0)
            {
                this.Population.Add(new Citizen(this.Type));
            }
        }

        public void SetPlanetExplored(Player p)
        {
            p.KnownPlanets[this.Id] = this;

            this.SetPlayerLastKnownPlanetFleetStrength(p);
        }

        public void SetPlayerLastKnownPlanetFleetStrength(Player p)
        {
            Fleet lastKnownFleet = StarShipFactoryHelper.GenerateFleetWithShipCount(this.PlanetaryFleet.StarShips[StarShipType.SystemDefense].Count,
                                                                                    this.PlanetaryFleet.StarShips[StarShipType.Scout].Count,
                                                                                    this.PlanetaryFleet.StarShips[StarShipType.Destroyer].Count,
                                                                                    this.PlanetaryFleet.StarShips[StarShipType.Cruiser].Count,
                                                                                    this.PlanetaryFleet.StarShips[StarShipType.Battleship].Count,
                                                                                    this.BoundingHex);

            lastKnownFleet.SetFleetHasSpacePlatform();//if the planet has a space platform, mark that

            LastKnownFleet lastKnownFleetObject = new LastKnownFleet(lastKnownFleet, this.Owner);
            lastKnownFleetObject.TurnLastExplored = GameTools.GameModel.Turn.Number;

            p.LastKnownPlanetFleetStrength[this.Id] = lastKnownFleetObject;
        }

        public void CountPopulationWorkerTypes(out int farmers, out int miners, out int workers)
        {
            farmers = 0; miners = 0; workers = 0;
            foreach (Citizen c in this.Population)
            {
                switch (c.WorkerType)
                {
                    case CitizenWorkerType.Farmer:
                        farmers++;
                        break;
                    case CitizenWorkerType.Miner:
                        miners++;
                        break;
                    case CitizenWorkerType.Worker:
                        workers++;
                        break;
                }
            }
        }

        public void UpdatePopulationWorkerTypesByDiff(int currentFarmers, int currentMiners, int currentWorkers, int farmerDiff, int minerDiff, int workerDiff)
        {
            while (farmerDiff != 0)
            {
                if (farmerDiff > 0)
                {
                    //move miners and workers to be farmers
                    if (currentMiners > 0 && minerDiff < 0)
                    {
                        this.getCitizenType(CitizenWorkerType.Miner).WorkerType = CitizenWorkerType.Farmer;
                        currentMiners--;
                        currentFarmers++;
                        farmerDiff--;
                        minerDiff++;

                    }
                    if (farmerDiff != 0 && currentWorkers > 0 && workerDiff < 0)
                    {
                        this.getCitizenType(CitizenWorkerType.Worker).WorkerType = CitizenWorkerType.Farmer;
                        currentWorkers--;
                        currentFarmers++;
                        farmerDiff--;
                        workerDiff++;
                    }
                }
                else
                {
                    //make farmers to miners and workers
                    if (minerDiff > 0 && currentMiners < this.MaxPopulation)
                    {
                        this.getCitizenType(CitizenWorkerType.Farmer).WorkerType = CitizenWorkerType.Miner;
                        currentFarmers--;
                        currentMiners++;
                        farmerDiff++;
                        minerDiff--;
                    }
                    if (farmerDiff != 0 && workerDiff > 0 && currentWorkers < this.MaxPopulation)
                    {
                        this.getCitizenType(CitizenWorkerType.Farmer).WorkerType = CitizenWorkerType.Worker;
                        currentFarmers--;
                        currentWorkers++;
                        farmerDiff++;
                        workerDiff--;
                    }
                }
            }

            //next check miners, don't touch farmers
            while (minerDiff != 0)
            {
                if (minerDiff > 0)
                {
                    //move workers to be miners
                    if (currentWorkers > 0 && workerDiff < 0)
                    {
                        this.getCitizenType(CitizenWorkerType.Worker).WorkerType = CitizenWorkerType.Miner;
                        currentWorkers--;
                        currentMiners++;
                        minerDiff--;
                        workerDiff++;
                    }
                }
                else
                {
                    //make miners to workers
                    if (workerDiff > 0 && currentWorkers < this.MaxPopulation)
                    {
                        this.getCitizenType(CitizenWorkerType.Miner).WorkerType = CitizenWorkerType.Worker;
                        currentMiners--;
                        currentWorkers++;
                        minerDiff++;
                        workerDiff--;
                    }
                }
            }

            //check for problems
            if (farmerDiff != 0 ||
                minerDiff != 0 ||
                workerDiff != 0 )
            {
                throw new InvalidOperationException("Couldn't move workers in Planet.UpdatePopulationWorkerTypesByDiff!");
            }
        }

        public void UpdatePopulationWorkerTypes(int targetFarmers, int targetMiners, int targetWorkers)
        {
            //this would be easier if we just cleared out our population and rebuilt it making sure we copy pop differences
            int currentFarmers = 0;
            int currentMiners = 0;
            int currentWorkers = 0;
            this.CountPopulationWorkerTypes(out currentFarmers, out currentMiners, out currentWorkers);

            //first check for farmers
            int diff = targetFarmers - currentFarmers;
            while (currentFarmers != targetFarmers)
            {
                if (diff > 0)
                {
                    //move miners and workers to be farmers
                    if (currentMiners > 0)
                    {
                        this.getCitizenType(CitizenWorkerType.Miner).WorkerType = CitizenWorkerType.Farmer;
                        currentMiners--;
                        currentFarmers++;
                        diff--;
                    }
                    if (diff > 0 && currentWorkers > 0)
                    {
                        this.getCitizenType(CitizenWorkerType.Worker).WorkerType = CitizenWorkerType.Farmer;
                        currentWorkers--;
                        currentFarmers++;
                        diff--;
                    }
                }
                else
                {
                    //make farmers to miners and workers
                    if (currentMiners < targetMiners && currentMiners < this.MaxPopulation)
                    {
                        this.getCitizenType(CitizenWorkerType.Farmer).WorkerType = CitizenWorkerType.Miner;
                        currentFarmers--;
                        currentMiners++;
                        diff++;
                    }
                    if (diff < 0 && currentWorkers < targetWorkers && currentWorkers < this.MaxPopulation)
                    {
                        this.getCitizenType(CitizenWorkerType.Farmer).WorkerType = CitizenWorkerType.Worker;
                        currentFarmers--;
                        currentWorkers++;
                        diff++;
                    }
                }
            }

            //next check workers, don't touch farmers
            diff = targetMiners - currentMiners;
            while (currentMiners != targetMiners)
            {
                if (diff > 0)
                {
                    //move workers to be miners
                    if (currentWorkers > 0)
                    {
                        this.getCitizenType(CitizenWorkerType.Worker).WorkerType = CitizenWorkerType.Miner;
                        currentWorkers--;
                        currentMiners++;
                        diff--;
                    }
                }
                else
                {
                    //make miners to workers
                    if (currentWorkers < targetWorkers && currentWorkers < this.MaxPopulation)
                    {
                        this.getCitizenType(CitizenWorkerType.Miner).WorkerType = CitizenWorkerType.Worker;
                        currentMiners--;
                        currentWorkers++;
                        diff++;
                    }
                }
            }

            //check for problems
            if (currentFarmers != targetFarmers ||
                currentMiners != targetMiners ||
                currentWorkers != targetWorkers)
            {
                throw new InvalidOperationException("Couldn't move workers in Planet.UpdatePopulationWorkerTypes!");
            }
        }

        private Citizen getCitizenType(CitizenWorkerType desiredType)
        {
            foreach (Citizen c in this.Population)
            {
                if (c.WorkerType == desiredType)
                    return c;
            }
            throw new InvalidOperationException("Couldn't find: " + desiredType + " in Planet.getCitizenType!");
        }

    }//class Planet

    public class PlanetFoodProductionComparer : IComparer<Planet>
    {
        int IComparer<Planet>.Compare(Planet a, Planet b)
        {
            int ret = b.ResourcesPerTurn.FoodAmountPerWorkerPerTurn.CompareTo(a.ResourcesPerTurn.FoodAmountPerWorkerPerTurn);

            return ret;
        }
    }

    public class PlanetMineralProductionComparer : IComparer<Planet>
    {
        private int oreNeeded = 0;
        private int iridiumNeeded = 0;
        public PlanetMineralProductionComparer(int oreNeeded, int iridiumNeeded)
        {
            this.oreNeeded = oreNeeded;
            this.iridiumNeeded = iridiumNeeded;
        }

        int IComparer<Planet>.Compare(Planet a, Planet b)
        {
            int ret = 0;

            if (oreNeeded >= iridiumNeeded)
                ret = b.ResourcesPerTurn.OreAmountPerWorkerPerTurn.CompareTo(a.ResourcesPerTurn.OreAmountPerWorkerPerTurn);
            else
                ret = b.ResourcesPerTurn.IridiumAmountPerWorkerPerTurn.CompareTo(a.ResourcesPerTurn.IridiumAmountPerWorkerPerTurn);

            return ret;
        }
    }

    public class PlanetDistanceComparer : IComparer<Planet>
    {
        private Planet source;
        public PlanetDistanceComparer(Planet source)
        {
            this.source = source;
        }

        #region IComparer<Planet> Members

        int IComparer<Planet>.Compare(Planet a, Planet b)
        {
            //TODO: this could be slow, we could just have an index for all distances instead of calculating it each time
            int ret = 0;
            int distanceA = 0;
            int distanceB = 0;
            if (a != source)//just to be sure
            {
                distanceA = GameTools.GameModel.GameGrid.GetHexDistance(source.BoundingHex, a.BoundingHex);
                ret = 1;
            }
            if (b != source)//just to be sure
            {
                distanceB = GameTools.GameModel.GameGrid.GetHexDistance(source.BoundingHex, b.BoundingHex);
                ret = -1;
            }

            if (ret != 0)//NOTE: this sorts in decending order or distance because we start at the end of the list
            {
                if (distanceA == distanceB)
                    ret = 0;
                else if (distanceA < distanceB)
                    ret = 1;
                else
                    ret = -1;
            }

            return ret;
        }

        #endregion
    }

    public class PlanetValueDistanceStrengthComparer : IComparer<Planet>
    {
        private Planet source;
        private Dictionary<int, LastKnownFleet> lastKnownPlanetFleetStrength;

        public PlanetValueDistanceStrengthComparer(Planet source, Dictionary<int, LastKnownFleet> lastKnownPlanetFleetStrength)
        {
            this.source = source;
            this.lastKnownPlanetFleetStrength = lastKnownPlanetFleetStrength;
        }

        private int increaseDistanceBasedOnPlanetValueAndFleetStrength(int distance, Planet p)
        {
            //to normalize distance, value and strength we increase the distance as follows
            //Based on Value (could eventually base this on what we need so if we need more minerals we prefer asteroids:
            // Class 2 planets add +0 distance
            // Class 1 planets add +1 distance
            // Dead planets add +2 distance
            // Asteroids add +3 distance
            //Based on last known fleet strength:
            // Strength < 20 add + 0
            // Strength 20 to 39 + 1
            // Strength 40 to 79 + 2
            // Strength > 80 + 3

            switch (p.Type)
            {
                case PlanetType.AsteroidBelt:
                    distance += 3;
                    break;
                case PlanetType.DeadPlanet:
                    distance += 2;
                    break;
                case PlanetType.PlanetClass1:
                    distance += 1;
                    break;
            }

            if (this.lastKnownPlanetFleetStrength.ContainsKey(p.Id))
            {
                int strength = this.lastKnownPlanetFleetStrength[p.Id].Fleet.DetermineFleetStrength();

                if (strength >= 20 && strength < 40)
                {
                    distance += 1;
                }
                else if(strength >= 40 && strength < 80)
                {
                    distance += 2;
                }
                else if (strength >= 80)
                {
                    distance += 3;
                }
            }

            return distance;
        }

        int IComparer<Planet>.Compare(Planet a, Planet b)
        {
            
            //TODO: this could be slow, we could just have an index for all distances instead of calculating it each time
            int ret = 0;
            int distanceA = 0;
            int distanceB = 0;
            if (a != source)//just to be sure
            {
                distanceA = GameTools.GameModel.GameGrid.GetHexDistance(source.BoundingHex, a.BoundingHex);
                ret = 1;
            }
            if (b != source)//just to be sure
            {
                distanceB = GameTools.GameModel.GameGrid.GetHexDistance(source.BoundingHex, b.BoundingHex);
                ret = -1;
            }

            if (ret != 0)//NOTE: this sorts in decending order or distance because we start at the end of the list
            {
                distanceA = this.increaseDistanceBasedOnPlanetValueAndFleetStrength(distanceA, a);
                distanceB = this.increaseDistanceBasedOnPlanetValueAndFleetStrength(distanceB, b);
                if (distanceA == distanceB)
                    ret = 0;
                else if (distanceA < distanceB)
                    ret = 1;
                else
                    ret = -1;
            }

            return ret;
        }
    }

    public class PlanetPerTurnResourceGeneration
    {
        private double baseFoodAmountPerWorkerPerTurn = 0;
        public double BaseFoodAmountPerWorkerPerTurn
        {
            get { return this.baseFoodAmountPerWorkerPerTurn; }
        }

        private double baseOreAmountPerWorkerPerTurn = 0;
        public double BaseOreAmountPerWorkerPerTurn
        {
            get { return this.baseOreAmountPerWorkerPerTurn; }
        }

        private double baseIridiumAmountPerWorkerPerTurn = 0;
        public double BaseIridiumAmountPerWorkerPerTurn
        {
            get { return this.baseIridiumAmountPerWorkerPerTurn; }
        }

        private double baseProductionPerWorkerPerTurn = 2.0;
        public double BaseProductionPerWorkerPerTurn
        {
            get { return this.baseProductionPerWorkerPerTurn; }
        }

        public int FoodAmountPerWorkerPerTurn
        {
            get 
            {
                return (int)Math.Round(this.GetExactFoodAmountPerWorkerPerTurn());   
            }
        }

        public int OreAmountPerWorkerPerTurn
        {
            get
            {
                return (int)Math.Round(this.GetExactOreAmountPerWorkerPerTurn());
            }
        }

        public int IridiumAmountPerWorkerPerTurn
        {
            get
            {
                return (int)Math.Round(this.GetExactIridiumAmountPerWorkerPerTurn());
            }
        }

        public int ProductionAmountPerWorkerPerTurn
        {
            get
            {
                return (int)Math.Round(this.GetExactProductionAmountPerWorkerPerTurn());
            }
        }

        private int foodAmountPerTurn = 0;
        public int FoodAmountPerTurn
        {
            get { return this.foodAmountPerTurn; }
        }
        private double remainderFoodPerTurn = 0.0;
        public double RemainderFoodPerTurn
        {
            get { return this.remainderFoodPerTurn; }
        }
        
        private int oreAmountPerTurn = 0;
        public int OreAmountPerTurn
        {
            get { return this.oreAmountPerTurn; }
        }
        private double remainderOrePerTurn = 0.0;
        public double RemainderOrePerTurn
        {
            get { return this.remainderOrePerTurn; }
        }

        private int iridiumAmountPerTurn = 0;
        public int IridiumAmountPerTurn
        {
            get { return this.iridiumAmountPerTurn; }
        }
        private double remainderIridiumPerTurn = 0.0;
        public double RemainderIridiumPerTurn
        {
            get { return this.remainderIridiumPerTurn; }
        }

        private int productionAmountPerTurn = 0;
        public int ProductionAmountPerTurn
        {
            get { return this.productionAmountPerTurn; }
        }
        private double remainderProductionPerTurn = 0.0;
        public double RemainderProductionPerTurn
        {
            get { return this.remainderProductionPerTurn; }
        }

        private Planet planet = null;

        private const double IMPROVEMENT_RATIO = 0.5;

        public PlanetPerTurnResourceGeneration(Planet p, PlanetType type)
        {
            this.planet = p;

            //this is the initial/base planet resource production
            //base values by planet type:
            //Class2 (home):2 food, 1 ore, 0.5 crystal
            //Class1: 1 food, 1 ore, 1 crystal
            //Dead: 0.5 food, 0.5 ore, 0 crystal
            //Asteroid: 0 food, 2 ore, 2 crystal
            switch (type)
            {
                case PlanetType.AsteroidBelt:
                    this.baseFoodAmountPerWorkerPerTurn = 0.5;//formerly 0.25
                    this.baseOreAmountPerWorkerPerTurn = 2.0;
                    this.baseIridiumAmountPerWorkerPerTurn = 2.0;//formerly 4.0
                    break;
                case PlanetType.DeadPlanet:
                    this.baseFoodAmountPerWorkerPerTurn = 1.0;
                    this.baseOreAmountPerWorkerPerTurn = 1.5;//formerly 0.5
                    this.baseIridiumAmountPerWorkerPerTurn = 0.5;//formerly 0.25
                    break;
                case PlanetType.PlanetClass1:
                    this.baseFoodAmountPerWorkerPerTurn = 1.5;
                    this.baseOreAmountPerWorkerPerTurn = 0.75;//formerly 1.5
                    this.baseIridiumAmountPerWorkerPerTurn = 0.75;
                    break;
                case PlanetType.PlanetClass2:
                    this.baseFoodAmountPerWorkerPerTurn = 2.0;
                    this.baseOreAmountPerWorkerPerTurn = 0.5;//formerly 1.0
                    this.baseIridiumAmountPerWorkerPerTurn = 0.25;//formerly 0.5
                    break;
                default:
                    throw new NotImplementedException("Planet type " + type + "not supported by PlanetPerTurnResourceGeneration constructor.");
            }
            //update our stats
            this.UpdateResourcesPerTurnBasedOnPlanetStats();
        }

        public void UpdateResourcesPerTurnBasedOnPlanetStats()
        {
            //base resource generation on citizen amount and assignment
            int farmerCount = 0;
            int minerCount = 0;
            int workerCount = 0;
            this.planet.CountPopulationWorkerTypes(out farmerCount, out minerCount, out workerCount);

            double baseFoodAmountPerTurn = this.baseFoodAmountPerWorkerPerTurn * farmerCount;
            double baseOreAmountPerTurn = this.baseOreAmountPerWorkerPerTurn * minerCount;
            double baseIridiumAmountPerTurn = this.baseIridiumAmountPerWorkerPerTurn * minerCount;
            double baseProductionAmountPerTurn = this.baseProductionPerWorkerPerTurn * workerCount;

            //determine production per turn
            this.foodAmountPerTurn = (int)baseFoodAmountPerTurn;
            this.oreAmountPerTurn = (int)baseOreAmountPerTurn;
            this.iridiumAmountPerTurn = (int)baseIridiumAmountPerTurn;
            this.productionAmountPerTurn = (int)baseProductionAmountPerTurn;

            //each mine increases mineral production 50% from base (additive)
            //each farm increases food production 50% from base (additive)
            //additive means if you have two mines you don't get (base production * 1.5 * 1.5),
            //you get (base production + (base production * 0.5) + (base production * 0.5))

            if (this.planet.BuiltImprovementCount > 0)
            {
                int farmCount = this.planet.BuiltImprovements[PlanetImprovementType.Farm].Count;
                int mineCount = this.planet.BuiltImprovements[PlanetImprovementType.Mine].Count;
                int factoryCount = this.planet.BuiltImprovements[PlanetImprovementType.Factory].Count;

                if (farmCount > 0)
                {
                    double foodRemainder = (baseFoodAmountPerTurn * IMPROVEMENT_RATIO) * farmCount;
                    this.foodAmountPerTurn = (int)(baseFoodAmountPerTurn + (foodRemainder / 1.0));
                    this.remainderFoodPerTurn = foodRemainder % 1;
                }
                if (mineCount > 0)
                {
                    double oreRemainder = (baseOreAmountPerTurn * IMPROVEMENT_RATIO) * mineCount;
                    this.oreAmountPerTurn = (int)(baseOreAmountPerTurn + (oreRemainder / 1.0));
                    this.remainderOrePerTurn = oreRemainder % 1;

                    double iridiumRemainder = (baseIridiumAmountPerTurn * IMPROVEMENT_RATIO) * mineCount;
                    this.iridiumAmountPerTurn = (int)(baseIridiumAmountPerTurn + (iridiumRemainder / 1.0));
                    this.remainderIridiumPerTurn = iridiumRemainder % 1;
                }
                if (factoryCount > 0)
                {
                    double prodRemainder = (baseProductionAmountPerTurn * IMPROVEMENT_RATIO) * factoryCount;
                    this.productionAmountPerTurn = (int)(baseProductionAmountPerTurn + (prodRemainder / 1.0));
                    this.remainderProductionPerTurn = prodRemainder % 1;
                }
            }
        }

        public double GetExactFoodAmountPerWorkerPerTurn()
        {
            int farmCount = this.planet.BuiltImprovements[PlanetImprovementType.Farm].Count;
            return this.baseFoodAmountPerWorkerPerTurn + (this.baseFoodAmountPerWorkerPerTurn * (IMPROVEMENT_RATIO * farmCount));
        }

        public double GetExactOreAmountPerWorkerPerTurn()
        {
            int mineCount = this.planet.BuiltImprovements[PlanetImprovementType.Mine].Count;
            return this.baseOreAmountPerWorkerPerTurn + (this.baseOreAmountPerWorkerPerTurn * (IMPROVEMENT_RATIO * mineCount));
        }

        public double GetExactIridiumAmountPerWorkerPerTurn()
        {
            int mineCount = this.planet.BuiltImprovements[PlanetImprovementType.Mine].Count;
            return this.baseIridiumAmountPerWorkerPerTurn + (this.baseIridiumAmountPerWorkerPerTurn * (IMPROVEMENT_RATIO * mineCount));
        }

        public double GetExactProductionAmountPerWorkerPerTurn()
        {
            int factoryCount = this.planet.BuiltImprovements[PlanetImprovementType.Factory].Count;
            return this.baseProductionPerWorkerPerTurn + (this.baseProductionPerWorkerPerTurn * (IMPROVEMENT_RATIO * factoryCount));
        }
    }

    public class PlanetResources
    {
        public int FoodAmount = 0;
        public double FoodRemainder = 0.0;

        public PlanetResources()
        {

        }

        public void AccumulateResourceRemainders()
        {
            if (this.FoodRemainder >= 1.0)
            {
                this.FoodAmount += (int)(this.FoodRemainder / 1.0);
                this.FoodRemainder = this.FoodRemainder % 1;
            }
        }
    }

    public abstract class PlanetProductionItem
    {
        public int TurnsToComplete;//once this is built turns to complete will be 0 and will go into the built improvements for the planet

        public int ProductionCostComplete = 0;//this is how much of the BaseProductionCost we've completed
        public int BaseProductionCost;//this will translate into Turns to Complete based on population, factories, etc...

        public int GoldCost = 1;
        public int OreCost = 0;
        public int IridiumCost = 0;

        public void EstimateTurnsToComplete(int planetProductionPerTurn)
        {
            if (planetProductionPerTurn != 0)
            {
                int productionCostLeft = this.BaseProductionCost - this.ProductionCostComplete;
                this.TurnsToComplete = (int)Math.Ceiling(productionCostLeft / planetProductionPerTurn);
            }
            else
                this.TurnsToComplete = 99;//if there are no workers
        }

        public void GetRefundAmount(out double goldRefund, out double oreRefund, out double iridiumRefund)
        {
            //give refund
            double refundPercent = 1 - (this.ProductionCostComplete / (double)this.BaseProductionCost);
            goldRefund = this.GoldCost * refundPercent;
            oreRefund = this.OreCost * refundPercent;
            iridiumRefund = this.IridiumCost * refundPercent;
        }
    }

    public class PlanetImprovementToDestroy : PlanetProductionItem
    {
        public PlanetImprovementType TypeToDestroy;

        public PlanetImprovementToDestroy(PlanetImprovementType typeToDestroy)
        {
            this.TypeToDestroy = typeToDestroy;

            this.GoldCost = 0;

            int originalProductionCost = (new PlanetImprovement(typeToDestroy)).BaseProductionCost;

            this.BaseProductionCost = originalProductionCost / 4;
        }

        public override string ToString()
        {
            return "Demolish " + GameTools.PlanetImprovementTypeToFriendlyName(this.TypeToDestroy);
        }
    }

    public class PlanetImprovement : PlanetProductionItem
    {
        public PlanetImprovementType Type;

        public PlanetImprovement(PlanetImprovementType type)
        {
            this.Type = type;

            //setup production costs
            switch (this.Type)
            {
                case PlanetImprovementType.Colony:
                    this.BaseProductionCost = 16;
                    this.OreCost = 2;
                    this.IridiumCost = 1;
                    this.GoldCost = 3;
                    break;
                case PlanetImprovementType.Factory:
                    this.BaseProductionCost = 32;
                    this.OreCost = 4;
                    this.IridiumCost = 2;
                    this.GoldCost = 6;
                    break;
                case PlanetImprovementType.Farm:
                    this.BaseProductionCost = 4;
                    this.GoldCost = 1;
                    break;
                case PlanetImprovementType.Mine:
                    this.BaseProductionCost = 8;
                    this.OreCost = 1;
                    this.GoldCost = 2;
                    break;
                case PlanetImprovementType.SpacePlatform:
                    this.BaseProductionCost = 90;//space platforms should be expensive
                    this.OreCost = 8;
                    this.IridiumCost = 4;
                    this.GoldCost = 12;
                    break;
            }
        }

        public override string ToString()
        {
            return GameTools.PlanetImprovementTypeToFriendlyName(this.Type);
        }
    }

    public class StarShipInProduction : PlanetProductionItem
    {
        public StarShipType Type;

        public StarShipInProduction(StarShipType type)
        {
            this.Type = type;

            switch (this.Type)
            {
                case StarShipType.Battleship:
                    this.BaseProductionCost = 90;
                    this.OreCost = 8;
                    this.IridiumCost = 4;
                    this.GoldCost = 12;
                    break;
                case StarShipType.Cruiser:
                    this.BaseProductionCost = 42;
                    this.OreCost = 4;
                    this.IridiumCost = 2;
                    this.GoldCost = 6;
                    break;
                case StarShipType.Destroyer:
                    this.BaseProductionCost = 18;
                    this.OreCost = 2;
                    this.IridiumCost = 1;
                    this.GoldCost = 3;
                    break;
                case StarShipType.Scout:
                    this.BaseProductionCost = 6;
                    this.OreCost = 1;
                    this.GoldCost = 1;
                    break;
                case StarShipType.SystemDefense:
                    this.BaseProductionCost = 2;
                    this.GoldCost = 1;
                    break;
            }
        }

        public override string ToString()
        {
            return GameTools.StarShipTypeToFriendlyName(this.Type);
        }
    }

    public enum PlanetImprovementType
    {
        Factory, //increases the speed of building other improvements and ships (and allows for building destroyers and the space platform)
        Colony, //increases the max population
        Farm, //increases food production
        Mine, //increases the rate of raw minerals production
        SpacePlatform //provides defense for the planet, further speeds ship production and allows for cruiser and battleship production
    }

    public enum PlanetType : int
    {
        AsteroidBelt = 0,
        DeadPlanet = 1,
        PlanetClass1 = 2,
        PlanetClass2 = 3
    }

    public enum PlanetHappinessType
    {
        Normal,
        Unrest,
        Riots
    }

    public enum CitizenWorkerType
    {
        Farmer,
        Miner,
        Worker
    }

    public class Citizen
    {
        public double PopulationChange = 0;//between -1 and 1, when this gets >= -1 then we loose one pop, > 1 we gain one pop

        public CitizenWorkerType WorkerType = CitizenWorkerType.Farmer;

        public Citizen(PlanetType type)
        {
            if (type == PlanetType.AsteroidBelt)//default to miners for asteroids
                this.WorkerType = CitizenWorkerType.Miner;
        }
    }
}
