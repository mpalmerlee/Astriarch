using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;
using System.Windows.Controls.Primitives;
using SystemMaster.Library;
using SystemMaster.ControlLibrary;

namespace SystemMaster
{

    public partial class MainPage : UserControl
    {
        private Model model = null;
        private Dictionary<int, DrawnPlanet> canvasDrawnPlanets;//indexed by planet id

        private List<Fleet> canvasDrawnFleets;

        private List<TurnEventMessage> planetaryConflictMessages = new List<TurnEventMessage>();

        private Player mainPlayer = null;//TODO: change how this works for multiplayer

        private FontFamily displayFont = new FontFamily("Courier New");

        private bool drawQuadrants = false;//for debugging right now

        private bool sendingShipsTo = false;//for debugging right now
        private Hexagon sendShipsFromHex = null;

        private RandomTileImageCanvas interfaceBackground;

        private StartGameOptions startGameOptions;

        private ImageBrush imageBrushPopulationEmpty = new ImageBrush();
        private ImageBrush imageBrushPopulationFilled = new ImageBrush();

        private ImageBrush imageBrushPlanetaryImprovementEmpty = new ImageBrush();
        private ImageBrush imageBrushPlanetaryImprovementFilled = new ImageBrush();

        private ImageBrush imageBrushFarmer = new ImageBrush();
        private ImageBrush imageBrushMiner = new ImageBrush();
        private ImageBrush imageBrushBuilder = new ImageBrush();

        //for hex double click variables

        private bool hexClicked = false;
        private long hexLastClickedTime = 0;
        private Point hexLastClickedPoint = new Point();

        //end hex double click variables

        AudioInterface audioInterface = null;
        private System.Windows.Threading.DispatcherTimer mouseoutTimer = null;
        private System.Windows.Threading.DispatcherTimer mouseinTimer = null;

        BitmapImage speakerOffImage = new BitmapImage(new Uri("img/ico_speaker_off.png", UriKind.Relative));
        BitmapImage speakerOnImage = new BitmapImage(new Uri("img/ico_speaker_on.png", UriKind.Relative));

        public MainPage()
        {
            InitializeComponent();

            this.startGameOptions = new StartGameOptions();
            this.startGameOptions.StartGameOptionsOkButton.Click += new RoutedEventHandler(StartGameOptionsOkButton_Click);
            this.StartGameCanvas.Children.Add(this.startGameOptions);

            ThemeManager.ApplyStyle(this);

            this.audioInterface = new AudioInterface(MP3Start, MP3InGame1, MP3InGame2, MP3InGame3, MP3InGame4, MP3End);

            imageBrushPopulationEmpty.ImageSource = new BitmapImage(new Uri(@"img/PopulationSmallEmpty.png", UriKind.Relative));
            imageBrushPopulationFilled.ImageSource = new BitmapImage(new Uri(@"img/PopulationSmallFilled.png", UriKind.Relative));

            imageBrushPlanetaryImprovementEmpty.ImageSource = new BitmapImage(new Uri(@"img/PlanetaryImprovementSlotEmpty.png", UriKind.Relative));
            imageBrushPlanetaryImprovementFilled.ImageSource = new BitmapImage(new Uri(@"img/PlanetaryImprovementSlotFilled.png", UriKind.Relative));

            imageBrushFarmer.ImageSource = new BitmapImage(new Uri(@"img/Farmer.png", UriKind.Relative));
            imageBrushMiner.ImageSource = new BitmapImage(new Uri(@"img/Miner.png", UriKind.Relative));
            imageBrushBuilder.ImageSource = new BitmapImage(new Uri(@"img/Builder.png", UriKind.Relative));

            this.TurnDisplay.FontFamily = displayFont;
            this.TurnDisplay.FontSize = 14;

            this.SelectedItemStatus.FontFamily = displayFont;
            this.SelectedItemStatus.FontSize = 14;

            this.TextBlockPopulationAmount.FontFamily = displayFont;
            this.TextBlockPopulationAmount.FontSize = 24;
            this.TextBlockFoodAmount.FontFamily = displayFont;
            this.TextBlockFoodAmount.FontSize = 24;

            this.TextBlockGoldAmount.FontFamily = displayFont;
            this.TextBlockGoldAmount.FontSize = 24;
            this.TextBlockOreAmount.FontFamily = displayFont;
            this.TextBlockOreAmount.FontSize = 24;
            this.TextBlockIridiumAmount.FontFamily = displayFont;
            this.TextBlockIridiumAmount.FontSize = 24;

            this.TextBlockFarmCount.FontFamily = displayFont;
            this.TextBlockFarmCount.FontSize = 24;
            this.TextBlockMineCount.FontFamily = displayFont;
            this.TextBlockMineCount.FontSize = 24;
            this.TextBlockFactoryCount.FontFamily = displayFont;
            this.TextBlockFactoryCount.FontSize = 24;
            this.TextBlockColonyCount.FontFamily = displayFont;
            this.TextBlockColonyCount.FontSize = 24;
            this.TextBlockSpacePlatformCount.FontFamily = displayFont;
            this.TextBlockSpacePlatformCount.FontSize = 24;

            this.TextBlockDefenderCount.FontFamily = displayFont;
            this.TextBlockDefenderCount.FontSize = 16;
            this.TextBlockScoutCount.FontFamily = displayFont;
            this.TextBlockScoutCount.FontSize = 16;
            this.TextBlockDestroyerCount.FontFamily = displayFont;
            this.TextBlockDestroyerCount.FontSize = 16;
            this.TextBlockCruiserCount.FontFamily = displayFont;
            this.TextBlockCruiserCount.FontSize = 16;
            this.TextBlockBattleshipCount.FontFamily = displayFont;
            this.TextBlockBattleshipCount.FontSize = 16;


            this.interfaceBackground = new RandomTileImageCanvas(this.BackgroundCanvas);
            //ImageBrush ib = new ImageBrush();
            //ib.ImageSource = new BitmapImage(new Uri(@"img/large_background.jpg", UriKind.Relative));
            //ib.Stretch = Stretch.None;
            //this.LayoutRoot.Background = ib; 

            this.TurnSummaryItemsListBox.Background = DrawnPlanet.TRANSPARENT_BRUSH;
            this.TurnSummaryItemsListBox.BorderBrush = DrawnPlanet.TRANSPARENT_BRUSH;
            this.TurnSummaryItemsListBox.Foreground = DrawnPlanet.GREEN_BRUSH;

            ListBoxDoubleClick listBoxDoubleClick = new ListBoxDoubleClick(this.TurnSummaryItemsListBox);
            listBoxDoubleClick.DoubleClick += new MouseButtonEventHandler(listBoxDoubleClick_DoubleClick);

            this.TurnSummaryItemsListBox.SelectionChanged += new SelectionChangedEventHandler(TurnSummaryItemsListBox_SelectionChanged);
            //ControlTemplate ct = new ControlTemplate();

            //this.TurnSummaryItemsListBox.Template = ct;
            //DataTemplate dt = new DataTemplate();
            //<Border Name="border" BorderBrush="Aqua" BorderThickness="1" Padding="5" Margin="5">
            //Border b = new Border();
            //b.BorderBrush = DrawnPlanet.GREEN_BRUSH;


            this.PlayfieldCanvas.MouseLeftButtonDown += new MouseButtonEventHandler(PlayfieldCanvas_MouseLeftButtonDown);
            this.SendShipsButton.Click += new RoutedEventHandler(SendShipsButton_Click);
            this.CancelSendButton.Click += new RoutedEventHandler(CancelSendButton_Click);

            //TODO: make planetview double-click enabled
            this.PlanetViewButton.Click += new RoutedEventHandler(PlanetViewButton_Click);

            this.NextTurnButton.Click += new RoutedEventHandler(NextTurnButton_Click);

            this.MainMenuButton.Click += new RoutedEventHandler(MainMenuButton_Click);

            AI.OnComputerSentFleet += new AI.ComputerSentFleet(GameTools_OnComputerSentFleet);

            GameTools.OnPlayerDestroyed += new GameTools.PlayerDestroyed(GameTools_OnPlayerDestroyed);
            GameTools.OnPlayerDestroyedGameOver += new GameTools.PlayerDestroyed(GameTools_OnPlayerDestroyedGameOver);

            //write out version number
            try
            {
                string stAsmName = System.Reflection.Assembly.GetExecutingAssembly().FullName;
                System.Reflection.AssemblyName asmName = new System.Reflection.AssemblyName(stAsmName);

                Version v = asmName.Version;
                string version = v.Major + "." + v.Minor + "." + v.Build;
                this.SystemMasterVersion.Text = "v " + version;
                ToolTipService.SetToolTip(this.SystemMasterVersion, "Astriarch - Ruler of the Stars, Version: " + version + "\r\nCopyright 2010 Mastered Software\r\nMusic by Resonant");
            }
            catch{ }

            mouseoutTimer = new System.Windows.Threading.DispatcherTimer();
            mouseoutTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            mouseoutTimer.Tick += new EventHandler(mouseoutTimer_Tick);

            mouseinTimer = new System.Windows.Threading.DispatcherTimer();
            mouseinTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            mouseinTimer.Tick += new EventHandler(mouseinTimer_Tick);

            this.SliderVolume.MouseLeave += new MouseEventHandler(SliderVolume_MouseLeave);
            this.SliderVolume.MouseEnter += new MouseEventHandler(SliderVolume_MouseEnter);
            this.SliderVolume.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderVolume_ValueChanged);
            this.SpeakerIcon.MouseEnter += new MouseEventHandler(SpeakerIcon_MouseEnter);
            this.ButtonSpeakerIcon.MouseLeave += new MouseEventHandler(ButtonSpeakerIcon_MouseLeave);
            this.ButtonSpeakerIcon.Click += new RoutedEventHandler(ButtonSpeakerIcon_Click);

