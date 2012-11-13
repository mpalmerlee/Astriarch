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
    //special events are for assiging a random property to a planet for example
    //or random galactic event
    public class GameSpecialEventModifier
    {
        private string name = "";
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        private string description = "";
        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        private double floatData = 0.0;
        public double FloatData
        {
            get { return this.floatData; }
            set { this.floatData = value; }
        }

        public GameSpecialEventModifier(string name, string description)
        {
            this.name = name;
            this.description = description;


        }
    }
    
    public enum GameSpecialEventType : int
    {
        //for increasing output type events floatData will be the percentage amount 0.05 for 5%
        //for a deacrease in output use -0.05 for floatData
        PLANET_EFFECT_INCREASE_GOLD_OUTPUT = 1,
        PLANET_EFFECT_INCREASE_FARMER_OUTPUT = 2,
        PLANET_EFFECT_INCREASE_MINER_OUTPUT = 3,
        PLANET_EFFECT_INCREASE_WORKER_OUTPUT = 4,
        GLOBAL_EVENT_ALL_PLAYERS_INCREASE_GOLD_OUTPUT = 5,
        GLOBAL_EVENT_ALL_PLAYERS_INCREASE_FARMER_OUTPUT = 6,
        GLOBAL_EVENT_ALL_PLAYERS_INCREASE_MINER_OUTPUT = 7,
        GLOBAL_EVENT_ALL_PLAYERS_INCREASE_WORKER_OUTPUT = 8,
        //Ship effect?


    }


    public enum GameSpecialEventTargetType : int
    {
        PLANET,

    }

    public class Turn
    {
        private int number = 1;
        public int Number
        {
            get { return this.number; }
        }

        public Turn()
        {

        }

        public void Next()
        {
            this.number++;
        }

        public override string ToString()
        {
            return this.number + "";
        }

    }

    public enum PlanetsPerSystemOption : int
    {
        FOUR = 4,
        FIVE = 5,
        SIX = 6,
        SEVEN = 7,
        EIGHT = 8
    }

    public class Model
    {
        public bool ShowUnexploredPlanetsAndEnemyPlayerStats = false;//for debugging for now, could eventually be used once a scanner is researched?

        public int SystemsToGenerate = 4;

        public PlanetsPerSystemOption PlanetsPerSystem = PlanetsPerSystemOption.FOUR;
        //public bool EnsureEachSystemContainsAllPlanetTypes = true;//TODO: implement if false every planet type (except home) will be randomly selected

        public Turn Turn;

        public Player MainPlayer = null;//for now the local human player, this may have to change for multi-player
        public List<Player> Players;
        
        public Grid GameGrid;
        public List<Planet> Planets;

        private Size galaxySize;
        

        public List<Rect> Quadrants;
        public List<List<Rect>> SubQuadrants;

        public Model(List<Player> players, Player mainPlayer, int systemsToGenerate, PlanetsPerSystemOption planetsPerSystem)
        {
            this.Turn = new Turn();

            this.Players = players;
            this.MainPlayer = mainPlayer;
            this.SystemsToGenerate = systemsToGenerate;
            this.PlanetsPerSystem = planetsPerSystem;

            this.GameGrid = new Grid(615.0, 480.0);//TODO: externalize later
            this.galaxySize = new Size(615.0, 480.0);//TODO: externalize later

            this.Planets = new List<Planet>();

            //build quadrants
            this.Quadrants = new List<Rect>();
            this.SubQuadrants = new List<List<Rect>>();

            double x, y, width, height;
            x = 0;
            y = 0;
            width = galaxySize.Width / 2;
            height = galaxySize.Height / 2;
            if (this.SystemsToGenerate == 2 || this.SystemsToGenerate == 4)
            {
                //[0 , 0]
                this.Quadrants.Add(new Rect(x, y, width, height));
                //[0 , 1]
                x = width;
                this.Quadrants.Add(new Rect(x, y, width, height));
                //[1 , 1]
                y = height;
                this.Quadrants.Add(new Rect(x, y, width, height));
                //[1 , 0]
                x = 0;
                this.Quadrants.Add(new Rect(x, y, width, height));
            }
            else//we're generating 3 systems in a triangle arrangement
            {
                //[0 , 0]
                this.Quadrants.Add(new Rect(x, y, width, height));
                //[0 , 1]
                x = width;
                this.Quadrants.Add(new Rect(x, y, width, height));
                //[middle]
                x = width/2;
                y = height;
                this.Quadrants.Add(new Rect(x, y, width, height));
            }

            this.populatePlanets();
        }

        private void populatePlanets()
        {
            

            for (int q = 0; q < this.Quadrants.Count; q++)
            {
                Rect r = this.Quadrants[q];

                //build sub-quadrants for each quadrant
                List<Rect> subQuadrants = new List<Rect>(4);
                double subWidth = r.Width / 2;
                double subHeight = r.Height / 2;
                //insert for the corner sub-quadrants
                //this will prefer this sub-quadrant for the home planet

                Rect r0Sub = new Rect(r.X, r.Y, subWidth, subHeight);
                subQuadrants.Add(r0Sub);

                Rect r1Sub = new Rect(r.X + subWidth, r.Y, subWidth, subHeight);
                if(q == 1)
                    subQuadrants.Insert(0, r1Sub);
                else
                    subQuadrants.Add(r1Sub);

                Rect r2Sub = new Rect(r.X + subWidth, r.Y + subHeight, subWidth, subHeight);
                if (q == 2)
                    subQuadrants.Insert(0, r2Sub);
                else
                    subQuadrants.Add(r2Sub);

                Rect r3Sub = new Rect(r.X, r.Y + subHeight, subWidth, subHeight);
                if(q == 3)
                    subQuadrants.Insert(0, r3Sub);
                else
                    subQuadrants.Add(r3Sub);


                this.SubQuadrants.Add(subQuadrants);//save this just for debugging

                //get a list of hexes inside this quadrant
                List<List<Hexagon>> subQuadrantHexes = new List<List<Hexagon>>();
                List<List<Hexagon>> subQuadrantBoundedHexes = new List<List<Hexagon>>();
                for (int iSQ = 0; iSQ < subQuadrants.Count; iSQ++)
                {
                    Rect sub = subQuadrants[iSQ];
                    subQuadrantHexes.Add(new List<Hexagon>());
                    subQuadrantBoundedHexes.Add(new List<Hexagon>());
                    foreach (Hexagon h in this.GameGrid.Hexes)
                    {
                        if (sub.Left != h.TopLeftPoint.X && sub.Top != h.TopLeftPoint.Y &&
                            sub.Right != h.BottomRightPoint.X && sub.Bottom != h.BottomRightPoint.Y &&
                            sub.Contains(h.TopLeftPoint) && sub.Contains(h.BottomRightPoint))
                        {
                            //the hex is fully contained within this sub-quadrant
                            //and the hex doesn't lie on the outside of the sub-quadrant
                            subQuadrantHexes[iSQ].Add(h);
                            subQuadrantBoundedHexes[iSQ].Add(h);
                        }
                        else if (sub.Contains(h.TopLeftPoint) && sub.Contains(h.BottomRightPoint))
                        {
                            //the hex is inside the quadrand and possibly an outlier on the edge
                            subQuadrantBoundedHexes[iSQ].Add(h);
                        }
                    }
                }

                if (this.SystemsToGenerate == 2 && (q == 1 || q == 3))
                    continue;

                List<PlanetType> possiblePlanetTypes = new List<PlanetType>(4);
                possiblePlanetTypes.Add(PlanetType.PlanetClass1);
                possiblePlanetTypes.Add(PlanetType.DeadPlanet);
                possiblePlanetTypes.Add(PlanetType.AsteroidBelt);

                for (int iSQ = 0; iSQ < subQuadrants.Count; iSQ++)
                {
                    
                    //pick a planet bounding hex at random from the sub-quadrant (at lest for now)
                    int hexPos = GameTools.Randomizer.Next(0, subQuadrantHexes[iSQ].Count);
                    Hexagon planetBoundingHex = subQuadrantHexes[iSQ][hexPos];


                    //get at least one planet of each type, prefer the highest class planet
                    //int type = (Model.PLANETS_PER_QUADRANT - 1) - iSQ;
                    int type = 3;
                    PlanetType pt = PlanetType.PlanetClass2;
                    if (iSQ > 0 && possiblePlanetTypes.Count <= 3)
                    {
                        type = GameTools.Randomizer.Next(0, possiblePlanetTypes.Count);
                        pt = possiblePlanetTypes[type];
                        possiblePlanetTypes.RemoveAt(type);
                        
                    }
                    
                    Player initialPlanetOwner = null;

                    bool assignPlayer = false;
                    int assignedPlayerIndex = 0;
                    Color[] assignedPlayerColor = new Color[4];
                    assignedPlayerColor[0] = Color.FromArgb(255, 0, 255, 0);//Light Green
                    assignedPlayerColor[1] = Color.FromArgb(255, 200, 0, 200);//Light Purple
                    assignedPlayerColor[2] = Color.FromArgb(255, 0, 0, 255);//Light Blue
                    assignedPlayerColor[3] = Color.FromArgb(255, 255, 0, 0);//Light Red?
                    //it's a home planet, we'll see if we should assign a player
                    if (pt == PlanetType.PlanetClass2)
                    {
                        //TODO: make this more intelligent, based on # of players
                        if (q == 0)
                        {
                            assignPlayer = true;
                        }
                        else if (this.Players.Count == 2)
                        {
                            if (q == 2)
                            {
                                assignPlayer = true;
                                assignedPlayerIndex = 1;
                                
                            }
                        }
                        else if (q < this.Players.Count)
                        {
                            assignPlayer = true;
                            assignedPlayerIndex = q;
                        }
                    }

                    if (assignPlayer)
                    {
                        initialPlanetOwner = this.Players[assignedPlayerIndex];
                        initialPlanetOwner.Color = assignedPlayerColor[assignedPlayerIndex];
                    }

                    Planet p = new Planet(pt, planetBoundingHex.Id, planetBoundingHex, initialPlanetOwner);
                    

                    this.Planets.Add(p);
                }

                if (this.PlanetsPerSystem != PlanetsPerSystemOption.FOUR)
                {
                    int chanceQuadrant0 = 25;
                    int chanceQuadrant1 = 25;
                    int chanceQuadrant2 = 25;
                    int chanceQuadrant3 = 25;

                    int chanceToGetAsteroid = 50;
                    int chanceToGetDead = 34;
                    int chanceToGetClass1 = 16;

                    for (int iPlanet = 4; iPlanet < (int)this.PlanetsPerSystem; iPlanet++)
                    {
                        bool hexFound = false;

                        while (!hexFound)
                        {
                            //pick sub quadrant to put the planet in
                            //TODO: should we have another option to evenly space out the picking of quadrants? so that you don't end up with 5 planets in one quadrant potentially?

                            int iSQ = 0;
                            int max = chanceQuadrant0 + chanceQuadrant1 + chanceQuadrant2 + chanceQuadrant3;
                            if (GameTools.Randomizer.Next(0, max) > chanceQuadrant0)
                            {
                                iSQ = 1;
                                if (GameTools.Randomizer.Next(0, max) > chanceQuadrant1)
                                {
                                    iSQ = 2;
                                    if (GameTools.Randomizer.Next(0, max) > chanceQuadrant2)
                                    {
                                        iSQ = 3;
                                    }
                                }
                            }
                                

                            //pick a planet bounding hex at random from the sub-quadrant (at lest for now)
                            int hexPos = GameTools.Randomizer.Next(0, subQuadrantBoundedHexes[iSQ].Count);
                            Hexagon planetBoundingHex = subQuadrantBoundedHexes[iSQ][hexPos];

                            if (planetBoundingHex.PlanetContainedInHex != null)
                                continue;

                            //now that we've picked a quadrant, subtrack some off the chances for next time
                            switch (iSQ)
                            {
                                case 0:
                                    chanceQuadrant0 -= 6;
                                    break;
                                case 1:
                                    chanceQuadrant1 -= 6;
                                    break;
                                case 2:
                                    chanceQuadrant2 -= 6;
                                    break;
                                case 3:
                                    chanceQuadrant3 -= 6;
                                    break;
                            }

                            //hex has been found, now randomly choose planet type
                            //this logic prefers asteroids then dead then class1 and decreases our chances each time

                            PlanetType pt = PlanetType.PlanetClass1;
                            max = chanceToGetAsteroid + chanceToGetDead + chanceToGetClass1;
                            if (GameTools.Randomizer.Next(0, max) > chanceToGetClass1)
                            {
                                pt = PlanetType.DeadPlanet;
                                if (GameTools.Randomizer.Next(0, max) > chanceToGetDead)
                                {
                                    pt = PlanetType.AsteroidBelt;
                                    chanceToGetAsteroid -= 15;
                                }
                                else
                                    chanceToGetDead -= 15;
                            }
                            else
                                chanceToGetClass1 -= 15;

                            int type = GameTools.Randomizer.Next(0, 3);

                            Planet p = new Planet(pt, planetBoundingHex.Id, planetBoundingHex, null);

                            this.Planets.Add(p);

                            hexFound = true;
                        }
                    }
                }

            }//foreach quadrant

        }//populate planets

    }


}
