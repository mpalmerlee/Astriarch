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
    public class TurnEventMessage
    {
        public TurnEventMessageType Type;
        public string Message;
        public Planet Planet = null;//only populated if applies to message

        public PlanetaryConflictData Data = null;//for planetary conflict type messages

        public TurnEventMessage(TurnEventMessageType type, Planet p, string message)
        {
            this.Type = type;
            this.Planet = p;
            this.Message = message;
        }
    }

    public class TurnEventMessageComparer : IComparer<TurnEventMessage>
    {
        #region IComparer<Planet> Members

        int IComparer<TurnEventMessage>.Compare(TurnEventMessage a, TurnEventMessage b)
        {
            if (a.Type > b.Type)
                return -1;
            else if (a.Type < b.Type)
                return 1;
            return 0;
        }
        #endregion
    }

    public enum TurnEventMessageType
    {
        FoodShipped = 0,
        PopulationGrowth = 1,
        ImprovementBuilt = 2,
        ShipBuilt = 3,
        ImprovementDemolished = 4,
        InsufficientFood = 5,//either general food shortage or couldn't ship becuase of lack of gold, leads to population unrest
        BuildQueueEmpty = 6,
        DefendedAgainstAttackingFleet = 7,
        AttackingFleetLost = 8,
        PlanetCaptured = 9,
        PopulationStarvation = 10,
        FoodShortageRiots = 11,
        PlanetLostDueToStarvation = 12,//this is bad but you probably know it's bad
        PlanetLost = 13
    }

    public class PlanetaryConflictData
    {
        public Player DefendingPlayer;
        public Fleet DefendingFleet;
        public Player AttackingPlayer;
        public Fleet AttackingFleet;
        public Fleet WinningFleet = null;
        public int AttackingFleetChances = 0;//percentage chance the attacking fleet will win

        public PlanetaryConflictData(Player defendingPlayer, Fleet defendingFleet, Player attackingPlayer, Fleet attackingFleet)
        {
            this.DefendingPlayer = defendingPlayer;
            this.DefendingFleet = defendingFleet.CloneFleet();
            this.AttackingPlayer = attackingPlayer;
            this.AttackingFleet = attackingFleet.CloneFleet();
        }
    }
}
