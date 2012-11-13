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
using System.Text;
using SystemMaster.Library;

namespace SystemMaster
{
    public partial class PlanetaryConflictControl : UserControl, IDialogWindowControl
    {
        TurnEventMessage planetaryConflictMessage;

        public PlanetaryConflictControl(TurnEventMessage planetaryConflictMessage)
        {
            InitializeComponent();

            this.planetaryConflictMessage = planetaryConflictMessage;

            StringBuilder sb = new StringBuilder();
            sb.Append(planetaryConflictMessage.Message + "\r\n\r\n");
            int attackingFleetStrength = planetaryConflictMessage.Data.AttackingFleet.DetermineFleetStrength();
            sb.Append("Attacking Fleet (strength " + attackingFleetStrength + ", Chance to Win: " + planetaryConflictMessage.Data.AttackingFleetChances + "%): \r\n");
            sb.Append(planetaryConflictMessage.Data.AttackingFleet + "\r\n\r\n");
            int defendingFleetStrength = planetaryConflictMessage.Data.DefendingFleet.DetermineFleetStrength();
            sb.Append("Defending Fleet (strength " + defendingFleetStrength + "): \r\n");
            sb.Append(planetaryConflictMessage.Data.DefendingFleet + "\r\n\r\n");
            sb.Append("Ships Remaining: \r\n");
            sb.Append(planetaryConflictMessage.Data.WinningFleet + "\r\n");

            this.PlanetaryConflictSummary.Text = sb.ToString();

            if (!GameTools.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups)
                NeverShowPlanetaryConflictPopupsCheckbox.IsChecked = true;
        }

        public void OKClose()
        {
            GameTools.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups = true;
            if (NeverShowPlanetaryConflictPopupsCheckbox.IsChecked.HasValue && NeverShowPlanetaryConflictPopupsCheckbox.IsChecked.Value)
            {
                GameTools.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups = false;
            }
        }

        public void CancelClose()
        {
            //do nothing
        }
    }
}
