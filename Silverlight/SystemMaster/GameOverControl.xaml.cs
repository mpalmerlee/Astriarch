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
    public partial class GameOverControl : UserControl, IDialogWindowControl
    {
        private Player winingPlayer;

        public GameOverControl(Player winningPlayer, bool mainPlayerWon)
        {
            InitializeComponent();

            this.winingPlayer = winningPlayer;

            StringBuilder sb = new StringBuilder();
            if (mainPlayerWon)
                sb.Append("You conquered all of your enemies and reign supreme over the known universe!\r\nYou will be known as the first Astriarch - Ruler of the Stars.\r\n");
            else
                sb.Append("You lost control over all your fleets and planets and have been crushed by the power of your enemies!\r\n");

            sb.Append("In " + GameTools.GameModel.Turn.Number + " turns.\r\n");
            
            sb.Append(winingPlayer.Name + " won the game with " + winningPlayer.OwnedPlanets.Count + " planets.\r\n");
            sb.Append("\r\nYour points: " + GameTools.CalculateEndGamePoints(GameTools.GameModel.MainPlayer, mainPlayerWon));
            this.GameOverSummary.Text = sb.ToString();
        }

        public void OKClose()
        {
            //do nothing
        }

        public void CancelClose()
        {
            //do nothing
        }
    }
}
