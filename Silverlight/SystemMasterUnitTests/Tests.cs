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
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SystemMaster.Library;

namespace SystemMasterUnitTests
{
    [TestClass]
    public class Tests
    {
        //here are the advantages (-> means has an advantage over):
        //defenders -> destroyers -> battleships -> cruisers -> spaceplatforms -> scouts (-> defenders)
        [TestMethod]
        public void TestSimulateFleetBattleScoutVDefenders()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 1, 0, 0, 0, null);
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(2, 0, 0, 0, 0, null);
            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
            if (!f1Wins.HasValue || !f1Wins.Value)
                Assert.Fail("Scout didn't win against two defenders!");
        }

        [TestMethod]
        public void TestSimulateFleetBattleDefendersVScout()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(2, 0, 0, 0, 0, null);
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 1, 0, 0, 0, null);
            
            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
            if (!f1Wins.HasValue || f1Wins.Value)
                Assert.Fail("Scout didn't win against two defenders!");
        }

        [TestMethod]
        public void TestSimulateFleetBattleDefendersVBattleship()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(16, 0, 0, 0, 0, null);
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 0, 0, 1, null);

            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
            if (!f1Wins.HasValue || !f1Wins.Value)
                Assert.Fail("Defenders didn't win against a battleship!");
        }

        [TestMethod]
        public void TestSimulateFleetBattleBattleshipVCruisers()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 0, 2, 0, null);
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 0, 0, 1, null);

            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
            if (!f1Wins.HasValue || f1Wins.Value)
                Assert.Fail("Cruisers won against a battleship!");
        }

        [TestMethod]
        public void TestSimulateFleetBattleCruisersVSpacePlatform()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 0, 4, 0, null);
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 0, 0, 0, null);
            f2.HasSpacePlatform = true;

            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
            if (!f1Wins.HasValue || !f1Wins.Value)
                Assert.Fail("Cruisers didn't win against a space platform!");
        }

        [TestMethod]
        public void TestSimulateFleetBattleSpacePlatformVDestroyers()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 8, 0, 0, null);
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 0, 0, 0, null);
            f2.HasSpacePlatform = true;

            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
            if (!f1Wins.HasValue || f1Wins.Value)
                Assert.Fail("Space platform didn't defeat the destroyers!");
        }

        [TestMethod]
        public void TestSimulateFleetBattleDestroyersVScouts()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 1, 0, 0, null);
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 2, 0, 0, 0, null);

            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
            if (!f1Wins.HasValue || !f1Wins.Value)
                Assert.Fail("Destroyers didn't defeat the scouts!");
        }

        [TestMethod]
        public void TestSimulateFleetBattleComboFleetF1Disadvantage()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(16, 2, 0, 0, 0, null);
            f1.HasSpacePlatform = true;
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 0, 1, 4, 1, null);

            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
            if (!f1Wins.HasValue || f1Wins.Value)
                Assert.Fail("The disadvantaged fleet won!");
        }

        [TestMethod]
        public void TestSimulateFleetBattleComboFleetEven()
        {
            Fleet f1 = StarShipFactoryHelper.GenerateFleetWithShipCount(16, 2, 1, 0, 1, null);
            f1.HasSpacePlatform = true;
            Fleet f2 = StarShipFactoryHelper.GenerateFleetWithShipCount(0, 2, 5, 4, 1, null);

            bool? f1Wins = BattleSimulator.SimulateFleetBattle(f1, f2);
        }
    }
}