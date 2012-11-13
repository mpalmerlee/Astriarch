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
using SystemMaster.Library;

namespace SystemMaster
{
    public partial class StartGameOptions : UserControl
    {
        public string PlayerName = "Player1";
        public int OpponentCount = 1;
        public PlayerType Computer1Difficulty = PlayerType.Computer_Easy;
        public PlayerType Computer2Difficulty = PlayerType.Computer_Easy;
        public PlayerType Computer3Difficulty = PlayerType.Computer_Easy;
        public int SystemCount = 2;
        public PlanetsPerSystemOption PlanetsPerSystem = PlanetsPerSystemOption.FOUR;

        public StartGameOptions()
        {
            InitializeComponent();

            this.initializeComputerDiffCombos();

            this.PlanetsPerSystemComboBox.Items.Add(new PlanetsPerSystemComboBoxItem(PlanetsPerSystemOption.FOUR));
            this.PlanetsPerSystemComboBox.Items.Add(new PlanetsPerSystemComboBoxItem(PlanetsPerSystemOption.FIVE));
            this.PlanetsPerSystemComboBox.Items.Add(new PlanetsPerSystemComboBoxItem(PlanetsPerSystemOption.SIX));
            this.PlanetsPerSystemComboBox.Items.Add(new PlanetsPerSystemComboBoxItem(PlanetsPerSystemOption.SEVEN));
            this.PlanetsPerSystemComboBox.Items.Add(new PlanetsPerSystemComboBoxItem(PlanetsPerSystemOption.EIGHT));
            this.PlanetsPerSystemComboBox.SelectedIndex = 0;

            this.OpponentCountRadio1.Checked += new RoutedEventHandler(OpponentCountRadio1_Checked);
            this.OpponentCountRadio2.Checked += new RoutedEventHandler(OpponentCountRadio2_Checked);
            this.OpponentCountRadio3.Checked += new RoutedEventHandler(OpponentCountRadio3_Checked);

            this.StartGameOptionsOkButton.Click += new RoutedEventHandler(StartGameOptionsOkButton_Click);
        }

        void StartGameOptionsOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.PlayerName = this.PlayerNameTextBox.Text;

            this.OpponentCount = 1;
            if (OpponentCountRadio2.IsChecked.Value)
                this.OpponentCount = 2;
            else if (OpponentCountRadio3.IsChecked.Value)
                this.OpponentCount = 3;

            this.Computer1Difficulty = ((ComputerDifficultyComboBoxItem)this.Computer1DifficultyComboBox.SelectedItem).ComputerDifficulty;
            this.Computer2Difficulty = ((ComputerDifficultyComboBoxItem)this.Computer2DifficultyComboBox.SelectedItem).ComputerDifficulty;
            this.Computer3Difficulty = ((ComputerDifficultyComboBoxItem)this.Computer3DifficultyComboBox.SelectedItem).ComputerDifficulty;

            this.SystemCount = 2;
            if (this.StarfieldRadio2.IsChecked.Value)
                this.SystemCount = 3;
            else if (this.StarfieldRadio3.IsChecked.Value)
                this.SystemCount = 4;

            this.PlanetsPerSystem = ((PlanetsPerSystemComboBoxItem)this.PlanetsPerSystemComboBox.SelectedItem).PlanetsPerSystem;
        }

        void OpponentCountRadio1_Checked(object sender, RoutedEventArgs e)
        {
            this.refreshAfterOpponentCountSelected();
        }

        void OpponentCountRadio2_Checked(object sender, RoutedEventArgs e)
        {
            this.refreshAfterOpponentCountSelected();
        }

        void OpponentCountRadio3_Checked(object sender, RoutedEventArgs e)
        {
            this.refreshAfterOpponentCountSelected();
        }

        private void refreshAfterOpponentCountSelected()
        {
            int numberOfOpponents = 1;
            if (OpponentCountRadio2.IsChecked.Value)
                numberOfOpponents = 2;
            else if (OpponentCountRadio3.IsChecked.Value)
                numberOfOpponents = 3;

            this.Computer2Panel.Visibility = Visibility.Collapsed;
            this.Computer3Panel.Visibility = Visibility.Collapsed;

            this.StarfieldPanel1.Visibility = Visibility.Visible;
            this.StarfieldPanel2.Visibility = Visibility.Visible;

            if (numberOfOpponents >= 2)
            {
                this.Computer2Panel.Visibility = Visibility.Visible;
                this.StarfieldPanel1.Visibility = Visibility.Collapsed;
                this.StarfieldRadio2.IsChecked = true;
            }
            if (numberOfOpponents >= 3)
            {
                this.Computer3Panel.Visibility = Visibility.Visible;
                this.StarfieldPanel2.Visibility = Visibility.Collapsed;
                this.StarfieldRadio3.IsChecked = true;
            }
        }

        private void initializeComputerDiffCombos()
        {
            List<PlayerType> computerDifficultyPlayerTypes = new List<PlayerType>();
            computerDifficultyPlayerTypes.Add(PlayerType.Computer_Easy);
            computerDifficultyPlayerTypes.Add(PlayerType.Computer_Normal);
            computerDifficultyPlayerTypes.Add(PlayerType.Computer_Hard);
            computerDifficultyPlayerTypes.Add(PlayerType.Computer_Expert);

            foreach (PlayerType pt in computerDifficultyPlayerTypes)
            {
                this.Computer1DifficultyComboBox.Items.Add(new ComputerDifficultyComboBoxItem(pt));
                this.Computer2DifficultyComboBox.Items.Add(new ComputerDifficultyComboBoxItem(pt));
                this.Computer3DifficultyComboBox.Items.Add(new ComputerDifficultyComboBoxItem(pt));
            }

            this.Computer1DifficultyComboBox.SelectedIndex = 0;
            this.Computer2DifficultyComboBox.SelectedIndex = 0;
            this.Computer3DifficultyComboBox.SelectedIndex = 0;
        }

    }

    public class PlanetsPerSystemComboBoxItem : ComboBoxItem
    {
        public PlanetsPerSystemOption PlanetsPerSystem = PlanetsPerSystemOption.FOUR;

        public PlanetsPerSystemComboBoxItem(PlanetsPerSystemOption pps) : base()
        {
            this.PlanetsPerSystem = pps;
            this.Content = this.ToString();
        }

        public override string ToString()
        {
            return ((int)this.PlanetsPerSystem).ToString();
        }
    }

    public class ComputerDifficultyComboBoxItem : ComboBoxItem
    {
        public PlayerType ComputerDifficulty = PlayerType.Computer_Easy;

        public ComputerDifficultyComboBoxItem(PlayerType pt) : base()
        {
            this.ComputerDifficulty = pt;
            this.Content = this.ToString();
        }

        public override string ToString()
        {
            string difficultyString = "";
            switch (this.ComputerDifficulty)
            {
                case PlayerType.Computer_Easy:
                    difficultyString = "Easy";
                    break;
                case PlayerType.Computer_Normal:
                    difficultyString = "Normal";
                    break;
                case PlayerType.Computer_Hard:
                    difficultyString = "Hard";
                    break;
                case PlayerType.Computer_Expert:
                    difficultyString = "Expert";
                    break;

            }
            return difficultyString;
        }
    }
}