            this.audioInterface.StartMenuFirst();
        }

        void mouseinTimer_Tick(object sender, EventArgs e)
        {
            SliderVolume.Visibility = Visibility.Visible;
            this.mouseinTimer.Stop();
        }

        void mouseoutTimer_Tick(object sender, EventArgs e)
        {
            SliderVolume.Visibility = Visibility.Collapsed;
            this.mouseoutTimer.Stop();
        }

        void SliderVolume_MouseEnter(object sender, MouseEventArgs e)
        {
            this.mouseoutTimer.Stop();
        }

        void SliderVolume_MouseLeave(object sender, MouseEventArgs e)
        {
            this.mouseoutTimer.Start();
        }

        void SliderVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.audioInterface.Volume = (SliderVolume.Value / 100);
        }

        void ButtonSpeakerIcon_Click(object sender, RoutedEventArgs e)
        {
            this.audioInterface.Muted = !this.audioInterface.Muted;
            if (this.audioInterface.Muted)
            {
                SpeakerIcon.Source = this.speakerOffImage;
                ButtonSpeakerIconImage.Source = this.speakerOffImage;
                ToolTipService.SetToolTip(ButtonSpeakerIcon, "Unmute Audio");
            }
            else
            {
                SpeakerIcon.Source = this.speakerOnImage;
                ButtonSpeakerIconImage.Source = this.speakerOnImage;
                ToolTipService.SetToolTip(ButtonSpeakerIcon, "Mute Audio");
            }
        }

        void ButtonSpeakerIcon_MouseLeave(object sender, MouseEventArgs e)
        {
            SpeakerIcon.Visibility = Visibility.Visible;
            ButtonSpeakerIcon.Visibility = Visibility.Collapsed;
            this.mouseoutTimer.Start();
        }

        void SpeakerIcon_MouseEnter(object sender, MouseEventArgs e)
        {
            this.mouseoutTimer.Stop();

            SpeakerIcon.Visibility = Visibility.Collapsed;
            ButtonSpeakerIcon.Visibility = Visibility.Visible;
            this.mouseinTimer.Start();
        }

        void StartGameOptionsOkButton_Click(object sender, RoutedEventArgs e)
        {
            this.StartGameCanvas.Visibility = Visibility.Collapsed;

            this.PlanetViewButton.Visibility = Visibility.Visible;
            this.SendShipsButton.Visibility = Visibility.Visible;
            this.NextTurnButton.Visibility = Visibility.Visible;

            this.BottomStatusGrid.Visibility = Visibility.Visible;

            this.setupNewGame();
        }

        void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            this.audioInterface.StartMenu();

            this.StartGameCanvas.Visibility = Visibility.Visible;
            
            this.MainMenuButton.Visibility = Visibility.Collapsed;
        }

        void setupNewGame()
        {
            this.audioInterface.BeginGame();

            this.TurnDisplay.Text = "Turn 1, Year 3001";

            this.TurnSummaryItemsListBox.Items.Clear();

            this.PlanetViewButton.IsEnabled = false;//only enabled when an owned planet is selected
            this.SendShipsButton.IsEnabled = false;//only enabled when an owned planet with sendable ships is selected

            List<Player> players = new List<Player>();
            
            this.mainPlayer = new Player(PlayerType.Human, this.startGameOptions.PlayerName);//TODO: Make this configurable (and multiplayer!)
            players.Add(this.mainPlayer);
            players.Add(new Player(this.startGameOptions.Computer1Difficulty, "Computer 1"));
            if(this.startGameOptions.OpponentCount > 1)
                players.Add(new Player(this.startGameOptions.Computer2Difficulty, "Computer 2"));
            if (this.startGameOptions.OpponentCount > 2)
                players.Add(new Player(this.startGameOptions.Computer3Difficulty, "Computer 3"));

            this.PlayfieldCanvas.Children.Clear();

            //setup model
            this.model = GameTools.InitModel(players, this.mainPlayer, this.startGameOptions.SystemCount, this.startGameOptions.PlanetsPerSystem);
            this.canvasDrawnPlanets = new Dictionary<int, DrawnPlanet>();
            this.canvasDrawnFleets = new List<Fleet>();

            this.generateModel();

            this.updateCanvasForPlayer();

            this.updatePlayerStatusPanel();

            //select our home planet
            this.model.GameGrid.SelectHex(this.mainPlayer.HomePlanet.BoundingHex);
            this.updateSelectedItemPanelForPlanet();

            //this.ShowGridLines();
            
        }

        void listBoxDoubleClick_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            //popup the conflict dialog for list items relating to a conflict
            if (this.TurnSummaryItemsListBox.SelectedItem != null)
            {
                TurnSummaryMessageListBoxItem tsmlbi = (TurnSummaryMessageListBoxItem)TurnSummaryItemsListBox.SelectedItem;
                if (tsmlbi.EventMessage.Type == TurnEventMessageType.AttackingFleetLost ||
                    tsmlbi.EventMessage.Type == TurnEventMessageType.DefendedAgainstAttackingFleet ||
                    tsmlbi.EventMessage.Type == TurnEventMessageType.PlanetCaptured ||
                    tsmlbi.EventMessage.Type == TurnEventMessageType.PlanetLost)
                {
                    this.popupPlanetaryConflictControl(tsmlbi.EventMessage);
                }
            }
        }

        void TurnSummaryItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.TurnSummaryItemsListBox.SelectedItem != null)
            {
                TurnSummaryMessageListBoxItem tsmlbi = (TurnSummaryMessageListBoxItem)TurnSummaryItemsListBox.SelectedItem;
                if (tsmlbi.EventMessage != null && tsmlbi.EventMessage.Planet != null)
                {
                    this.selectPlanet(tsmlbi.EventMessage.Planet);
                }
            }
        }

        void GameTools_OnPlayerDestroyedGameOver(Player player)
        {
            this.audioInterface.EndGame();

            bool mainPlayerWins = (player == GameTools.GameModel.MainPlayer);
            GameOverControl goc = new GameOverControl(player, mainPlayerWins);
            DialogWindow dw = this.CreateDialogWindow(goc, "Game Over, Player: " + player.Name + " Wins!");
            dw.Closed += new EventHandler(GameOverControl_Closed);
            dw.Show();
        }

        void GameOverControl_Closed(object sender, EventArgs e)
        {
            //DialogWindow dw = (DialogWindow)sender;

            //show playfield after game over
            GameTools.GameModel.ShowUnexploredPlanetsAndEnemyPlayerStats = true;
            
            this.updateCanvasForPlayer();
            this.updateSelectedItemPanelForPlanet();

            this.PlanetViewButton.Visibility = Visibility.Collapsed;
            this.SendShipsButton.Visibility = Visibility.Collapsed;
            this.NextTurnButton.Visibility = Visibility.Collapsed;

            this.MainMenuButton.Visibility = Visibility.Visible;
        }

        void GameTools_OnPlayerDestroyed(Player player)
        {
            new AlertWindow("Player Destroyed", "Player: " + player.Name + " destroyed."); 
        }

        private void showPlanetView()
        {
            Planet p = this.model.GameGrid.SelectedHex.PlanetContainedInHex;

            PlanetViewControl pvc = new PlanetViewControl(p);
            DialogWindow dw = this.CreateDialogWindow(pvc, "Planet " + p.Name + " View", 563, 423);
            dw.Closed += new EventHandler(PlanetViewDialogWindow_Closed);
            dw.Show();
        }

        void PlanetViewButton_Click(object sender, RoutedEventArgs e)
        {
            this.showPlanetView();
        }

        void PlanetViewDialogWindow_Closed(object sender, EventArgs e)
        {
            DialogWindow dw = (DialogWindow)sender;

            if (dw.DialogResult == true)
            {
                this.updateSelectedItemPanelForPlanet();
                this.updatePlayerStatusPanel();//for total food per turn indicator and build queue updates
            }
        }

        void SendShipsButton_Click(object sender, RoutedEventArgs e)
        {
            this.sendingShipsTo = true;
            this.SendShipsStatus.Text = " Select Planet to Send Ships to. ";
            this.CancelSendButton.Visibility = Visibility.Visible;
            this.sendShipsFromHex = this.model.GameGrid.SelectedHex;
        }

        void CancelSendButton_Click(object sender, RoutedEventArgs e)
        {
            this.cancelSendShips();
        }

        void NextTurnButton_Click(object sender, RoutedEventArgs e)
        {
            this.model.Turn.Next();
            int year = this.model.Turn.Number + 3000;
            this.TurnDisplay.Text = "Turn " + this.model.Turn + ", Year " + year;

            List<TurnEventMessage> endOfTurnMessages = GameTools.EndTurns(model.Players);

            //gather our planetary conflict messages
            this.planetaryConflictMessages.Clear();
            //if we have the option enabled to show the planetary conflict dialog window at end of turn
            if (GameTools.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups)
            {
                foreach (TurnEventMessage tem in endOfTurnMessages)
                {
                    if (tem.Type == TurnEventMessageType.AttackingFleetLost ||
                        tem.Type == TurnEventMessageType.DefendedAgainstAttackingFleet ||
                        tem.Type == TurnEventMessageType.PlanetCaptured ||
                        tem.Type == TurnEventMessageType.PlanetLost)
                    {
                        if (tem.Data != null)//just to make sure
                            planetaryConflictMessages.Add(tem);
                    }
                }
            }

            this.updatePlayerStatusPanel();

            this.updateSelectedItemPanelForPlanet();

            this.updateCanvasForPlayer();

            this.TurnSummaryItemsListBox.Items.Clear();
            if (endOfTurnMessages.Count > 0)
            {
                /*
                    FoodShipped = 0,
                    PopulationGrowth = 1,
                    ImprovementBuilt = 2,
                    ShipBuilt= 3,
                    BuildQueueEmpty = 4,
                    PopulationStarvation = 5,
                    FoodShortageRiots = 6,
                    PlanetLostDueToStarvation = 7,//this is bad but you probably know it's bad
                    DefendedAgainstAttackingFleet = 8,
                    AttackingFleetLost = 9,
                    PlanetCaptured = 10,
                    PlanetLost = 11
                 */
                foreach (TurnEventMessage tev in endOfTurnMessages)
                {
                    TurnSummaryMessageListBoxItem tsmlbi = new TurnSummaryMessageListBoxItem(tev);
                    switch (tsmlbi.EventMessage.Type)
                    {
                        case TurnEventMessageType.FoodShipped:
                        case TurnEventMessageType.PopulationGrowth:
                            tsmlbi.Foreground = new SolidColorBrush(Colors.Blue);
                            break;
                        case TurnEventMessageType.DefendedAgainstAttackingFleet:
                            tsmlbi.Foreground = new SolidColorBrush(Colors.Purple);
                            break;
                        case TurnEventMessageType.BuildQueueEmpty:
                        case TurnEventMessageType.InsufficientFood:
                            tsmlbi.Foreground = new SolidColorBrush(Colors.Yellow);
                            break;
                        case TurnEventMessageType.PlanetCaptured:
                        case TurnEventMessageType.AttackingFleetLost:
                            tsmlbi.Foreground = new SolidColorBrush(Colors.Orange);
                            break;
                        case TurnEventMessageType.PlanetLost:
                        case TurnEventMessageType.PopulationStarvation:
                        case TurnEventMessageType.PlanetLostDueToStarvation:
                        case TurnEventMessageType.FoodShortageRiots:
                            tsmlbi.Foreground = new SolidColorBrush(Colors.Red);
                            break;
                    }
                    this.TurnSummaryItemsListBox.Items.Add(tsmlbi);
                }

                processNextEndOfTurnPlanetaryConflictMessage();
            }
        }

        private void processNextEndOfTurnPlanetaryConflictMessage()
        {
            int i = planetaryConflictMessages.Count - 1;
            if(i >= 0)
            {
                TurnEventMessage tem = planetaryConflictMessages[i];
                planetaryConflictMessages.RemoveAt(i);
                if (GameTools.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups)
                    this.popupPlanetaryConflictControl(tem);
            }
        }

        private void popupPlanetaryConflictControl(TurnEventMessage tem)
        {
            PlanetaryConflictControl pcc = new PlanetaryConflictControl(tem);
            string attackingPlayerName = tem.Data.AttackingPlayer.Name;
            string defendingPlayerName = GameTools.PlanetOwnerToFriendlyName(tem.Planet, tem.Data.DefendingPlayer);
            DialogWindow planetaryConflictDialogWindow = this.CreateDialogWindow(pcc, attackingPlayerName + " Attacked " + defendingPlayerName + " at Planet: " + tem.Planet.Name);
            planetaryConflictDialogWindow.Closed += new EventHandler(planetaryConflictDialogWindow_Closed);
            planetaryConflictDialogWindow.Show();
        }

        void planetaryConflictDialogWindow_Closed(object sender, EventArgs e)
        {
            processNextEndOfTurnPlanetaryConflictMessage();
        }

        private DialogWindow CreateDialogWindow(UserControl uc, string title)
        {
            return CreateDialogWindow(uc, title, 404, 303);
        }

        private DialogWindow CreateDialogWindow(UserControl uc, string title, double width, double height)
        {
            DialogWindow dw = new DialogWindow(uc, width, height);
            dw.Title = title;
            dw.HorizontalAlignment = HorizontalAlignment.Center;
            dw.VerticalAlignment = VerticalAlignment.Center;

            return dw;
        }

        //no GUI for this yet should be in options? or just for debugging?
        private void ShowGridLines()
        {
            this.model.Players[0].Options.ShowHexGrid = true;
            this.model.GameGrid.ShowHexGrid();
        }

        //no GUI for this yet should be in options? or just for debugging?
        private void HideGridLines()
        {
            this.model.Players[0].Options.ShowHexGrid = true;
            this.model.GameGrid.HideHexGrid();
        }

        private void selectPlanet(Planet p)
        {
            this.model.GameGrid.SelectHex(p.BoundingHex);
            this.updateSelectedItemPanelForPlanet();
        }

        private void cancelSendShips()
        {
            this.SendShipsStatus.Text = "";
            this.CancelSendButton.Visibility = Visibility.Collapsed;
            
            sendingShipsTo = false;
        }

        void PlayfieldCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.model == null)
                return;

            Point p = e.GetPosition(this.PlayfieldCanvas);

            Hexagon hClicked = this.model.GameGrid.GetHexAt(p);

            if (hClicked != null && hClicked.PlanetContainedInHex != null)
            {
                this.model.GameGrid.SelectHex(hClicked);
                this.updateSelectedItemPanelForPlanet();

                if (this.sendingShipsTo)//TODO: this could be cleaner, at least make it change the cursor so you know your sending ships
                {
                    if (this.sendShipsFromHex != this.model.GameGrid.SelectedHex)
                    {
                        int distance = this.model.GameGrid.GetHexDistance(this.sendShipsFromHex, this.model.GameGrid.SelectedHex);

                        //show select ships to send dialog
                        Planet p1 = this.sendShipsFromHex.PlanetContainedInHex;
                        Planet p2 = this.model.GameGrid.SelectedHex.PlanetContainedInHex;

                        SendShipsControl ssc = new SendShipsControl(p1, p2, distance);
                        string p1Name = p1.Name;
                        string p2Name = p2.Name;
                        DialogWindow sendShipsDialogWindow = this.CreateDialogWindow(ssc, "Sending Ships from " + p1Name + " to " + p2Name);
                        sendShipsDialogWindow.Closed += new EventHandler(sendShipsDialogWindow_Closed);
                        sendShipsDialogWindow.Show();
                    }
                    //"The hexes are " + distance + " spaces away.";
                    this.cancelSendShips();
                }

                if (hClicked.PlanetContainedInHex.Owner == this.mainPlayer)
                {
                    //check for double-click
                    long currentMillis = DateTime.Now.Ticks / 10000;

                    if (this.hexClicked)
                    {
                        this.hexClicked = false;
                        if (currentMillis - this.hexLastClickedTime < 500 && this.lastHexPointClose(e.GetPosition(null)))
                        {
                            //fire off planet view

                            this.showPlanetView();
                        }
                    }
                    else
                    {
                        this.hexClicked = true;
                        this.hexLastClickedTime = DateTime.Now.Ticks / 10000;
                    }
                    this.hexLastClickedPoint = e.GetPosition(null);
                }
            }
        }

        private bool lastHexPointClose(Point p)
        {
            bool bRet = false;

            if (Math.Abs(p.X - this.hexLastClickedPoint.X) < 3 && Math.Abs(p.Y - this.hexLastClickedPoint.Y) < 3)
            {
                bRet = true;
            }
            return bRet;
        }

        void sendShipsDialogWindow_Closed(object sender, EventArgs e)
        {
            DialogWindow dw = (DialogWindow)sender;

            SendShipsControl ssc = ((SendShipsControl)dw.ContentUserControl);

            if (dw.DialogResult == true)
            {
                //check to make sure a fleet was created and then add the appropriate controls to our canvas
                if (ssc.CreatedFleet != null)
                {
                    createFleetCanvasItems(ssc.CreatedFleet);

                    //update the canvas so we hide the fleets if there are no more ships in the planetary fleet
                    if (ssc.SourcePlanet.PlanetaryFleet.GetPlanetaryFleetMobileStarshipCount() == 0)
                    {
                        DrawnPlanet dp = this.canvasDrawnPlanets[ssc.SourcePlanet.Id];
                        dp.UpdatePlanetDrawingForPlayer(this.mainPlayer);
                    }
                }
            }
            
            this.selectPlanet(ssc.SourcePlanet);//reselect the source planet even if they hit ok
        }

        void CreatedFleet_OnFleetMoved(Fleet f)
        {
            f.TravelFleetRect.Fill = DrawnPlanet.BRUSH_FLEET;
        }

        void CreatedFleet_OnFleetMergedOrDestroyed(Fleet f)
        {
            //just cleanup our fleet from our canvas and list
            this.PlayfieldCanvas.Children.Remove(f.TravelFleetRect);
            this.PlayfieldCanvas.Children.Remove(f.TravelLine);
            this.PlayfieldCanvas.Children.Remove(f.TravelETATextBlock);
            this.canvasDrawnFleets.Remove(f);
        }

        private void createFleetCanvasItems(Fleet f)
        {
            this.canvasDrawnFleets.Add(f);
            
            this.PlayfieldCanvas.Children.Add(f.TravelFleetRect);
            this.PlayfieldCanvas.Children.Add(f.TravelLine);
            this.PlayfieldCanvas.Children.Add(f.TravelETATextBlock);
            f.OnFleetMergedOrDestroyed += new Fleet.FleetMergedOrDestroyed(CreatedFleet_OnFleetMergedOrDestroyed);
            f.OnFleetMoved += new Fleet.FleetMoved(CreatedFleet_OnFleetMoved);
        }

        void GameTools_OnComputerSentFleet(Fleet f)
        {
            if (GameTools.GameModel.ShowUnexploredPlanetsAndEnemyPlayerStats)
            {
                this.createFleetCanvasItems(f);
            }
        }

        private void updatePlayerStatusPanel()
        {
            if (this.mainPlayer != null)//this should never happen
            {
                this.OverallPlayerStatusGrid.Visibility = Visibility.Visible;

                int totalPopulation = this.mainPlayer.GetTotalPopulation();

                this.TextBlockPopulationAmount.Text = totalPopulation + "";

                int totalFoodAmount = this.mainPlayer.TotalFoodAmount;
                int totalFoodProduction = this.mainPlayer.GetTotalFoodProductionPerTurn();
                int foodDiffPerTurn = totalFoodProduction - totalPopulation;
                string foodDiffPositiveIndicator = "";
                if (foodDiffPerTurn >= 0)
                    foodDiffPositiveIndicator = "+";

                Color foodAmountColor = Colors.Green;
                if (foodDiffPerTurn < 0)
                {
                    if (foodDiffPerTurn + totalFoodAmount < totalPopulation)
                        foodAmountColor = Colors.Red;
                    else//we're not going to starve
                        foodAmountColor = Colors.Yellow;
                }
                else if(foodDiffPerTurn + totalFoodAmount < totalPopulation)
                    foodAmountColor = Colors.Orange;//we're still gaining food but we'll still starve

                this.TextBlockFoodAmount.Foreground = new SolidColorBrush(foodAmountColor);
                this.TextBlockFoodAmount.Text = totalFoodAmount + " " + foodDiffPositiveIndicator + foodDiffPerTurn + ""; ;

                this.TextBlockGoldAmount.Text = this.mainPlayer.Resources.GoldAmount + "";
                this.TextBlockOreAmount.Text = this.mainPlayer.Resources.OreAmount + "";
                this.TextBlockIridiumAmount.Text = this.mainPlayer.Resources.IridiumAmount + "";                
            }
        }

        /*
        private void updatePlayerStatusPanelText()
        {
            if (this.mainPlayer != null)//this should never happen
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(this.mainPlayer.Name + "\r\n");
                sb.Append(this.mainPlayer.Resources.GoldAmount + " Gold\r\n");
                sb.Append(this.mainPlayer.Resources.OreAmount + " Ore\r\n");
                sb.Append(this.mainPlayer.Resources.IridiumAmount + " Iridium\r\n");
                int totalPopulation = this.mainPlayer.GetTotalPopulation();
                sb.Append(totalPopulation + " Total Population\r\n");
                int totalFoodAmount = this.mainPlayer.TotalFoodAmount;
                int totalFoodProduction = this.mainPlayer.GetTotalFoodProductionPerTurn();
                int foodDiffPerTurn = totalFoodProduction - totalPopulation;
                string foodDiffPositiveIndicator = "";
                if (foodDiffPerTurn >= 0)
                    foodDiffPositiveIndicator = "+";
                sb.Append(totalFoodAmount + " Total Food (" + foodDiffPositiveIndicator + foodDiffPerTurn + ")\r\n");

                this.OverallPlayerStatus.Text = sb.ToString();
            }
        }
        */

        private void updateSelectedItemPopulationPanel(int populationCount, int maxPopulation)
        {
            ToolTipService.SetToolTip(this.SelectedItemPopulationPanel, "Planet Population: " + populationCount + " / " + maxPopulation);
            
            //clear out the appropriate ones
            for (int i = maxPopulation + 1; i <= 16; i++)
            {
                Rectangle image = (Rectangle)LayoutRoot.FindName("PopulationImage" + i);
                image.Fill = null;
            }

            for (int i = 1; i <= maxPopulation; i++)
            {
                Rectangle image = (Rectangle)LayoutRoot.FindName("PopulationImage" + i);
                if (i <= populationCount)
                    image.Fill = this.imageBrushPopulationFilled;
                else
                    image.Fill = this.imageBrushPopulationEmpty;
            }
        }

        private void updateSelectedItemImprovementSlotsPanel(int improvementCount, int maxImprovements)
        {
            ToolTipService.SetToolTip(this.SelectedItemImprovementSlotsPanel, "Planetary Improvements: " + improvementCount + " / " + maxImprovements);

            //clear out the appropriate ones
            for (int i = maxImprovements + 1; i <= 9; i++)
            {
                Rectangle image = (Rectangle)LayoutRoot.FindName("PlanetaryImprovementSlotsImage" + i);
                image.Fill = null;
            }

            for (int i = 1; i <= maxImprovements; i++)
            {
                Rectangle image = (Rectangle)LayoutRoot.FindName("PlanetaryImprovementSlotsImage" + i);
                if (i <= improvementCount)
                    image.Fill = this.imageBrushPlanetaryImprovementFilled;
                else
                    image.Fill = this.imageBrushPlanetaryImprovementEmpty;
            }
        }

        private void updateSelectedItemPopulationAssignmentsPanel(int farmers, int miners, int workers)
        {
            string farmerString = farmers == 1 ? " Farmer, " : " Farmers, ";
            string minerString = miners == 1 ? " Miner, " : " Miners, ";
            string workerString = workers == 1 ? " Workers" : " Workers";
            string tooltip = farmers + farmerString + miners + minerString + workers + workerString;
            ToolTipService.SetToolTip(this.SelectedItemPopulationAssignmentsPanel, tooltip);

            //clear out the appropriate ones
            for (int i = farmers + miners + workers + 1; i <= 16; i++)
            {
                Rectangle image = (Rectangle)LayoutRoot.FindName("PopulationAssignmentImage" + i);
                image.Fill = null;
            }

            for (int i = 1; i <= farmers; i++)
            {
                Rectangle image = (Rectangle)LayoutRoot.FindName("PopulationAssignmentImage" + i);
                image.Fill = this.imageBrushFarmer;
            }

            for (int i = farmers + 1; i <= farmers + miners; i++)
            {
                Rectangle image = (Rectangle)LayoutRoot.FindName("PopulationAssignmentImage" + i);
                image.Fill = this.imageBrushMiner;
            }

            for (int i = farmers + miners + 1; i <= farmers + miners + workers; i++)
            {
                Rectangle image = (Rectangle)LayoutRoot.FindName("PopulationAssignmentImage" + i);
                image.Fill = this.imageBrushBuilder;
            }
        }

        private void updateSelectedItemPanelForPlanet()
        {
            this.PlanetViewButton.IsEnabled = false;
            this.SendShipsButton.IsEnabled = false;

            StringBuilder sb = new StringBuilder();

            if (this.model.GameGrid.SelectedHex != null)
            {
                Planet p = this.model.GameGrid.SelectedHex.PlanetContainedInHex;

                sb.Append("--- Planet " + p.Name + " ---\r\n");

                this.SelectedItemBuiltImprovementsGrid.Visibility = Visibility.Collapsed;
                this.SelectedItemPlanetaryFleetGrid.Visibility = Visibility.Collapsed;
                this.SelectedItemPopulationPanel.Visibility = Visibility.Collapsed;
                this.SelectedItemPopulationAssignmentsPanel.Visibility = Visibility.Collapsed;
                this.SelectedItemImprovementSlotsPanel.Visibility = Visibility.Collapsed;

                //for now only one player, change this for multiplayer
                bool planetKnownByPlayer = this.mainPlayer.PlanetKnownByPlayer(p);

                if (!planetKnownByPlayer)//this planet is unexplored
                {
                    sb.Append("Unexplored");
                }
                else//the main player has explored it
                {
                    LastKnownFleet lastKnownFleet = null;
                    Player lastKnownOwner = null;
                    if (this.mainPlayer.LastKnownPlanetFleetStrength.ContainsKey(p.Id))
                    {
                        lastKnownFleet = this.mainPlayer.LastKnownPlanetFleetStrength[p.Id];
                        lastKnownOwner = lastKnownFleet.LastKnownOwner;
                    }
                        

                    sb.Append(GameTools.PlanetTypeToFriendlyName(p.Type) + "\r\n");

                    

                    if (p.Owner == this.mainPlayer || GameTools.GameModel.ShowUnexploredPlanetsAndEnemyPlayerStats)//for now only show details for owned planets
                    {
                        string owner = (p.Type == PlanetType.AsteroidBelt) ? "None" : "Natives";
                        if (p.Owner != null)
                            owner = p.Owner.Name;
                        sb.Append(owner + "\r\n");

                        this.SelectedItemBuiltImprovementsGrid.Visibility = Visibility.Visible;
                        this.SelectedItemPlanetaryFleetGrid.Visibility = Visibility.Visible;
                        this.SelectedItemPopulationPanel.Visibility = Visibility.Visible;
                        this.SelectedItemPopulationAssignmentsPanel.Visibility = Visibility.Visible;
                        this.SelectedItemImprovementSlotsPanel.Visibility = Visibility.Visible;

                        this.updateSelectedItemPopulationPanel(p.Population.Count, p.MaxPopulation);

                        int farmCount = p.BuiltImprovements[PlanetImprovementType.Farm].Count;
                        int mineCount = p.BuiltImprovements[PlanetImprovementType.Mine].Count;
                        int colonyCount = p.BuiltImprovements[PlanetImprovementType.Colony].Count;
                        int factoryCount = p.BuiltImprovements[PlanetImprovementType.Factory].Count;
                        int spacePlatformCount = p.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count;

                        int builtImprovementsCount = farmCount + mineCount + factoryCount + colonyCount;

                        this.updateSelectedItemImprovementSlotsPanel(builtImprovementsCount, p.MaxImprovements);

                        int farmers = 0;
                        int miners = 0;
                        int workers = 0;
                        p.CountPopulationWorkerTypes(out farmers, out miners, out workers);

                        this.updateSelectedItemPopulationAssignmentsPanel(farmers, miners, workers);

                        /*
                        sb.Append("\r\n");

                        sb.Append(p.ResourcesPerTurn.FoodAmountPerTurn + " Food per turn\r\n");
                        sb.Append(p.ResourcesPerTurn.OreAmountPerTurn + " Ore per turn\r\n");
                        sb.Append(p.ResourcesPerTurn.IridiumAmountPerTurn + " Iridium per turn\r\n");
                        sb.Append(p.ResourcesPerTurn.ProductionAmountPerTurn + " Production per turn\r\n");
                        */
                        this.TextBlockFarmCount.Text = farmCount + "";
                        this.TextBlockMineCount.Text = mineCount + "";
                        this.TextBlockFactoryCount.Text = factoryCount + "";
                        this.TextBlockColonyCount.Text = colonyCount + "";
                        this.TextBlockSpacePlatformCount.Text = spacePlatformCount + "";
                        
                        int scoutCount = p.PlanetaryFleet.StarShips[StarShipType.Scout].Count;
                        int destroyerCount = p.PlanetaryFleet.StarShips[StarShipType.Destroyer].Count;
                        int cruiserCount = p.PlanetaryFleet.StarShips[StarShipType.Cruiser].Count;
                        int battleshipCount = p.PlanetaryFleet.StarShips[StarShipType.Battleship].Count;

                        this.TextBlockDefenderCount.Text = p.PlanetaryFleet.StarShips[StarShipType.SystemDefense].Count + "";
                        this.TextBlockScoutCount.Text = scoutCount + "";
                        this.TextBlockDestroyerCount.Text = destroyerCount + "";
                        this.TextBlockCruiserCount.Text = cruiserCount + "";
                        this.TextBlockBattleshipCount.Text = battleshipCount + "";

                        if (p.Owner == this.mainPlayer)//make sure we can't modify planets even while debugging TODO: hack for now will have to change for multi-player
                        {
                            this.PlanetViewButton.IsEnabled = true;

                            if (scoutCount != 0 || destroyerCount != 0 || cruiserCount != 0 || battleshipCount != 0)
                                this.SendShipsButton.IsEnabled = true;
                        }
                    }//if owned by main player
                    else if (lastKnownFleet != null)
                    {
                        sb.Append(GameTools.PlanetOwnerToFriendlyName(p, lastKnownOwner) + "\r\n");

                        Fleet lastKnownPlanetFleet = lastKnownFleet.Fleet;
                        int turnSinceExplored = GameTools.GameModel.Turn.Number - lastKnownFleet.TurnLastExplored;
                        string turnString = turnSinceExplored == 0 ? "Explored this turn." : (turnSinceExplored == 1 ? "Explored last turn." : "As of " + turnSinceExplored + " turns ago.");

                        sb.Append("--- Known Fleet ---\r\n");
                        sb.Append(turnString + "\r\n");
                        if (lastKnownPlanetFleet.HasSpacePlatform)
                            sb.Append("1 Space Platform\r\n");
                        else
                            sb.Append("No Space Platform\r\n");
                        this.TextBlockDefenderCount.Text = lastKnownPlanetFleet.StarShips[StarShipType.SystemDefense].Count + "";
                        this.TextBlockScoutCount.Text = lastKnownPlanetFleet.StarShips[StarShipType.Scout].Count + "";
                        this.TextBlockDestroyerCount.Text = lastKnownPlanetFleet.StarShips[StarShipType.Destroyer].Count + "";
                        this.TextBlockCruiserCount.Text = lastKnownPlanetFleet.StarShips[StarShipType.Cruiser].Count + "";
                        this.TextBlockBattleshipCount.Text = lastKnownPlanetFleet.StarShips[StarShipType.Battleship].Count + "";

                        this.SelectedItemPlanetaryFleetGrid.Visibility = Visibility.Visible;
                    }
                }

            }
            this.SelectedItemStatus.Text = sb.ToString();
        }
        /*
        private void updateSelectedItemPanelForPlanetText()
        {
            StringBuilder sb = new StringBuilder();

            if (this.model.GameGrid.SelectedHex != null)
            {
                Planet p = this.model.GameGrid.SelectedHex.PlanetContainedInHex;

                sb.Append("--- Planet " + p.Name + " ---\r\n");

                //for now only one player, change this for multiplayer
                bool planetKnownByPlayer = this.mainPlayer.PlanetKnownByPlayer(p);

                if (!planetKnownByPlayer)//this planet is unexplored
                {
                    sb.Append("Unexplored");
                }
                else//the main player has explored it
                {
                    sb.Append(GameTools.PlanetTypeToFriendlyName(p.Type) + "\r\n");

                    string owner = (p.Type == PlanetType.AsteroidBelt) ? "None" : "Natives";
                    if (p.Owner != null)
                        owner = p.Owner.Name;
                    sb.Append(owner + "\r\n");

                    if (p.Owner == this.mainPlayer || GameTools.GameModel.ShowUnexploredPlanetsAndEnemyPlayerStats)//for now only show details for owned planets, could eventually track last known details on unowned planets
                    {

                        sb.Append("\r\n");
                        sb.Append(p.Population.Count + " / " + p.MaxPopulation + " Population\r\n");

                        int farmCount = p.BuiltImprovements[PlanetImprovementType.Farm].Count;
                        int mineCount = p.BuiltImprovements[PlanetImprovementType.Mine].Count;
                        int colonyCount = p.BuiltImprovements[PlanetImprovementType.Colony].Count;
                        int factoryCount = p.BuiltImprovements[PlanetImprovementType.Factory].Count;
                        int spacePlatformCount = p.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count;

                        int builtImprovementsCount = farmCount + mineCount + factoryCount + colonyCount;

                        sb.Append(builtImprovementsCount + " / " + p.MaxImprovements + " Slots\r\n");

                        //sb.Append(p.Resources.FoodAmount + " Food\r\n");

                        sb.Append("\r\n");

                        int farmers = 0;
                        int miners = 0;
                        int workers = 0;
                        p.CountPopulationWorkerTypes(out farmers, out miners, out workers);

                        sb.Append("Farmers/Miners/Workers\r\n");
                        sb.Append(farmers + " / " + miners + " / " + workers + "\r\n");

                        sb.Append("\r\n");

                        sb.Append(p.ResourcesPerTurn.FoodAmountPerTurn + " Food per turn\r\n");
                        sb.Append(p.ResourcesPerTurn.OreAmountPerTurn + " Ore per turn\r\n");
                        sb.Append(p.ResourcesPerTurn.IridiumAmountPerTurn + " Iridium per turn\r\n");
                        sb.Append(p.ResourcesPerTurn.ProductionAmountPerTurn + " Production per turn\r\n");

                        sb.Append("\r\n");

                        sb.Append("--- Improvements ---\r\n");
                        sb.Append(farmCount + " Farms\r\n");
                        sb.Append(mineCount + " Mines\r\n");
                        sb.Append(colonyCount + " Colonies\r\n");
                        sb.Append(factoryCount + " Factories\r\n");
                        sb.Append(spacePlatformCount + " Space Platforms\r\n");

                        sb.Append("\r\n");

                        //if(p.Owner.Type == PlayerType.Human)
                        sb.Append("--- Fleet ---\r\n");

                        int scoutCount = p.PlanetaryFleet.StarShips[StarShipType.Scout].Count;
                        int destroyerCount = p.PlanetaryFleet.StarShips[StarShipType.Destroyer].Count;
                        int cruiserCount = p.PlanetaryFleet.StarShips[StarShipType.Cruiser].Count;
                        int battleshipCount = p.PlanetaryFleet.StarShips[StarShipType.Battleship].Count;

                        sb.Append(p.PlanetaryFleet.StarShips[StarShipType.SystemDefense].Count + " Defenders\r\n");
                        sb.Append(scoutCount + " Scouts\r\n");
                        sb.Append(destroyerCount + " Destroyers\r\n");
                        sb.Append(cruiserCount + " Cruisers\r\n");
                        sb.Append(battleshipCount + " Battleships\r\n");

                        if (p.Owner == this.mainPlayer)//make sure we can't modify planets even while debugging TODO: hack for now will have to change for multi-player
                        {
                            this.PlanetViewButton.IsEnabled = true;

                            if (scoutCount != 0 || destroyerCount != 0 || cruiserCount != 0 || battleshipCount != 0)
                                this.SendShipsButton.IsEnabled = true;
                        }
                    }//if owned by main player
                    else if (this.mainPlayer.LastKnownPlanetFleetStrength.ContainsKey(p.Id))
                    {
                        Fleet lastKnownPlanetFleet = this.mainPlayer.LastKnownPlanetFleetStrength[p.Id].Fleet;
                        sb.Append("--- Last Known Fleet ---\r\n");
                        if (lastKnownPlanetFleet.HasSpacePlatform)
                            sb.Append("1 Space Platform\r\n");
                        else
                            sb.Append("No Space Platforms\r\n");
                        sb.Append(lastKnownPlanetFleet.StarShips[StarShipType.SystemDefense].Count + " Defenders\r\n");
                        sb.Append(lastKnownPlanetFleet.StarShips[StarShipType.Scout].Count + " Scouts\r\n");
                        sb.Append(lastKnownPlanetFleet.StarShips[StarShipType.Destroyer].Count + " Destroyers\r\n");
                        sb.Append(lastKnownPlanetFleet.StarShips[StarShipType.Cruiser].Count + " Cruisers\r\n");
                        sb.Append(lastKnownPlanetFleet.StarShips[StarShipType.Battleship].Count + " Battleships\r\n");
                    }
                }

            }
            this.SelectedItemStatus.Text = sb.ToString();
        }
        */

        private void updateFleetIndicators()
        {
            foreach (Fleet f in this.mainPlayer.FleetsInTransit)
            {

            }

            //also look for planet outbound fleets
            foreach (Planet p in this.mainPlayer.OwnedPlanets.Values)
            {
                foreach (Fleet f in p.OutgoingFleets)
                {

                }
            }
        }

        private void updateCanvasForPlayer()
        {
            foreach (DrawnPlanet dp in this.canvasDrawnPlanets.Values)
            {
                dp.UpdatePlanetDrawingForPlayer(this.mainPlayer);//TODO: update for multiplayer
            }
        }

        private void generateModel()
        {
            //SolidColorBrush greenBrush = new SolidColorBrush(Color.FromArgb(255, 50, 255, 50));

            //add hexes (for debugging)
            foreach (Hexagon h in model.GameGrid.Hexes)
            {
                PlayfieldCanvas.Children.Add(h.PolyBase);

                /*
                TextBlock tb = new TextBlock();
                tb.TextAlignment = TextAlignment.Center;
                tb.Text = "[" + h.PathCoOrdX + "," + h.PathCoOrdY + "]";
                tb.FontWeight = FontWeights.Bold;
                tb.FontSize = 9.0;

                tb.Foreground = new SolidColorBrush(Colors.Red);

                tb.SetValue(Canvas.LeftProperty, h.MidPoint.X);
                tb.SetValue(Canvas.TopProperty, h.MidPoint.Y);
                PlayfieldCanvas.Children.Add(tb);
                */
            }
            

            //draw quadrants (for now for debugging)
            if (this.drawQuadrants)
            {
                foreach (Rect r in model.Quadrants)
                {
                    Rectangle rect = new Rectangle();
                    rect.Width = r.Width;
                    rect.Height = r.Height;
                    rect.SetValue(Canvas.LeftProperty, r.X);
                    rect.SetValue(Canvas.TopProperty, r.Y);
                    rect.Stroke = new SolidColorBrush(Colors.Gray);

                    PlayfieldCanvas.Children.Add(rect);
                }

                //draw sub-quadrants (for now for debugging)
                foreach (List<Rect> subQuadrant in model.SubQuadrants)
                {
                    foreach (Rect r in subQuadrant)
                    {
                        Rectangle rect = new Rectangle();
                        rect.Width = r.Width;
                        rect.Height = r.Height;
                        rect.SetValue(Canvas.LeftProperty, r.X);
                        rect.SetValue(Canvas.TopProperty, r.Y);
                        rect.Stroke = new SolidColorBrush(Colors.Gray);

                        PlayfieldCanvas.Children.Add(rect);
                    }
                }
            }

            

            foreach (Planet p in model.Planets)
            {
                DrawnPlanet dp = new DrawnPlanet(p);
                this.canvasDrawnPlanets.Add(p.Id, dp);
                this.PlayfieldCanvas.Children.Add(dp.Ellipse);
                this.PlayfieldCanvas.Children.Add(dp.TextBlock);
                this.PlayfieldCanvas.Children.Add(dp.TextBlockPlanetStrength);
                this.PlayfieldCanvas.Children.Add(dp.FleetRectangle);
                this.PlayfieldCanvas.Children.Add(dp.SpacePlatformRectangle);

                /*
                Path path = new Path();
                path.Fill = greenBrush;
                path.Stroke = blackBrush;

                EllipseGeometry eg = new EllipseGeometry();
                eg.Center = new Point(p.LocationX, p.LocationY);
                eg.RadiusX = 30;
                eg.RadiusY = 30;
                */

                //ellipse.SetValue(DependencyProperty
                
                
                
                //gp.Children.Add(ellipse);
            }
            //PlayfieldCanvas.Arrange(new Rect(0, 0, PlayfieldCanvas.Width, PlayfieldCanvas.Height));
        }


    }

    class TurnSummaryMessageListBoxItem : ListBoxItem
    {
        public TurnEventMessage EventMessage = null;

        public TurnSummaryMessageListBoxItem(TurnEventMessage turnEventMessage)
        {
            this.EventMessage = turnEventMessage;
            this.Content = turnEventMessage.Message;
        }
    }
}
