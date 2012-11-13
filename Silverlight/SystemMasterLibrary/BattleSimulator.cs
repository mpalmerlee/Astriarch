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
    public static class BattleSimulator
    {
        public const int STARSHIP_WEAPON_POWER = 2;//defenders have one gun and battleships have 16
        public const int STARSHIP_WEAPON_POWER_HALF = 1;

        

        //new simulate fleet battle logic:
        //this will support choosing a target and allowing ships to have an advantage over other types
        //here are the advantages (-> means has an advantage over):
        //defenders -> destroyers -> battleships -> cruisers -> spaceplatforms -> scouts (-> defenders)
        //target decisions:
        //if there is an enemy ship that this ship has an advantage over, choose that one
        //otherwise go for the weakest ship to shoot at

        public static bool? SimulateFleetBattle(Fleet f1, Fleet f2)
        {
            bool? fleet1Wins = null;

            

            //fleet damage pending structures are so we can have both fleets fire simultaneously without damaging each-other till the end
            Dictionary<StarShip, int> fleet1DamagePending = new Dictionary<StarShip, int>();
            int fleet1SpacePlatformDamagePending = 0;
            Dictionary<StarShip, int> fleet2DamagePending = new Dictionary<StarShip, int>();
            int fleet2SpacePlatformDamagePending = 0;


            while (f1.DetermineFleetStrength(true) > 0 && f2.DetermineFleetStrength(true) > 0)
            {
                List<StarShip> f1StarShips = f1.GetAllStarShips();
                List<StarShip> f2StarShips = f2.GetAllStarShips();

                foreach (StarShip s in f1StarShips)
                {
                    StarshipFireWeapons(s.Strength, s.Type, false, f2StarShips, f2.HasSpacePlatform, fleet2DamagePending, ref fleet2SpacePlatformDamagePending);
                }
                if (f1.HasSpacePlatform)
                    StarshipFireWeapons(Fleet.SPACE_PLATFORM_STRENGTH - f1.SpacePlatformDamage, StarShipType.SystemDefense, true, f2StarShips, f2.HasSpacePlatform, fleet1DamagePending, ref fleet2SpacePlatformDamagePending);

                foreach (StarShip s in f2StarShips)
                {
                    StarshipFireWeapons(s.Strength, s.Type, false, f1StarShips, f1.HasSpacePlatform, fleet1DamagePending, ref fleet1SpacePlatformDamagePending);
                }
                if (f2.HasSpacePlatform)
                    StarshipFireWeapons(Fleet.SPACE_PLATFORM_STRENGTH - f2.SpacePlatformDamage, StarShipType.SystemDefense, true, f1StarShips, f1.HasSpacePlatform, fleet1DamagePending, ref fleet1SpacePlatformDamagePending);

                //deal damage
                foreach(KeyValuePair<StarShip, int> f1KVP in fleet1DamagePending)
                {
                    f1KVP.Key.DamageAmount += f1KVP.Value;
                }
                fleet1DamagePending.Clear();

                foreach (KeyValuePair<StarShip, int> f2KVP in fleet2DamagePending)
                {
                    f2KVP.Key.DamageAmount += f2KVP.Value;
                }
                fleet2DamagePending.Clear();

                f1.ReduceFleet(fleet1SpacePlatformDamagePending);
                f2.ReduceFleet(fleet2SpacePlatformDamagePending);
            }
            
            if (f1.DetermineFleetStrength(true) > 0)
                fleet1Wins = true;
            else if (f2.DetermineFleetStrength(true) > 0)
                fleet1Wins = false;

            return fleet1Wins;
        }

        public static void StarshipFireWeapons(int strength, StarShipType type, bool isSpacePlatform, List<StarShip> enemyFleet, bool enemyHasSpacePlatform, Dictionary<StarShip, int> fleetDamagePending, ref int enemyFleetSpacePlatformDamagePending)
        {
            List<StarShip> workingEnemyFleet = new List<StarShip>(enemyFleet.Count);
            workingEnemyFleet.AddRange(enemyFleet);
            workingEnemyFleet.Sort(new StarShipAdvantageStrengthComparer(type, isSpacePlatform, fleetDamagePending));
            

            for (int iGun = 0; iGun < strength; iGun += BattleSimulator.STARSHIP_WEAPON_POWER)
            {
                //remove any in the enemy fleet with strength - pending damage <= 0
                for (int i = workingEnemyFleet.Count - 1; i >= 0 ; i--)
                {
                    StarShip enemy = workingEnemyFleet[i];
                    int pendingDamage = 0;
                    if(fleetDamagePending.ContainsKey(enemy))
                        pendingDamage = fleetDamagePending[enemy];
                    if(workingEnemyFleet[i].Strength - pendingDamage <=0)
                        workingEnemyFleet.RemoveAt(i);
                }

                //choose target
                if (enemyHasSpacePlatform && ((type == StarShipType.Cruiser) || workingEnemyFleet.Count == 0))//shoot at the space platform
                {
                    //calculate starship max damage
                    int maxDamage = BattleSimulator.STARSHIP_WEAPON_POWER;
                    if (StarshipHasAdvantageBasedOnType(isSpacePlatform, type, true, StarShipType.SystemDefense))
                        maxDamage += BattleSimulator.STARSHIP_WEAPON_POWER_HALF;
                    else if (StarshipHasDisadvantageBasedOnType(isSpacePlatform, type, true, StarShipType.SystemDefense))
                        maxDamage -= BattleSimulator.STARSHIP_WEAPON_POWER_HALF;

                    int damage = GameTools.Randomizer.Next(0, maxDamage + 1);

                    enemyFleetSpacePlatformDamagePending += damage;
                }
                else if (workingEnemyFleet.Count > 0)
                {
                    StarShip target = workingEnemyFleet[0];
                    //calculate starship max damage
                    int maxDamage = BattleSimulator.STARSHIP_WEAPON_POWER;
                    if (StarshipHasAdvantageBasedOnType(isSpacePlatform, type, false, target.Type))
                        maxDamage += BattleSimulator.STARSHIP_WEAPON_POWER_HALF;
                    else if (StarshipHasDisadvantageBasedOnType(isSpacePlatform, type, false, target.Type))
                        maxDamage -= BattleSimulator.STARSHIP_WEAPON_POWER_HALF;

                    int damage = GameTools.Randomizer.Next(0, maxDamage + 1);

                    if (damage != 0)
                    {
                        if (!fleetDamagePending.ContainsKey(target))
                            fleetDamagePending.Add(target, 0);
                        fleetDamagePending[target] += damage;
                    }
                }
            }
        }

        public static bool StarshipHasAdvantageBasedOnType(bool attackerIsSpacePlatform, StarShipType sstAttacker, bool defenderIsSpacePlatform, StarShipType sstDefender)
        {
            if (attackerIsSpacePlatform || defenderIsSpacePlatform)
            {
                if (attackerIsSpacePlatform && sstDefender == StarShipType.Destroyer)
                    return true;
                else if (defenderIsSpacePlatform && sstAttacker == StarShipType.Cruiser)
                    return true;
            }
            else if (sstAttacker == StarShipType.SystemDefense && sstDefender == StarShipType.Battleship)
                return true;
            else if (sstAttacker == StarShipType.Battleship && sstDefender == StarShipType.Cruiser)
                return true;
            else if (sstAttacker == StarShipType.Destroyer && sstDefender == StarShipType.Scout)
                return true;
            else if (sstAttacker == StarShipType.Scout && sstDefender == StarShipType.SystemDefense)
                return true;
            

            return false;
        }

        public static bool StarshipHasDisadvantageBasedOnType(bool attackerIsSpacePlatform, StarShipType sstAttacker, bool defenderIsSpacePlatform, StarShipType sstDefender)
        {
            if (attackerIsSpacePlatform || defenderIsSpacePlatform)
            {
                if (attackerIsSpacePlatform && sstDefender == StarShipType.Cruiser)
                    return true;
                else if (defenderIsSpacePlatform && sstAttacker == StarShipType.Destroyer)
                    return true;
            }
            else if (sstAttacker == StarShipType.Battleship && sstDefender == StarShipType.SystemDefense)
                return true;
            else if (sstAttacker == StarShipType.Cruiser && sstDefender == StarShipType.Battleship)
                return true;
            else if (sstAttacker == StarShipType.Scout && sstDefender == StarShipType.Destroyer)
                return true;
            else if (sstAttacker == StarShipType.SystemDefense && sstDefender == StarShipType.Scout)
                return true;


            return false;
        }

        /*
        /// <summary>
        /// Simulates a battle between two fleets
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="fleet1Strength"></param>
        /// <param name="f2"></param>
        /// <param name="fleet2Strength"></param>
        /// <returns>true if fleet 1 wins, null if neither win (both destroyed)</returns>
        public static bool? SimulateFleetBattleOld(Fleet f1, int fleet1Strength, Fleet f2, int fleet2Strength)
        {
            bool? fleet1Wins = null;
            List<StarShip> f1StarShips = f1.GetAllStarShips();
            List<StarShip> f2StarShips = f2.GetAllStarShips();
            while (fleet1Strength > 0 && fleet2Strength > 0)
            {
                int damageToFleet1 = 0;
                int damageToFleet2 = 0;

                foreach (StarShip s in f1StarShips)
                {
                    damageToFleet2 += GameTools.Randomizer.Next(0, s.Strength + 1);
                }
                if (f1.HasSpacePlatform)
                    damageToFleet2 += GameTools.Randomizer.Next(0, Fleet.SPACE_PLATFORM_STRENGTH - f1.SpacePlatformDamage + 1);

                foreach (StarShip s in f2StarShips)
                {
                    damageToFleet1 += GameTools.Randomizer.Next(0, s.Strength + 1);
                }
                if (f2.HasSpacePlatform)
                    damageToFleet1 += GameTools.Randomizer.Next(0, Fleet.SPACE_PLATFORM_STRENGTH - f2.SpacePlatformDamage + 1);

                //deal damage
                f1.ReduceFleet(damageToFleet1, fleet1Strength);
                fleet1Strength -= damageToFleet1;

                f2.ReduceFleet(damageToFleet2, fleet2Strength);
                fleet2Strength -= damageToFleet2;
            }
            if (fleet1Strength > 0)
                fleet1Wins = true;
            else if (fleet2Strength > 0)
                fleet1Wins = false;

            return fleet1Wins;
        }
        */

        /*
        public static bool? OldResolvePlanetaryConflictsFunction(Fleet playerFleet, int playerFleetStrength, Fleet enemyFleet, int enemyFleetStrength)
        {
            //determine strength differences
            // GameTools.BATTLE_RANDOMNESS_FACTOR = 4 in this case
            //if one fleet's strength is 4 (log base 16 4 = .5) times as strong or more that fleet automatically wins
            //  damage done to winning fleet is (strength of loser / strength Multiplier) +- some randomness
            //if neither fleet is 4 times as strong as the other or more, we have to roll the dice (preferring the stronger fleet) for who wins

            //if the player's fleet is destroyed the enemy (defender) always wins because you can't land fleets and capture the system without fleets

            bool? playerWins = null;
            if (enemyFleetStrength > playerFleetStrength * GameTools.BATTLE_RANDOMNESS_FACTOR)
            {
                planetaryConflictData.AttackingFleetChances = 0;
                playerWins = false;

                enemyFleet.ReduceFleetBasedOnStrength(enemyFleetStrength, playerFleetStrength);
            }
            else if (playerFleetStrength > enemyFleetStrength * GameTools.BATTLE_RANDOMNESS_FACTOR)
            {
                planetaryConflictData.AttackingFleetChances = 100;
                playerWins = true;

                bool playerFleetDestroyed = playerFleet.ReduceFleetBasedOnStrength(playerFleetStrength, enemyFleetStrength);
                if (playerFleetDestroyed)
                {
                    playerWins = false;
                    enemyFleet.ReduceFleetBasedOnStrength(enemyFleetStrength, playerFleetStrength);
                }
            }
            else//randomly choose winner but prefer the player with more strength
            {
                //algorithm for chance:
                // If the strength is equal 50% chance
                // otherwize % chance = 50 + LOG base 25(greater fleet strength / lesser fleet strength)

                int randomnessUpperBounds = 0;
                if (playerFleetStrength == enemyFleetStrength)
                {
                    planetaryConflictData.AttackingFleetChances = 50;
                    //even odds
                    playerWins = GameTools.Randomizer.Next(0, 2) == 0;
                }
                else if (playerFleetStrength > enemyFleetStrength)
                {
                    //prefer player
                    double extraPercentageChance = Math.Log(playerFleetStrength / (double)enemyFleetStrength, Math.Pow(GameTools.BATTLE_RANDOMNESS_FACTOR, 2)) * 100;//((playerFleetStrength - enemyFleetStrength) / (double)enemyFleetStrength) * 50;
                    randomnessUpperBounds = 50 + (int)Math.Round(extraPercentageChance);
                    planetaryConflictData.AttackingFleetChances = randomnessUpperBounds;
                    int pickedRandom = GameTools.Randomizer.Next(0, 100);
                    playerWins = pickedRandom < randomnessUpperBounds;
                }
                else
                {
                    //prefer enemy
                    double extraPercentageChanceEnemy = Math.Log(enemyFleetStrength / (double)playerFleetStrength, Math.Pow(GameTools.BATTLE_RANDOMNESS_FACTOR, 2)) * 100;//((enemyFleetStrength - playerFleetStrength) / (double)playerFleetStrength) * 50;
                    randomnessUpperBounds = 50 + (int)Math.Round(extraPercentageChanceEnemy);
                    planetaryConflictData.AttackingFleetChances = 100 - randomnessUpperBounds;
                    int pickedRandom = GameTools.Randomizer.Next(0, 100);
                    playerWins = !(pickedRandom < randomnessUpperBounds);
                }

                //reduce fleet of winner (the looser gets destroyed so we don't need to reduce it)
                if (playerWins.Value)
                {
                    bool playerFleetDestroyed = playerFleet.ReduceFleetBasedOnStrength(playerFleetStrength, enemyFleetStrength);
                    if (playerFleetDestroyed)
                    {
                        playerWins = false;
                    }
                }

                if (!playerWins.Value)
                    enemyFleet.ReduceFleetBasedOnStrength(enemyFleetStrength, playerFleetStrength);

                
            }
            return playerWins;
        }* */
    }
}
