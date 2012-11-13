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
    public static class AI
    {
        public delegate void ComputerSentFleet(Fleet f);
        public static event ComputerSentFleet OnComputerSentFleet;

        public static void ComputerTakeTurn(Player player)
        {
            //determine highest priority for resource usage
            //early game should be building developments and capturing/exploring planets while keeping up food production
            //mid game should be building space-platforms, high-class ships and further upgrading planets
            //late game should be strategically engaging the enemy
            //check planet production, prefer high-class planets (or even weather strategic points should be developed instead of high-class planets?)
            //if the planet has slots available and we have enough resources build (in order when we don't have)
            //
            computerSetPlanetBuildGoals(player);


            computerBuildImprovementsAndShips(player);


            //adjust population assignments as appropriate based on planet and needs
            computerAdjustPopulationAssignments(player);


            //base strategies on computer-level
            //here is the basic strategy:
            //if there are unclaimed explored planets
            //find the closest one and send a detachment, based on planet class
            //for easy level send detachments based only on distance
            //normal mode additionaly prefers class 2 planets
            //hard mode additionaly prefers Dead planets and considers enemy force before making an attack
            //expert mode additionaly prefers asteroid belts late in the game when it needs crystal

            //send scouts to unexplored planets (harder levels of computers know where better planets are?)

            computerSendShips(player);



        }

        private static void computerAdjustPopulationAssignments(Player player)
        {
            //TODO: this could be better by having an object for the planet pop assignments
            Dictionary<int, int> planetFarmers = new Dictionary<int, int>();
            Dictionary<int, int> planetMiners = new Dictionary<int, int>();
            Dictionary<int, int> planetWorkers = new Dictionary<int, int>();

            List<Planet> allPlanets = new List<Planet>();//this list will be for sorting

            //List<Planet> planetsClass2 = new List<Planet>();
            //List<Planet> planetsClass1 = new List<Planet>();
            //List<Planet> planetsDead = new List<Planet>();
            //List<Planet> planetsAsteroidBelt = new List<Planet>();

            foreach (Planet p in player.OwnedPlanets.Values)
            {
                int farmers = 0;
                int miners = 0;
                int workers = 0;
                p.CountPopulationWorkerTypes(out farmers, out miners, out workers);
                planetFarmers.Add(p.Id, farmers);
                planetMiners.Add(p.Id, miners);
                planetWorkers.Add(p.Id, workers);

                allPlanets.Add(p);
                /*
                switch (p.Type)
                {
                    case PlanetType.PlanetClass2:
                        planetsClass2.Add(p);
                        break;
                    case PlanetType.PlanetClass1:
                        planetsClass1.Add(p);
                        break;
                    case PlanetType.DeadPlanet:
                        planetsDead.Add(p);
                        break;
                    case PlanetType.AsteroidBelt:
                        planetsAsteroidBelt.Add(p);
                        break;
                }
                 * */

                //TODO: do we need to do this?
                //make sure we have up to date resources per turn before we make decisions based on assignments
                //p.ResourcesPerTurn.UpdateResourcesPerTurnBasedOnPlanetStats();
            }


            int totalPopulation = player.GetTotalPopulation();
            int totalFoodProduction = 0;
            int totalFoodAmountOnPlanets = 0;
            int totalPlanetsWithPopulationGrowthPotential = 0; //this will always give the computers some extra food surplus to avoid starving new population
            foreach (Planet p in player.OwnedPlanets.Values)
            {
                totalFoodAmountOnPlanets += p.Resources.FoodAmount;
                totalFoodProduction += p.ResourcesPerTurn.FoodAmountPerTurn;
                if (p.Population.Count < p.MaxPopulation)
                    totalPlanetsWithPopulationGrowthPotential++;
            }
            totalFoodAmountOnPlanets -= (totalPopulation);//this is what we'll eat this turn

            //this is what we'll keep in surplus to avoid starving more-difficult comps
            totalFoodAmountOnPlanets -= totalPlanetsWithPopulationGrowthPotential;

            //to make the easier computers even easier we will sometimes have them generate too much food and sometimes generate too little so they starve
            int totalFoodAmountOnPlanetsAdjustmentLow = 0;
            int totalFoodAmountOnPlanetsAdjustmentHigh = 0;
            //add some extra food padding based on player type, this will make the easier computers less agressive
            switch (player.Type)
            {
                case PlayerType.Computer_Easy:
                    totalFoodAmountOnPlanetsAdjustmentLow -= (totalPopulation * 3);
                    totalFoodAmountOnPlanetsAdjustmentHigh = (int)(totalPopulation * 1.5);
                    break;
                case PlayerType.Computer_Normal:
                    totalFoodAmountOnPlanetsAdjustmentLow -= (int)(totalPopulation * 1.5);
                    totalFoodAmountOnPlanetsAdjustmentHigh = totalPopulation;
                    break;
                case PlayerType.Computer_Hard:
                    totalFoodAmountOnPlanetsAdjustmentLow -= (totalPopulation / 2);
                    totalFoodAmountOnPlanetsAdjustmentHigh = 0;
                    break;
            }

            int totalFoodAmountOnPlanetsAdjustment = GameTools.Randomizer.Next(totalFoodAmountOnPlanetsAdjustmentLow, totalFoodAmountOnPlanetsAdjustmentHigh + 1);

            totalFoodAmountOnPlanets += totalFoodAmountOnPlanetsAdjustment;

            int oreAmountRecommended = 0;
            int iridiumAmountRecommended = 0;
            //base mineral need on desired production (build goals)
            //  for each planet with a space platform 
            //    if it is a class 1 or asteroid belt (planets with the most mineral potential), recommended ore and iridium should be for a battleship
            //    otherwise recommended ore and iridum should be for a cruiser
            //  for each planet without a space platform but at least one factory
            //    recommended ore and iridum should be for a destroyer
            //  for each planet witout a factory
            //    recommended ore for a scout only

            foreach (Planet p in player.OwnedPlanets.Values)
            {
                if (player.PlanetBuildGoals.ContainsKey(p.Id))
                {
                    PlanetProductionItem ppi = player.PlanetBuildGoals[p.Id];
                    oreAmountRecommended += ppi.OreCost;
                    iridiumAmountRecommended += ppi.IridiumCost;
                }
                else//this happens when we have placed our build goal into the queue already
                {
                    continue;
                    //add a bit more?
                    //oreAmountRecommended += 2;
                    //iridiumAmountRecommended += 1;
                }
            }

            //further stunt the easy computers growth by over estimating ore and iridium amount recommended
            double mineralOverestimation = 1.0;
            switch (player.Type)
            {
                case PlayerType.Computer_Easy:
                    mineralOverestimation = GameTools.Randomizer.Next(20, 41)/10.0;
                    break;
                case PlayerType.Computer_Normal:
                    mineralOverestimation = GameTools.Randomizer.Next(10, 21) / 10.0;
                    break;
            }

            int oreAmountNeeded = (int)Math.Round(oreAmountRecommended * mineralOverestimation) - player.Resources.OreAmount;
            int iridiumAmountNeeded = (int)Math.Round(iridiumAmountRecommended * mineralOverestimation) - player.Resources.IridiumAmount;

            if (totalPopulation > (totalFoodProduction + totalFoodAmountOnPlanets))
            {
                //check to see if we can add farmers to class 1 and class 2 planets
                int foodDiff = totalPopulation - (totalFoodProduction + totalFoodAmountOnPlanets);
                //first try to satiate by retasking miners/workers on planets with less food amount than population


                //gather potential planets for adding farmers to
                //TODO: this should order by planets with farms as well as planets who's population demands more food than it produces (more potential for growth)
                allPlanets.Sort(new PlanetFoodProductionComparer());

                int neededFarmers = foodDiff;
                List<Planet> planetCandidatesForAddingFarmers = new List<Planet>();

                if (neededFarmers > 0)
                    foreach (Planet p in allPlanets)
                    {
                        if (neededFarmers > 0 && p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn > 0
                                && (planetMiners[p.Id] > 0 || planetWorkers[p.Id] > 0))
                        {
                            planetCandidatesForAddingFarmers.Add(p);
                            neededFarmers -= p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn;
                            if (neededFarmers <= 0)
                                break;
                        }
                    }

                while (foodDiff > 0)
                {
                    bool changedAssignment = false;
                    foreach (Planet p in planetCandidatesForAddingFarmers)
                    {
                        int maxMiners = planetWorkers[p.Id];//we don't want more miners than workers when we have food shortages
                        if (p.Type == PlanetType.PlanetClass2)
                        {
                            if (p.BuiltImprovements[PlanetImprovementType.Mine].Count == 0)
                                maxMiners = 0;
                            else
                                maxMiners = 1;
                        }
                        if (planetMiners[p.Id] >= maxMiners && planetMiners[p.Id] > 0)
                        {
                            p.UpdatePopulationWorkerTypesByDiff(planetFarmers[p.Id], planetMiners[p.Id], planetWorkers[p.Id], 1, -1, 0);
                            planetFarmers[p.Id]++;
                            planetMiners[p.Id]--;
                            foodDiff -= p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn;
                            changedAssignment = true;
                        }
                        else if (planetWorkers[p.Id] > 0)
                        {
                            p.UpdatePopulationWorkerTypesByDiff(planetFarmers[p.Id], planetMiners[p.Id], planetWorkers[p.Id], 1, 0, -1);
                            planetFarmers[p.Id]++;
                            planetWorkers[p.Id]--;
                            foodDiff -= p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn;
                            changedAssignment = true;
                        }

                        if (foodDiff <= 0)
                            break;
                    }

                    //if we got here and didn't change anything, break out
                    if (!changedAssignment)
                        break;
                }//while (foodDiff > 0)

                //if we weren't able to satisfy the population's hunger at this point,
                // we may just have to starve
            }
            else//we can re-task farmers at class 1 and class 2 planets (and maybe dead planets?)
            {
                int foodDiff = (totalFoodProduction + totalFoodAmountOnPlanets) - totalPopulation;

                //gather potential planets for removing farmers from
                //TODO: this should order by planets without farms and planets which have more food production than it's population demands (less potential for growth)
                allPlanets.Sort(new PlanetFoodProductionComparer());
                allPlanets.Reverse();

                int unneededFarmers = foodDiff;
                List<Planet> planetCandidatesForRemovingFarmers = new List<Planet>();
                if (unneededFarmers > 0)
                    foreach (Planet p in allPlanets)
                    {
                        if (unneededFarmers > 0 &&
                            unneededFarmers > p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn &&
                            planetFarmers[p.Id] > 0)
                        {
                            planetCandidatesForRemovingFarmers.Add(p);
                            unneededFarmers -= p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn;
                            if (unneededFarmers <= 0)
                                break;
                        }
                    }

                while (foodDiff > 0)
                {
                    bool changedAssignment = false;
                    foreach (Planet p in planetCandidatesForRemovingFarmers)
                    {
                        if (foodDiff < p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn)
                            continue;//if removing this farmer would create a shortage, skip this planet

                        //check if we need more minerals, otherwise prefer production
                        //on terrestrial planets, make sure we have a mine before we add a miner
                        bool addMiner = (p.Type != PlanetType.PlanetClass2 || p.BuiltImprovements[PlanetImprovementType.Mine].Count > 0);
                        if (addMiner && (oreAmountNeeded > 0 || iridiumAmountNeeded > 0) && planetFarmers[p.Id] > 0)
                        {
                            p.UpdatePopulationWorkerTypesByDiff(planetFarmers[p.Id], planetMiners[p.Id], planetWorkers[p.Id], -1, 1, 0);
                            planetFarmers[p.Id]--;
                            planetMiners[p.Id]++;
                            oreAmountNeeded -= p.ResourcesPerTurn.OreAmountPerWorkerPerTurn;
                            iridiumAmountNeeded -= p.ResourcesPerTurn.IridiumAmountPerWorkerPerTurn;
                            foodDiff -= p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn;
                            changedAssignment = true;
                        }
                        else if (planetFarmers[p.Id] > 0)
                        {
                            p.UpdatePopulationWorkerTypesByDiff(planetFarmers[p.Id], planetMiners[p.Id], planetWorkers[p.Id], -1, 0, 1);
                            planetFarmers[p.Id]--;
                            planetWorkers[p.Id]++;
                            foodDiff -= p.ResourcesPerTurn.FoodAmountPerWorkerPerTurn;
                            changedAssignment = true;
                        }

                        if (foodDiff <= 0)
                            break;
                    }

                    //if we got here and didn't change anything, break out
                    if (!changedAssignment)
                        break;
                }//while (foodDiff > 0)
            }

            //next see if we need miners, look for workers to reassign (don't reassign farmers at this point)
            if (oreAmountNeeded > 0 || iridiumAmountNeeded > 0)
            {
                double oreAmountNeededWorking = (double)oreAmountNeeded;
                double iridumAmountNeededWorking = (double)iridiumAmountNeeded;

                List<Planet> planetCandidatesForRemovingWorkers = new List<Planet>();

                allPlanets.Sort(new PlanetMineralProductionComparer(oreAmountNeeded, iridiumAmountNeeded));

                if (oreAmountNeededWorking > 0 || iridumAmountNeededWorking > 0)
                    foreach (Planet p in allPlanets)
                    {
                        //leave at least one worker on terrestrial planets, leave 2 workers if we don't have a mine yet
                        int minWorkers = 0;
                        int minFarmers = -1;
                        if (p.Type == PlanetType.PlanetClass2)
                        {
                            minWorkers = p.BuiltImprovements[PlanetImprovementType.Mine].Count == 0 ? 2 : 1;
                            minFarmers = 0;//also make sure we have one farmer before reassigning a worker to be miner
                        }


                        if (planetWorkers[p.Id] > minWorkers && planetFarmers[p.Id] > minFarmers)
                        {
                            planetCandidatesForRemovingWorkers.Add(p);
                            oreAmountNeededWorking -= p.ResourcesPerTurn.OreAmountPerWorkerPerTurn;
                            iridumAmountNeededWorking -= p.ResourcesPerTurn.IridiumAmountPerWorkerPerTurn;
                        }
                    }
                

                while (oreAmountNeeded > 0 || iridiumAmountNeeded > 0)
                {
                    bool changedAssignment = false;
                    foreach (Planet p in planetCandidatesForRemovingWorkers)
                    {
                        //double check we have enough workers still
                        int minWorkers = 1;
                        if (p.BuiltImprovements[PlanetImprovementType.Mine].Count == 0)
                            minWorkers = 2;

                        if (planetWorkers[p.Id] > minWorkers)
                        {
                            p.UpdatePopulationWorkerTypesByDiff(planetFarmers[p.Id], planetMiners[p.Id], planetWorkers[p.Id], 0, 1, -1);
                            planetMiners[p.Id]++;
                            planetWorkers[p.Id]--;
                            oreAmountNeeded -= p.ResourcesPerTurn.OreAmountPerWorkerPerTurn;
                            iridiumAmountNeeded -= p.ResourcesPerTurn.IridiumAmountPerWorkerPerTurn;
                            changedAssignment = true;
                        }

                        if (oreAmountNeeded <= 0 && iridiumAmountNeeded <= 0)
                            break;
                    }

                    //if we got here and didn't change anything, break out
                    if (!changedAssignment)
                        break;
                }//while (oreAmountNeeded > 0 || iridiumAmountNeeded > 0)

            }//if (oreAmountNeeded > 0 || iridiumAmountNeeded > 0)
            else//we have enough minerals, reassign miners to workers
            {
                double oreMinersUnneeded = (double)Math.Abs(oreAmountNeeded) / 2.0;
                double iridumMinersUnneeded = (double)Math.Abs(iridiumAmountNeeded);
                List<Planet> planetCandidatesForRemovingMiners = new List<Planet>();

                allPlanets.Sort(new PlanetMineralProductionComparer(oreAmountNeeded, iridiumAmountNeeded));
                allPlanets.Reverse();

                if (oreMinersUnneeded > 0 || iridumMinersUnneeded > 0)
                    foreach (Planet p in allPlanets)
                    {
                        if (planetMiners[p.Id] > 0)
                        {
                            planetCandidatesForRemovingMiners.Add(p);
                            oreMinersUnneeded -= p.ResourcesPerTurn.OreAmountPerWorkerPerTurn;
                            iridumMinersUnneeded -= p.ResourcesPerTurn.IridiumAmountPerWorkerPerTurn;
                        }
                    }

                while (oreAmountNeeded <= 0 || iridiumAmountNeeded <= 0)
                {
                    bool changedAssignment = false;
                    foreach (Planet p in planetCandidatesForRemovingMiners)
                    {
                        if (planetMiners[p.Id] > 0)//double check we still have miners
                        {
                            p.UpdatePopulationWorkerTypesByDiff(planetFarmers[p.Id], planetMiners[p.Id], planetWorkers[p.Id], 0, -1, 1);
                            planetMiners[p.Id]--;
                            planetWorkers[p.Id]++;
                            oreAmountNeeded += p.ResourcesPerTurn.OreAmountPerWorkerPerTurn;
                            iridiumAmountNeeded += p.ResourcesPerTurn.IridiumAmountPerWorkerPerTurn;
                            changedAssignment = true;
                        }

                        if (oreAmountNeeded > 0 && iridiumAmountNeeded > 0)
                            break;
                    }

                    //if we got here and didn't change anything, break out
                    if (!changedAssignment)
                        break;
                }//while (oreAmountNeeded < 0 || iridiumAmountNeeded < 0)
            }
        }

        private static void computerSetPlanetBuildGoals(Player player)
        {   
            //first look for planets that need build goals set, either for ships or for improvements

            List<Planet> planetCandidatesForNeedingImprovements = new List<Planet>();
            List<Planet> planetCandidatesForNeedingSpacePlatforms = new List<Planet>();
            List<Planet> planetCandidatesForNeedingShips = new List<Planet>();

            foreach(Planet p in player.OwnedPlanets.Values)
            {
                //if this planet doesn't already have a build goal in player.PlanetBuildGoals
                if (!player.PlanetBuildGoals.ContainsKey(p.Id))
                {
                    if (p.BuildQueue.Count <= 1)//even if we have something in queue we might want to set a goal to save up resources?
                    {
                        if (p.BuiltAndBuildQueueImprovementCount() < p.MaxImprovements)
                        {
                            planetCandidatesForNeedingImprovements.Add(p);
                        }
                        else if (player.CountPlanetsNeedingExploration() != 0)//if we need to explore some planets before building a space platform, do so
                        {
                            planetCandidatesForNeedingShips.Add(p);
                        }
                        else if (p.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count == 0)
                        {
                            planetCandidatesForNeedingSpacePlatforms.Add(p);
                        }
                        else
                        {
                            planetCandidatesForNeedingShips.Add(p);
                        }
                    }
                }
            }

            //space platforms
            foreach (Planet p in planetCandidatesForNeedingSpacePlatforms)
            {
                if (p.BuiltAndBuildQueueImprovementTypeCount(PlanetImprovementType.SpacePlatform) == 0)
                {
                    player.PlanetBuildGoals[p.Id] = new PlanetImprovement(PlanetImprovementType.SpacePlatform);
                }
            }

            //build improvements
            foreach (Planet p in planetCandidatesForNeedingImprovements)
            {
                //planet class 2 should have 3 farms and 2 mines
                //planet class 1 should have 2 farm and 1 mines
                //dead planets should have 0 farms and 1 mine
                //asterorids should have 0 farms and 1 mine
                //otherwise build 1 factory if none exist
                //otherwise build 1 colony if none exist
                //otherwise build factories to recommended amount
                //otherwise build a spaceport space platform is none exist
                //otherwise colonies till we're filled up

                int recommendedFarms = 0;
                int recommendedMines = 0;
                int recommendedFactories = 1;
                

                //NOTE: we aren't checking gold for the purposes of farms, we'll just build them

                if (p.Type == PlanetType.PlanetClass2)
                {
                    recommendedFarms = 3;
                    recommendedMines = 2;
                    recommendedFactories = 3;
                }
                else if (p.Type == PlanetType.PlanetClass1)
                {
                    recommendedFarms = 2;
                    recommendedFactories = 2;
                }
                else if (p.Type == PlanetType.DeadPlanet)
                {
                    recommendedMines = 1;
                }
                else if (p.Type == PlanetType.AsteroidBelt)
                {
                    recommendedMines = 1;
                }

                //make sure farms are built before mines
                if (p.BuiltAndBuildQueueImprovementTypeCount(PlanetImprovementType.Farm) < recommendedFarms)
                {
                    player.PlanetBuildGoals[p.Id] = new PlanetImprovement(PlanetImprovementType.Farm);
                }
                else if (p.BuiltAndBuildQueueImprovementTypeCount(PlanetImprovementType.Mine) < recommendedMines)
                {
                    player.PlanetBuildGoals[p.Id] = new PlanetImprovement(PlanetImprovementType.Mine);
                }
                else if (p.BuiltAndBuildQueueImprovementTypeCount(PlanetImprovementType.Factory) == 0)
                {
                    player.PlanetBuildGoals[p.Id] = new PlanetImprovement(PlanetImprovementType.Factory);
                }
                else if (p.BuiltAndBuildQueueImprovementTypeCount(PlanetImprovementType.Colony) == 0)
                {
                    player.PlanetBuildGoals[p.Id] = new PlanetImprovement(PlanetImprovementType.Colony);
                }
                else if (p.BuiltAndBuildQueueImprovementTypeCount(PlanetImprovementType.Factory) < recommendedFactories)
                {
                    player.PlanetBuildGoals[p.Id] = new PlanetImprovement(PlanetImprovementType.Factory);
                }
                else if (p.BuiltAndBuildQueueImprovementCount() < p.MaxImprovements)//just to double check
                {
                    player.PlanetBuildGoals[p.Id] = new PlanetImprovement(PlanetImprovementType.Colony);
                }

                //after all that we should be ready to set fleet goals
            }

            //build ships
            foreach (Planet p in planetCandidatesForNeedingShips)
            {
                //defenders and destroyers will be built at random for the easier computers
                bool buildDefenders = false;
                bool buildDestroyers = false;
                if(player.Type == PlayerType.Computer_Easy)
                {
                    //50% chance to build defenders, 50% chance for destroyers
                    buildDefenders = (GameTools.Randomizer.Next(0, 4) <= 1);
                    buildDestroyers = (!buildDefenders && GameTools.Randomizer.Next(0, 4) <= 1);
                }
                else if (player.Type == PlayerType.Computer_Normal)
                {
                    //25% chance to build defenders, 25% chance for destroyers
                    buildDefenders = (GameTools.Randomizer.Next(0, 4) == 0);
                    buildDestroyers = (!buildDefenders && GameTools.Randomizer.Next(0, 4) == 0);
                }

                if (p.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count > 0 && !buildDefenders)
                {
                    int rand = GameTools.Randomizer.Next(4);
                    //build battleships at half the planets with spaceplatforms
                    if (rand < 2)
                    {
                        if (rand % 2 == 0)//1/4 the time we build battleshipts 1/2 time build destroyers
                            player.PlanetBuildGoals[p.Id] = new StarShipInProduction(StarShipType.Battleship);
                        else
                            player.PlanetBuildGoals[p.Id] = new StarShipInProduction(StarShipType.Destroyer);
                    }
                    else 
                    {
                        if (rand % 2 == 1)//1/4 the time we build cruisers 1/2 time build destroyers
                            player.PlanetBuildGoals[p.Id] = new StarShipInProduction(StarShipType.Cruiser);
                        else
                            player.PlanetBuildGoals[p.Id] = new StarShipInProduction(StarShipType.Destroyer);
                    }
                }
                //if there are unexplored planets still, build some scouts
                else if (player.CountPlanetsNeedingExploration() != 0)
                {
                    player.PlanetBuildGoals[p.Id] = new StarShipInProduction(StarShipType.Scout);
                }
                else if (p.BuiltImprovements[PlanetImprovementType.Factory].Count > 0 && buildDestroyers)
                {
                    //NOTE: this actually never gets hit because right now we're always building scouts, then spaceplatforms, then above applies
                    player.PlanetBuildGoals[p.Id] = new StarShipInProduction(StarShipType.Destroyer);
                }
                //else create defender (but only sometimes so we save gold)
                else if (GameTools.GameModel.Turn.Number % 4 == 0 && buildDefenders)
                {
                    player.PlanetBuildGoals[p.Id] = new StarShipInProduction(StarShipType.SystemDefense);
                }

            }

        }

        private static void computerBuildImprovementsAndShips(Player player)
        {

            //determine gold surplus needed to ship food
            int goldSurplus = player.LastTurnFoodNeededToBeShipped;

            //increase recommended goldSurplus based on computer difficulty to further make the easier computers a bit less agressive
            switch (player.Type)
            {
                case PlayerType.Computer_Easy:
                    //goldSurplus += (player.OwnedPlanets.Count - 1);
                    goldSurplus = GameTools.Randomizer.Next(0, (goldSurplus + 1)/4);//this should make the easy computer even easier, because sometimes he should starve himself
                    break;
                case PlayerType.Computer_Normal:
                    //goldSurplus += (player.OwnedPlanets.Count - 1 / 2);
                    goldSurplus = GameTools.Randomizer.Next(0, (goldSurplus + 1)/2);//this should make the normal computer easier, because sometimes he should starve himself
                    break;
                case PlayerType.Computer_Hard:
                    goldSurplus += (player.OwnedPlanets.Count - 1 / 2);
                    break;
                case PlayerType.Computer_Expert:
                    goldSurplus += (player.OwnedPlanets.Count - 1 / 4);
                    break;
            }

            //build improvements and ships based on build goals
            foreach (Planet p in player.OwnedPlanets.Values)
            {
                if (p.BuildQueue.Count == 0)
                {
                    if (player.PlanetBuildGoals.ContainsKey(p.Id))
                    {
                        PlanetProductionItem ppi = player.PlanetBuildGoals[p.Id];
                        //check resources
                        if (player.Resources.GoldAmount - ppi.GoldCost > goldSurplus &&
                            player.Resources.OreAmount - ppi.OreCost >= 0 &&
                            player.Resources.IridiumAmount - ppi.IridiumCost >= 0)
                        {
                            p.EnqueueProductionItemAndSpendResources(ppi, player.Resources);
                            player.PlanetBuildGoals.Remove(p.Id);
                        }
                    }
                    else//could this be a problem?
                    {
                        continue;
                    }
                }

            }

        }

        private static void computerSendShips(Player player)
        {
            //easy computer sends ships to closest planet at random
            //normal computers keep detachments of ships as defence as deemed necessary based on scouted enemy forces and planet value
            //hard computers also prefer planets based on class, location, and fleet defence
            //expert computers also amass fleets at strategic planets,
            //when two planets have ship building capabilities (i.e. have at least one factory),
            //and a 3rd desired planet is unowned, the further of the two owned planets sends it's ships to the closer as reinforcements

            //all but easy computers will also re-scout enemy planets after a time to re-establish intelligence

            List<Planet> planetCandidatesForSendingShips = new List<Planet>();
            foreach (Planet p in player.OwnedPlanets.Values)
            {

                if (p.PlanetaryFleet.GetPlanetaryFleetMobileStarshipCount() > 0)
                {
                    if (player.Type == PlayerType.Computer_Easy)//easy computers can send ships as long as there is somthing to send
                    {
                        planetCandidatesForSendingShips.Add(p);
                    }
                    else
                    {

                        int strengthToDefend = 0;

                        if (player.CountPlanetsNeedingExploration() != 0)
                        {
                            //this is done because of how the goals are set right now,
                            //we don't want the computer defending with all of it's ships when there is exploring to be done
                            strengthToDefend = 0; 
                        }
                        else if (p.BuiltImprovements[PlanetImprovementType.Factory].Count > 0)//if we can build ships it is probably later in the game and we should start defending this planet
                        {
                            strengthToDefend = (int)(Math.Pow(((int)p.Type), 2) * 4);//defense based on planet type
                        }

                        if (player.Type == PlayerType.Computer_Hard || player.Type == PlayerType.Computer_Expert)
                        {
                            //base defense upon enemy fleet strength within a certain range of last known planets
                            // as well as if there are ships in queue and estimated time till production

                            //TODO: we should get all enemy planets within a certain range instead of just the closest one
                            int distance = 0;
                            Planet closestUnownedPlanet = GameTools.GetClosestUnownedPlanet(player, p, out distance);
                            if (closestUnownedPlanet != null)
                            {
                                if (player.LastKnownPlanetFleetStrength.ContainsKey(closestUnownedPlanet.Id))
                                {
                                    strengthToDefend += player.LastKnownPlanetFleetStrength[closestUnownedPlanet.Id].Fleet.DetermineFleetStrength(false);
                                }
                                else if (player.KnownPlanets.ContainsKey(closestUnownedPlanet.Id))
                                {
                                    strengthToDefend += (int)(Math.Pow(((int)closestUnownedPlanet.Type), 2) * 4); ;
                                }
                            }

                            int turnsToComplete = 99;
                            int starshipStrength = 0;
                            if (p.BuildQueueContainsMobileStarship(out turnsToComplete, out starshipStrength))
                            {
                                if (turnsToComplete <= distance + 1)//if we can build this before an enemy can get here
                                {
                                    strengthToDefend -= starshipStrength;
                                }
                            }
                        }

                        if (p.PlanetaryFleet.DetermineFleetStrength() > strengthToDefend)
                        {
                            planetCandidatesForSendingShips.Add(p);//TODO: for some computer levels below we should also leave a defending detachment based on strength to defend, etc...
                        }
                    }
                }

            }


            List<Planet> planetCandidatesForInboundScouts = new List<Planet>();
            List<Planet> planetCandidatesForInboundAttackingFleets = new List<Planet>();
            if (planetCandidatesForSendingShips.Count > 0)
            {
                foreach (Planet p in GameTools.GameModel.Planets)
                {
                    if (p.Owner != player && !player.PlanetContainsFriendlyInboundFleet(p))//exploring/attacking inbound fleets to unowned planets should be excluded
                    {
                        if (!player.KnownPlanets.ContainsKey(p.Id) ||
                            //also check to see if we need to update intelligence
                            (player.Type != PlayerType.Computer_Easy &&
                            player.LastKnownPlanetFleetStrength.ContainsKey(p.Id) &&
                            (GameTools.GameModel.Turn.Number - player.LastKnownPlanetFleetStrength[p.Id].TurnLastExplored) > 20))//TODO: figure out best value here (could be shorter if planets are closer together)
                        {
                            planetCandidatesForInboundScouts.Add(p);
                        }
                        else if (!player.PlanetOwnedByPlayer(p))//TODO: we might still want to gather fleets strategically
                        {
                            planetCandidatesForInboundAttackingFleets.Add(p);
                        }
                    }
                }
            }


            //computer should send one available ship to unexplored planets (TODO: later build scouts/destroyers as appropriate for this)
            //computer should gather fleets strategically at fronts close to unowned planets (TODO: later base this on last known force strength)
            //
            //new send ship logic:
            // for each planet that can send ships
            //  get list of the closest unowned planets
            //   if it is unexplored (and we don't already have an inbound fleet), send a one ship detachment
            //   if it is explored and if we have more strength than the last known strength on planet (and we don't already have an inbound fleet), send a detachment


            //first sort planet candidates for inbound fleets by closest to home planet
            if (player.HomePlanet != null)//just to make sure
            {
                if (player.Type == PlayerType.Computer_Easy || player.Type == PlayerType.Computer_Normal)
                {
                    planetCandidatesForInboundAttackingFleets.Sort(new PlanetDistanceComparer(player.HomePlanet));
                    planetCandidatesForInboundScouts.Sort(new PlanetDistanceComparer(player.HomePlanet));
                }
                else
                {
                    //hard and expert computer will sort with a bit of complexly (based on value and last known strength as well as distance)
                    planetCandidatesForInboundAttackingFleets.Sort(new PlanetValueDistanceStrengthComparer(player.HomePlanet, player.LastKnownPlanetFleetStrength));
                    planetCandidatesForInboundScouts.Sort(new PlanetValueDistanceStrengthComparer(player.HomePlanet, player.LastKnownPlanetFleetStrength));
                }

            }

            List<Planet> planetCandidatesForInboundReinforcements = new List<Planet>();
            if (player.Type == PlayerType.Computer_Expert)
            {
                foreach (Planet p in player.OwnedPlanets.Values)
                {
                    if (p.BuiltImprovements[PlanetImprovementType.Factory].Count > 0)
                    {
                        planetCandidatesForInboundReinforcements.Add(p);
                    }
                }
            }

            //next for each candidate for inbound attacking fleets, sort the candidates for sending ships by closest first

            //look for closest planet to attack first
            for (int i = planetCandidatesForInboundAttackingFleets.Count - 1; i >= 0; i--)
            {
                Planet pEnemyInbound = planetCandidatesForInboundAttackingFleets[i];

                if (player.Type == PlayerType.Computer_Easy || player.Type == PlayerType.Computer_Normal)
                {
                    planetCandidatesForSendingShips.Sort(new PlanetDistanceComparer(pEnemyInbound));
                }
                else// harder computers should start with planets with more ships and/or reinforce closer planets from further planets with more ships
                {
                    planetCandidatesForSendingShips.Sort(new PlanetValueDistanceStrengthComparer(pEnemyInbound, player.LastKnownPlanetFleetStrength));
                    //because the PlanetValueDistanceStrengthComparer prefers weakest planets, we want the opposite in this case
                    //so we want to prefer sending from asteroid belts with high strength value
                    planetCandidatesForSendingShips.Reverse();
                }

                //in order to slow the agression of the easier computers we want to only attack when we have a multiple of the enemy fleet
                double additionalStrengthMultiplierNeededToAttackLow = 0.5;
                double additionalStrengthMultiplierNeededToAttackHigh = 1.0;
                switch (player.Type)
                {
                    case PlayerType.Computer_Easy:
                        additionalStrengthMultiplierNeededToAttackLow = 3.0;
                        additionalStrengthMultiplierNeededToAttackHigh = 6.0;
                        break;
                    case PlayerType.Computer_Normal:
                        additionalStrengthMultiplierNeededToAttackLow = 2.0;
                        additionalStrengthMultiplierNeededToAttackHigh = 4.0;
                        break;
                    case PlayerType.Computer_Hard:
                        additionalStrengthMultiplierNeededToAttackLow = 1.0;
                        additionalStrengthMultiplierNeededToAttackHigh = 2.0;
                        break;
                }

                double additionalStrengthMultiplierNeededToAttack = GameTools.Randomizer.Next((int)(additionalStrengthMultiplierNeededToAttackLow * 10), (int)(additionalStrengthMultiplierNeededToAttackHigh * 10) + 1) / 10.0;

                bool fleetSent = false;
                for (int j = planetCandidatesForSendingShips.Count - 1; j >= 0; j--)
                {
                    Planet pFriendly = planetCandidatesForSendingShips[j];

                    //send attacking fleet

                    //rely only on our last known-information
                    int fleetStrength = (int)(Math.Pow((((int)pEnemyInbound.Type) + 1), 2) * 4);//estimate required strength based on planet type
                    if (player.LastKnownPlanetFleetStrength.ContainsKey(pEnemyInbound.Id))
                    {
                        fleetStrength = player.LastKnownPlanetFleetStrength[pEnemyInbound.Id].Fleet.DetermineFleetStrength();
                    }

                    int scouts = pFriendly.PlanetaryFleet.StarShips[StarShipType.Scout].Count;
                    int destroyers = pFriendly.PlanetaryFleet.StarShips[StarShipType.Destroyer].Count;
                    int cruisers = pFriendly.PlanetaryFleet.StarShips[StarShipType.Cruiser].Count;
                    int battleships = pFriendly.PlanetaryFleet.StarShips[StarShipType.Battleship].Count;

                    //TODO: for some computer levels below we should also leave a defending detachment based on strength to defend, etc...

                    //generate this fleet just to ensure strength > destination fleet strength
                    Fleet newFleet = StarShipFactoryHelper.GenerateFleetWithShipCount(0, scouts, destroyers, cruisers, battleships, pFriendly.BoundingHex);
                    if (newFleet.DetermineFleetStrength() > (fleetStrength * additionalStrengthMultiplierNeededToAttack))
                    {
                        newFleet = pFriendly.PlanetaryFleet.SplitFleet(scouts, destroyers, cruisers, battleships);

                        newFleet.SetDestination(pFriendly.BoundingHex, pEnemyInbound.BoundingHex);

                        pFriendly.OutgoingFleets.Add(newFleet);

                        if (AI.OnComputerSentFleet != null)
                        {
                            AI.OnComputerSentFleet(newFleet);
                        }

                        int mobileStarshipsLeft = pFriendly.PlanetaryFleet.GetPlanetaryFleetMobileStarshipCount();
                        if (mobileStarshipsLeft == 0)
                        {
                            planetCandidatesForSendingShips.RemoveAt(j);
                        }

                        fleetSent = true;
                        break;
                    }
                }

                if (!fleetSent && planetCandidatesForInboundReinforcements.Count > 0)
                {
                    //here is where we reinforce close planets for expert computers

                    //logic:
                    //  find closest planet capable of building better ships (has at least one factory) to enemy planet
                    //  send a detachment from each planetCandidatesForSendingShips other than closest ship builder to reinforce and amass for later

                    planetCandidatesForInboundReinforcements.Sort(new PlanetDistanceComparer(pEnemyInbound));
                    Planet planetToReinforce = planetCandidatesForInboundReinforcements[planetCandidatesForInboundReinforcements.Count - 1];
                    int distanceFromPlanetToReinforceToEnemy = GameTools.GameModel.GameGrid.GetHexDistance(pEnemyInbound.BoundingHex, planetToReinforce.BoundingHex);

                    for (int r = planetCandidatesForSendingShips.Count - 1; r >= 0; r--)
                    {
                        Planet pFriendly = planetCandidatesForSendingShips[r];

                        if (pFriendly.Id == planetToReinforce.Id)//don't reinforce ourselves
                            break;

                        //also make sure the friendly planet is further from our target than the planet to reinforce
                        if (GameTools.GameModel.GameGrid.GetHexDistance(pEnemyInbound.BoundingHex, pFriendly.BoundingHex) < distanceFromPlanetToReinforceToEnemy)
                            break;

                        int scouts = pFriendly.PlanetaryFleet.StarShips[StarShipType.Scout].Count;
                        int destroyers = pFriendly.PlanetaryFleet.StarShips[StarShipType.Destroyer].Count;
                        int cruisers = pFriendly.PlanetaryFleet.StarShips[StarShipType.Cruiser].Count;
                        int battleships = pFriendly.PlanetaryFleet.StarShips[StarShipType.Battleship].Count;

                        //TODO: for some computer levels below we should also leave a defending detachment based on strength to defend, etc...

                        Fleet newFleet = StarShipFactoryHelper.GenerateFleetWithShipCount(0, scouts, destroyers, cruisers, battleships, pFriendly.BoundingHex);

                        newFleet = pFriendly.PlanetaryFleet.SplitFleet(scouts, destroyers, cruisers, battleships);

                        newFleet.SetDestination(pFriendly.BoundingHex, planetToReinforce.BoundingHex);

                        pFriendly.OutgoingFleets.Add(newFleet);

                        if (AI.OnComputerSentFleet != null)
                        {
                            AI.OnComputerSentFleet(newFleet);
                        }

                        int mobileStarshipsLeft = pFriendly.PlanetaryFleet.GetPlanetaryFleetMobileStarshipCount();
                        if (mobileStarshipsLeft == 0)
                        {
                            planetCandidatesForSendingShips.RemoveAt(r);
                        }

                        fleetSent = true;
                        break;

                    }
                }

                if (planetCandidatesForSendingShips.Count == 0)
                    break;

            }//end planetCandidatesForInboundAttackingFleets loop

            if (planetCandidatesForSendingShips.Count > 0)
            {
                for (int i = planetCandidatesForInboundScouts.Count - 1; i >= 0; i--)
                {
                    Planet pEnemyInbound = planetCandidatesForInboundScouts[i];

                    if (player.Type == PlayerType.Computer_Easy || player.Type == PlayerType.Computer_Normal)
                    {
                        planetCandidatesForSendingShips.Sort(new PlanetDistanceComparer(pEnemyInbound));
                    }
                    else// harder computers should start with planets with more ships and/or reinforce closer planets from further planets with more ships
                    {
                        planetCandidatesForSendingShips.Sort(new PlanetValueDistanceStrengthComparer(pEnemyInbound, player.LastKnownPlanetFleetStrength));
                        //because the PlanetValueDistanceStrengthComparer prefers weakest planets, we want the opposite in this case
                        //so we want to prefer sending from asteroid belts with high strength value
                        planetCandidatesForSendingShips.Reverse();
                    }

                    //bool fleetSent = false;
                    for (int j = planetCandidatesForSendingShips.Count - 1; j >= 0; j--)
                    {
                        Planet pFriendly = planetCandidatesForSendingShips[j];


                        //send smallest detachment possible
                        Planet inboundPlanet = planetCandidatesForInboundScouts[i];
                        Fleet newFleet = pFriendly.PlanetaryFleet.SplitOffSmallestPossibleFleet();
                        //if we do this right newFleet should never be null
                        {
                            newFleet.SetDestination(pFriendly.BoundingHex, inboundPlanet.BoundingHex);
                            pFriendly.OutgoingFleets.Add(newFleet);
                            if (AI.OnComputerSentFleet != null)
                            {
                                AI.OnComputerSentFleet(newFleet);
                            }

                            int mobileStarshipsLeft = pFriendly.PlanetaryFleet.GetPlanetaryFleetMobileStarshipCount();
                            if (mobileStarshipsLeft == 0)
                            {
                                planetCandidatesForSendingShips.RemoveAt(j);
                            }

                            //fleetSent = true;
                            break;
                        }

                    }

                    if (planetCandidatesForSendingShips.Count == 0)
                        break;
                }
            }
        }
    }
}
