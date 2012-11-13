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
using System.Windows.Media.Imaging;
using SystemMaster.Library;
using SystemMaster.ControlLibrary;

namespace SystemMaster
{
    public partial class PlanetViewControl : UserControl, IDialogWindowControl
    {
        private Planet planetMain = null;

        private int population = 0;
        private int farmers = 0;
        private int miners = 0;
        private int workers = 0;

        private int farmersOrig = 0;
        private int minersOrig = 0;
        private int workersOrig = 0;

        private SliderValueClicked lastClicked = SliderValueClicked.None;
        private SliderValueClicked lastChanged = SliderValueClicked.None;

        private bool updatingGUI = false;

        //build queue items
        private List<PlanetProductionItem> workingBuildQueue;
        private PlayerResources workingResources;
        private int workingProductionRemainderOriginal = 0;

        AvailableImprovementListBoxItem lbiFarm = new AvailableImprovementListBoxItem(PlanetImprovementType.Farm);
        AvailableImprovementListBoxItem lbiMine = new AvailableImprovementListBoxItem(PlanetImprovementType.Mine);
        AvailableImprovementListBoxItem lbiColony = new AvailableImprovementListBoxItem(PlanetImprovementType.Colony);
        AvailableImprovementListBoxItem lbiFactory = new AvailableImprovementListBoxItem(PlanetImprovementType.Factory);
        AvailableImprovementListBoxItem lbiSpacePlatform = new AvailableImprovementListBoxItem(PlanetImprovementType.SpacePlatform);

        AvailableStarShipListBoxItem lbiDefender = new AvailableStarShipListBoxItem(StarShipType.SystemDefense);
        AvailableStarShipListBoxItem lbiScout = new AvailableStarShipListBoxItem(StarShipType.Scout);
        AvailableStarShipListBoxItem lbiDestroyer = new AvailableStarShipListBoxItem(StarShipType.Destroyer);
        AvailableStarShipListBoxItem lbiCruiser = new AvailableStarShipListBoxItem(StarShipType.Cruiser);
        AvailableStarShipListBoxItem lbiBattleship = new AvailableStarShipListBoxItem(StarShipType.Battleship);

        private FontFamily displayFont = new FontFamily("Courier New");

        private SolidColorBrush insufficientResourcesBrush = new SolidColorBrush(Colors.Yellow);
        private SolidColorBrush regularBrush = new SolidColorBrush(Colors.White);

        private int farmCount = 0;
        private int mineCount = 0;
        private int factoryCount = 0;
        private int colonyCount = 0;

        public PlanetViewControl(Planet planet)
        {
            InitializeComponent();

            this.TextBlockPlanetType.FontFamily = displayFont;
            this.TextBlockPlanetType.FontSize = 14;

            this.TextBlockFoodPerTurn.FontFamily = displayFont;
            this.TextBlockFoodPerTurn.FontSize = 24;
            this.TextBlockOrePerTurn.FontFamily = displayFont;
            this.TextBlockOrePerTurn.FontSize = 24;
            this.TextBlockIridiumPerTurn.FontFamily = displayFont;
            this.TextBlockIridiumPerTurn.FontSize = 24;
            this.TextBlockProductionPerTurn.FontFamily = displayFont;
            this.TextBlockProductionPerTurn.FontSize = 24;

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

            this.planetMain = planet;

            ImageBrush imageBrushPlanet = new ImageBrush();
            switch (this.planetMain.Type)
            {
                case PlanetType.PlanetClass2:
                    imageBrushPlanet.ImageSource = new BitmapImage(new Uri(@"img/PlanetClass2.png", UriKind.Relative));
                    break;
                case PlanetType.PlanetClass1:
                    imageBrushPlanet.ImageSource = new BitmapImage(new Uri(@"img/PlanetClass1.png", UriKind.Relative));
                    break;
                case PlanetType.DeadPlanet:
                    imageBrushPlanet.ImageSource = new BitmapImage(new Uri(@"img/PlanetDead.png", UriKind.Relative));
                    break;
                case PlanetType.AsteroidBelt:
                    imageBrushPlanet.ImageSource = new BitmapImage(new Uri(@"img/PlanetAsteroid.png", UriKind.Relative));
                    break;
            }
            this.PlanetImage.Fill = imageBrushPlanet;
            this.TextBlockPlanetType.Text = GameTools.PlanetTypeToFriendlyName(this.planetMain.Type);

            this.farmCount = this.planetMain.BuiltImprovements[PlanetImprovementType.Farm].Count;
            this.mineCount = this.planetMain.BuiltImprovements[PlanetImprovementType.Mine].Count;
            this.factoryCount = this.planetMain.BuiltImprovements[PlanetImprovementType.Factory].Count;
            this.colonyCount = this.planetMain.BuiltImprovements[PlanetImprovementType.Colony].Count;

            this.TextBlockFarmCount.Text = this.farmCount + "";
            this.TextBlockMineCount.Text = this.mineCount + "";
            this.TextBlockFactoryCount.Text = this.factoryCount + "";
            this.TextBlockColonyCount.Text = this.colonyCount + "";
            this.TextBlockSpacePlatformCount.Text = this.planetMain.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count + "";

            this.ButtonDemolishFarm.Click += new RoutedEventHandler(ButtonDemolishFarm_Click);
            this.ButtonDemolishMine.Click += new RoutedEventHandler(ButtonDemolishMine_Click);
            this.ButtonDemolishFactory.Click += new RoutedEventHandler(ButtonDemolishFactory_Click);
            this.ButtonDemolishColony.Click += new RoutedEventHandler(ButtonDemolishColony_Click);

            this.refreshResourcesPerTurnTextBoxes();

            this.updatePlanetStatsToolTip();

            population = planet.Population.Count;
            this.SliderFarmers.Maximum = population;
            this.SliderFarmers.LargeChange = 1;
            this.SliderMiners.Maximum = population;
            this.SliderMiners.LargeChange = 1;
            this.SliderWorkers.Maximum = population;
            this.SliderWorkers.LargeChange = 1;
            
            planet.CountPopulationWorkerTypes(out farmers, out miners, out workers);

            //copy to our orig variables to remember in case we cancel/close
            this.farmersOrig = farmers;
            this.minersOrig = miners;
            this.workersOrig = workers;

            this.SliderFarmers.Value = farmers;
            this.TextBoxFarmers.Text = farmers.ToString();

            this.SliderMiners.Value = miners;
            this.TextBoxMiners.Text = miners.ToString();

            this.SliderWorkers.Value = workers;
            this.TextBoxWorkers.Text = workers.ToString();


            this.SliderFarmers.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderFarmers_ValueChanged);
            this.SliderMiners.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderMiners_ValueChanged);
            this.SliderWorkers.ValueChanged += new RoutedPropertyChangedEventHandler<double>(SliderWorkers_ValueChanged);


            //copy the planet's buildQueue into our working build queue
            this.workingBuildQueue = planet.BuildQueue.ToList();
            this.workingResources = planet.Owner.Resources.Clone();
            this.workingProductionRemainderOriginal = planet.RemainderProduction;

            this.refreshItemsAvailableListBox();
            this.refreshBuildQueueListBox();
            this.showOrHideDemolishImprovementButtons();

            ListBoxDoubleClick lbdc = new ListBoxDoubleClick(this.ItemsAvailableListBox);
            lbdc.DoubleClick += new MouseButtonEventHandler(lbdc_DoubleClick);

            this.ItemsAvailableListBox.SelectionChanged += new SelectionChangedEventHandler(ItemsAvailableListBox_SelectionChanged);

            this.BuildQueueListBox.SelectionChanged += new SelectionChangedEventHandler(BuildQueueListBox_SelectionChanged);

            this.ButtonBuildQueueAddSelectedItem.IsEnabled = false;
            this.ButtonBuildQueueAddSelectedItem.Click += new RoutedEventHandler(ButtonBuildQueueAddSelectedItem_Click);

            this.ButtonBuildQueueRemoveSelectedItem.IsEnabled = false;
            this.ButtonBuildQueueRemoveSelectedItem.Click += new RoutedEventHandler(ButtonBuildQueueRemoveSelectedItem_Click);

            this.ButtonBuildQueueMoveSelectedItemDown.IsEnabled = false;
            this.ButtonBuildQueueMoveSelectedItemDown.Click += new RoutedEventHandler(ButtonBuildQueueMoveSelectedItemDown_Click);

            this.ButtonBuildQueueMoveSelectedItemUP.IsEnabled = false;
            this.ButtonBuildQueueMoveSelectedItemUP.Click += new RoutedEventHandler(ButtonBuildQueueMoveSelectedItemUP_Click);

            this.refreshCurrentWorkingResourcesTextBoxes();

            string toolTip = "If this option is checked and the build queue is empty at the end of the turn,\r\nthe ship last built on this planet will be added to the queue.\r\nIn order to build the ship, sufficient resources must exist\r\nas well as a surplus of gold to cover the amount of food shipped last turn.";
            this.LastShipBuiltTextBlock.Text = "";
            if (this.planetMain.StarShipTypeLastBuilt.HasValue)
            {
                string starshipname = GameTools.StarShipTypeToFriendlyName(this.planetMain.StarShipTypeLastBuilt.Value);
                //toolTip = toolTip + "\r\nLast Built: " + starshipname;
                this.LastShipBuiltTextBlock.Text = "Last Built: " + starshipname;

                //this.BuildLastShipCheckBox.IsEnabled = true;
                //if(this.planetMain.BuildQueue.Count == 0)
                //    this.BuildLastShipCheckBox.Content = "Build " +  + "s";
            }

            ToolTipService.SetToolTip(this.BuildLastShipCheckBox, toolTip);
            this.BuildLastShipCheckBox.IsChecked = this.planetMain.BuildLastStarShip;
            
            
        }

        private int countDemolishImprovementsInQueueByType(PlanetImprovementType pit)
        {
            int count = 0;

            foreach (PlanetProductionItem ppi in this.workingBuildQueue)
            {
                if (ppi is PlanetImprovementToDestroy && ((PlanetImprovementToDestroy)ppi).TypeToDestroy == pit)
                {
                    count++;
                }
            }

            return count;
        }

        private void showOrHideDemolishImprovementButtons()
        {
            if (this.farmCount - countDemolishImprovementsInQueueByType(PlanetImprovementType.Farm) > 0)
            {
                this.ButtonDemolishFarm.Visibility = Visibility.Visible;
            }
            else
                this.ButtonDemolishFarm.Visibility = Visibility.Collapsed;

            if (this.mineCount - countDemolishImprovementsInQueueByType(PlanetImprovementType.Mine) > 0)
            {
                this.ButtonDemolishMine.Visibility = Visibility.Visible;
            }
            else
                this.ButtonDemolishMine.Visibility = Visibility.Collapsed;

            if (this.factoryCount - countDemolishImprovementsInQueueByType(PlanetImprovementType.Factory) > 0)
            {
                this.ButtonDemolishFactory.Visibility = Visibility.Visible;
            }
            else
                this.ButtonDemolishFactory.Visibility = Visibility.Collapsed;

            if (this.colonyCount - countDemolishImprovementsInQueueByType(PlanetImprovementType.Colony) > 0)
            {
                this.ButtonDemolishColony.Visibility = Visibility.Visible;
            }
            else
                this.ButtonDemolishColony.Visibility = Visibility.Collapsed;
        }

        void ButtonDemolishColony_Click(object sender, RoutedEventArgs e)
        {
            addImprovementToDestroy(PlanetImprovementType.Colony);
        }

        void ButtonDemolishFactory_Click(object sender, RoutedEventArgs e)
        {
            addImprovementToDestroy(PlanetImprovementType.Factory);
        }

        void ButtonDemolishMine_Click(object sender, RoutedEventArgs e)
        {
            addImprovementToDestroy(PlanetImprovementType.Mine);
        }

        void ButtonDemolishFarm_Click(object sender, RoutedEventArgs e)
        {
            addImprovementToDestroy(PlanetImprovementType.Farm);
        }

        private void addImprovementToDestroy(PlanetImprovementType pit)
        {
            PlanetImprovementToDestroy pi = new PlanetImprovementToDestroy(pit);
            this.workingBuildQueue.Add(pi);
            this.showOrHideDemolishImprovementButtons();
            this.refreshBuildQueueListBox();
        }

        void updatePlanetStatsToolTip()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.Append("Base Worker Resource Generation Per Turn:\r\n");
            sb.Append("Food: " + this.planetMain.ResourcesPerTurn.BaseFoodAmountPerWorkerPerTurn + "\r\n");
            sb.Append("Ore: " + this.planetMain.ResourcesPerTurn.BaseOreAmountPerWorkerPerTurn + "\r\n");
            sb.Append("Iridium: " + this.planetMain.ResourcesPerTurn.BaseIridiumAmountPerWorkerPerTurn + "\r\n");
            sb.Append("Production: " + this.planetMain.ResourcesPerTurn.BaseProductionPerWorkerPerTurn + "\r\n\r\n");

            sb.Append("Worker Resource Generation with Improvements:\r\n");
            sb.Append("Food: " + this.planetMain.ResourcesPerTurn.GetExactFoodAmountPerWorkerPerTurn() + "\r\n");
            sb.Append("Ore: " + this.planetMain.ResourcesPerTurn.GetExactOreAmountPerWorkerPerTurn() + "\r\n");
            sb.Append("Iridium: " + this.planetMain.ResourcesPerTurn.GetExactIridiumAmountPerWorkerPerTurn() + "\r\n");
            sb.Append("Production: " + this.planetMain.ResourcesPerTurn.GetExactProductionAmountPerWorkerPerTurn() + "\r\n\r\n");

            sb.Append("Food amount on planet: " + this.planetMain.Resources.FoodAmount + "\r\n");

            ToolTipService.SetToolTip(this.PlanetImage, "Planet Stats:\r\n" + sb.ToString());
        }

        void BuildQueueListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ButtonBuildQueueRemoveSelectedItem.IsEnabled = false;
            this.ButtonBuildQueueMoveSelectedItemDown.IsEnabled = false;
            this.ButtonBuildQueueMoveSelectedItemUP.IsEnabled = false;
            if (this.BuildQueueListBox.SelectedItem != null)
            {
                this.ButtonBuildQueueRemoveSelectedItem.IsEnabled = true;
                if (this.BuildQueueListBox.SelectedIndex != this.BuildQueueListBox.Items.Count - 1)
                    this.ButtonBuildQueueMoveSelectedItemDown.IsEnabled = true;
                if (this.BuildQueueListBox.SelectedIndex != 0)
                    this.ButtonBuildQueueMoveSelectedItemUP.IsEnabled = true;
            }
        }

        void ItemsAvailableListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ButtonBuildQueueAddSelectedItem.IsEnabled = false;
            if (this.ItemsAvailableListBox.SelectedItem != null && ((ListBoxItem)this.ItemsAvailableListBox.SelectedItem).IsEnabled)
            {
                this.ButtonBuildQueueAddSelectedItem.IsEnabled = true;
            }
        }

        void ButtonBuildQueueAddSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            this.addSelectedItemToQueue();
        }

        void ButtonBuildQueueRemoveSelectedItem_Click(object sender, RoutedEventArgs e)
        {
            this.removeSelectedItemFromQueue();
        }

        void ButtonBuildQueueMoveSelectedItemUP_Click(object sender, RoutedEventArgs e)
        {
            this.moveSelectedItemInQueue(true);
        }

        void ButtonBuildQueueMoveSelectedItemDown_Click(object sender, RoutedEventArgs e)
        {
            this.moveSelectedItemInQueue(false);
        }

        private void updateSliderValues(SliderValueClicked clicked)
        {
            //TODO: is dependent sliders the way to go? for now hopefully it's easy
            //determine if we're adding or removing...
            //if clicked farmers switch off between giving to/taking from miners and workers
            //if clicked miners switch off between giving to/taking from farmers and workers
            //if clicked workers switch off between giving to/taking from farmers and miners
            int diff = 0;
            this.updatingGUI = true;

            if(this.lastClicked != clicked)
                this.lastChanged = SliderValueClicked.None;

            int roundedFarmerSliderValue = (int)Math.Round(this.SliderFarmers.Value);
            int roundedMinerSliderValue = (int)Math.Round(this.SliderMiners.Value);
            int roundedWorkerSliderValue = (int)Math.Round(this.SliderWorkers.Value);

            //figure out who we can give to or take from
            //if either others are candidates, choose the one we didn't last change (alternate)

            //first figure differences
            switch (clicked)
            {
                case SliderValueClicked.Farmers:
                    diff = roundedFarmerSliderValue - this.farmers;
                    break;
                case SliderValueClicked.Miners:
                    diff = roundedMinerSliderValue - this.miners;
                    break;
                case SliderValueClicked.Workers:
                    diff = roundedWorkerSliderValue - this.workers;
                    break;
            }

            bool canChangeFarmers = false;
            bool canChangeMiners = false;
            bool canChangeWorkers = false;
            //next figure can change candidates
            if (diff > 0) //we're looking for a slider to take from
            {
                canChangeFarmers = (this.farmers != 0);
                canChangeMiners = (this.miners != 0);
                canChangeWorkers = (this.workers != 0);
            }
            else if (diff < 0) //we're looking for a slider to give to
            {
                canChangeFarmers = (this.farmers != this.population);
                canChangeMiners = (this.miners != this.population);
                canChangeWorkers = (this.workers != this.population);
            }
            else
            {
                //we're not changing anything
                this.updatingGUI = false;
                //System.Diagnostics.Debug.WriteLine("NOT Changing the sliders - worker value: " + this.SliderWorkers.Value);
                return;
            }

            //System.Diagnostics.Debug.WriteLine("Changing the " + clicked + " slider - worker value: " + this.SliderWorkers.Value);

            while (diff != 0)
            {
                int diffToChange = 1;
                if (diff < 0)
                    diffToChange = -1;

                SliderValueClicked sliderToChange = SliderValueClicked.None;
                //next pick a slider to change
                switch (clicked)
                {
                    case SliderValueClicked.Farmers:
                        if (canChangeMiners && !canChangeWorkers)
                        {
                            sliderToChange = SliderValueClicked.Miners;
                        }
                        else if (!canChangeMiners && canChangeWorkers)
                        {
                            sliderToChange = SliderValueClicked.Workers;
                        }
                        else//if both values are the same, check last change to alternate candidates
                        //otherwize first check diff to see if we want the larger or the smaller
                        {
                            if (roundedMinerSliderValue == roundedWorkerSliderValue)
                            {
                                if (this.lastChanged != SliderValueClicked.Miners)
                                    sliderToChange = SliderValueClicked.Miners;
                                else
                                    sliderToChange = SliderValueClicked.Workers;
                            }
                            else if (diff > 0)//we're removing so choose the slider with a larger value
                            {
                                if (roundedMinerSliderValue > roundedWorkerSliderValue)
                                    sliderToChange = SliderValueClicked.Miners;
                                else
                                    sliderToChange = SliderValueClicked.Workers;
                            }
                            else//choose the slider with a smaller value
                            {
                                if (roundedMinerSliderValue < roundedWorkerSliderValue)
                                    sliderToChange = SliderValueClicked.Miners;
                                else
                                    sliderToChange = SliderValueClicked.Workers;
                            }
                        }

                        break;
                    case SliderValueClicked.Miners:
                        if (canChangeFarmers && !canChangeWorkers)
                        {
                            sliderToChange = SliderValueClicked.Farmers;
                        }
                        else if (!canChangeFarmers && canChangeWorkers)
                        {
                            sliderToChange = SliderValueClicked.Workers;
                        }
                        else//if both values are the same, check last change to alternate candidates
                        //otherwize first check diff to see if we want the larger or the smaller
                        {
                            if (roundedFarmerSliderValue == roundedWorkerSliderValue)
                            {
                                if (this.lastChanged != SliderValueClicked.Farmers)
                                    sliderToChange = SliderValueClicked.Farmers;
                                else
                                    sliderToChange = SliderValueClicked.Workers;
                            }
                            else if (diff > 0)//we're removing so choose the slider with a larger value
                            {
                                if (roundedFarmerSliderValue > roundedWorkerSliderValue)
                                    sliderToChange = SliderValueClicked.Farmers;
                                else
                                    sliderToChange = SliderValueClicked.Workers;
                            }
                            else//choose the slider with a smaller value
                            {
                                if (roundedFarmerSliderValue < roundedWorkerSliderValue)
                                    sliderToChange = SliderValueClicked.Farmers;
                                else
                                    sliderToChange = SliderValueClicked.Workers;
                            }
                        }
                        break;
                    case SliderValueClicked.Workers:
                        if (canChangeFarmers && !canChangeMiners)
                        {
                            sliderToChange = SliderValueClicked.Farmers;
                        }
                        else if (!canChangeFarmers && canChangeMiners)
                        {
                            sliderToChange = SliderValueClicked.Miners;
                        }
                        else//if both values are the same, check last change to alternate candidates
                        //otherwize first check diff to see if we want the larger or the smaller
                        {
                            if (roundedFarmerSliderValue == roundedMinerSliderValue)
                            {
                                if (this.lastChanged != SliderValueClicked.Farmers)
                                    sliderToChange = SliderValueClicked.Farmers;
                                else
                                    sliderToChange = SliderValueClicked.Miners;
                            }
                            else if (diff > 0)//we're removing so choose the slider with a larger value
                            {
                                if (roundedFarmerSliderValue > roundedMinerSliderValue)
                                    sliderToChange = SliderValueClicked.Farmers;
                                else
                                    sliderToChange = SliderValueClicked.Miners;
                            }
                            else//choose the slider with a smaller value
                            {
                                if (roundedFarmerSliderValue < roundedMinerSliderValue)
                                    sliderToChange = SliderValueClicked.Farmers;
                                else
                                    sliderToChange = SliderValueClicked.Miners;
                            }
                        }
                        break;
                }

                //finally, change the picked slider
                switch (sliderToChange)
                {
                    case SliderValueClicked.Farmers:
                        roundedFarmerSliderValue -= diffToChange;
                        this.SliderFarmers.Value = roundedFarmerSliderValue;
                        this.lastChanged = SliderValueClicked.Farmers;
                        break;
                    case SliderValueClicked.Miners:
                        roundedMinerSliderValue -= diffToChange;
                        this.SliderMiners.Value = roundedMinerSliderValue;
                        this.lastChanged = SliderValueClicked.Miners;
                        break;
                    case SliderValueClicked.Workers:
                        roundedWorkerSliderValue -= diffToChange;
                        this.SliderWorkers.Value = roundedWorkerSliderValue;
                        this.lastChanged = SliderValueClicked.Workers;
                        break;
                    default:
                        throw new InvalidOperationException("Unable to determine slider to change in PlanetViewControl!");
                        break;
                }

                if (diff > 0)
                    diff--;
                else if (diff < 0)
                    diff++;
            }


            this.updatingGUI = false;

            this.lastClicked = clicked;

            this.farmers = (int)Math.Round(this.SliderFarmers.Value);
            this.TextBoxFarmers.Text = this.farmers.ToString();

            this.miners = (int)Math.Round(this.SliderMiners.Value);
            this.TextBoxMiners.Text = this.miners.ToString();

            this.workers = (int)Math.Round(this.SliderWorkers.Value);
            this.TextBoxWorkers.Text = this.workers.ToString();
            

            this.recalculateBuildQueueListItemsTurnsToCompleteEstimates();
            this.refreshResourcesPerTurnTextBoxes();
        }

        void SliderFarmers_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(!updatingGUI)
                this.updateSliderValues(SliderValueClicked.Farmers);
        }

        void SliderMiners_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!updatingGUI)
                this.updateSliderValues(SliderValueClicked.Miners);
        }

        void SliderWorkers_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!updatingGUI)
                this.updateSliderValues(SliderValueClicked.Workers);
        }

        void lbdc_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            //they double clicked on the list
            this.addSelectedItemToQueue();
        }

        private bool workingQueueContainsSpacePlatform()
        {
            foreach (PlanetProductionItem ppi in this.workingBuildQueue)
            {
                if (ppi is PlanetImprovement && ((PlanetImprovement)ppi).Type == PlanetImprovementType.SpacePlatform)
                    return true;
            }
            return false;
        }

        private void disableOrEnableImprovementsBasedOnSlotsAvailable()
        {
            int improvementCount = this.planetMain.BuiltImprovementCount;
            //count items that take up slots in working queue
            foreach (PlanetProductionItem ppi in this.workingBuildQueue)
            {
                if (ppi is PlanetImprovement && ((PlanetImprovement)ppi).Type != PlanetImprovementType.SpacePlatform)
                    improvementCount++;
            }

            int slotsAvailable = this.planetMain.MaxImprovements - improvementCount;

            if (slotsAvailable <= 0)//less than zero is a problem, but we'll just make sure they can't build more here
            {
                lbiFarm.CanBuild = false;
                lbiMine.CanBuild = false;
                lbiColony.CanBuild = false;
                lbiFactory.CanBuild = false;
            }
            else
            {
                lbiFarm.CanBuild = true;
                lbiMine.CanBuild = true;
                lbiColony.CanBuild = true;
                lbiFactory.CanBuild = true;
            }

            if (this.planetMain.BuiltImprovements[PlanetImprovementType.Factory].Count == 0)
                lbiSpacePlatform.CanBuild = false;
            else
                lbiSpacePlatform.CanBuild = true;

            //we can only have one space platform
            if (this.planetMain.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count > 0 ||
                this.workingQueueContainsSpacePlatform())
            {
                lbiSpacePlatform.CanBuild = false;
            }
        }

        private void disableImprovementsBasedOnResourcesAvailable()
        {
            foreach(ListBoxItem lbi in this.ItemsAvailableListBox.Items)
            {
                if (lbi is AvailableImprovementListBoxItem)
                {
                    AvailableImprovementListBoxItem lbiImprovement = (AvailableImprovementListBoxItem)lbi;
                    if (lbiImprovement.CanBuild)
                    {
                        if (this.workingResources.GoldAmount < lbiImprovement.AvailablePlanetImprovement.GoldCost ||
                            this.workingResources.IridiumAmount < lbiImprovement.AvailablePlanetImprovement.IridiumCost ||
                            this.workingResources.OreAmount < lbiImprovement.AvailablePlanetImprovement.OreCost)
                        {
                            lbiImprovement.CanBuildBasedOnResources = false;
                            lbiImprovement.Foreground = insufficientResourcesBrush;
                        }
                        else
                        {
                            lbiImprovement.CanBuildBasedOnResources = true;
                            lbiImprovement.Foreground = regularBrush;
                        }
                    }

                }
                else if (lbi is AvailableStarShipListBoxItem)
                {
                    AvailableStarShipListBoxItem lbiStarShip = (AvailableStarShipListBoxItem)lbi;
                    if (lbiStarShip.CanBuild)
                    {
                        if (this.workingResources.GoldAmount < lbiStarShip.AvailableStarShip.GoldCost ||
                                this.workingResources.IridiumAmount < lbiStarShip.AvailableStarShip.IridiumCost ||
                                this.workingResources.OreAmount < lbiStarShip.AvailableStarShip.OreCost)
                        {
                            lbiStarShip.CanBuildBasedOnResources = false;
                            lbiStarShip.Foreground = insufficientResourcesBrush;
                        }
                        else
                        {
                            lbiStarShip.CanBuildBasedOnResources = true;
                            lbiStarShip.Foreground = regularBrush;
                        }
                    }
                }
            }

            //check to ensure the selected item is not enabled
            if (this.ItemsAvailableListBox.SelectedItem != null && !((ListBoxItem)this.ItemsAvailableListBox.SelectedItem).IsEnabled)
            {
                this.ItemsAvailableListBox.SelectedItem = null;
            }
        }

        private void refreshItemsAvailableListBox()
        {
            this.ItemsAvailableListBox.Items.Clear();

            this.disableOrEnableImprovementsBasedOnSlotsAvailable();

            this.ItemsAvailableListBox.Items.Add(lbiFarm);
            this.ItemsAvailableListBox.Items.Add(lbiMine);
            this.ItemsAvailableListBox.Items.Add(lbiColony);
            this.ItemsAvailableListBox.Items.Add(lbiFactory);
            this.ItemsAvailableListBox.Items.Add(lbiSpacePlatform);

            //this.ItemsAvailableListBox.Items.Add(new Separator());

            this.ItemsAvailableListBox.Items.Add(lbiDefender);
            this.ItemsAvailableListBox.Items.Add(lbiScout);
            if (this.planetMain.BuiltImprovements[PlanetImprovementType.Factory].Count == 0)
                lbiDestroyer.CanBuild = false;
            this.ItemsAvailableListBox.Items.Add(lbiDestroyer);
            if (this.planetMain.BuiltImprovements[PlanetImprovementType.SpacePlatform].Count == 0)
            {
                this.lbiCruiser.CanBuild = false;
                this.lbiBattleship.CanBuild = false;
            }
            this.ItemsAvailableListBox.Items.Add(lbiCruiser);
            this.ItemsAvailableListBox.Items.Add(lbiBattleship);

            this.disableImprovementsBasedOnResourcesAvailable();
        }

        private void refreshBuildQueueListBox()
        {
            this.BuildQueueListBox.Items.Clear();

            int workingProdRemainder = this.workingProductionRemainderOriginal;
            foreach (PlanetProductionItem ppi in this.workingBuildQueue)
            {
                ppi.EstimateTurnsToComplete(this.planetMain.ResourcesPerTurn.ProductionAmountPerTurn + workingProdRemainder);
                workingProdRemainder = 0;
                BuildQueueListBoxItem queueItem = new BuildQueueListBoxItem(ppi);
                this.BuildQueueListBox.Items.Add(queueItem);
            }
        }

        public void recalculateBuildQueueListItemsTurnsToCompleteEstimates()
        {
            //TODO: will this be too slow?
            this.planetMain.UpdatePopulationWorkerTypes(this.farmers, this.miners, this.workers);
            this.planetMain.ResourcesPerTurn.UpdateResourcesPerTurnBasedOnPlanetStats();

            int workingProdRemainder = this.workingProductionRemainderOriginal;
            foreach (BuildQueueListBoxItem queueItem in this.BuildQueueListBox.Items)
            {
                queueItem.ProductionItem.EstimateTurnsToComplete(this.planetMain.ResourcesPerTurn.ProductionAmountPerTurn + workingProdRemainder);
                workingProdRemainder = 0;
                queueItem.Content = queueItem.ToString();
            }
        }

        void refreshResourcesPerTurnTextBoxes()
        {
            this.TextBlockFoodPerTurn.Text = this.planetMain.ResourcesPerTurn.FoodAmountPerTurn + "";
            this.TextBlockOrePerTurn.Text = this.planetMain.ResourcesPerTurn.OreAmountPerTurn + "";
            this.TextBlockIridiumPerTurn.Text = this.planetMain.ResourcesPerTurn.IridiumAmountPerTurn + "";
            this.TextBlockProductionPerTurn.Text = this.planetMain.ResourcesPerTurn.ProductionAmountPerTurn + "";
        }

        void refreshCurrentWorkingResourcesTextBoxes()
        {
            this.TextBlockCurrentGoldAmount.Text = this.workingResources.GoldAmount + "";
            this.TextBlockCurrentOreAmount.Text = this.workingResources.OreAmount + "";
            this.TextBlockCurrentIridiumAmount.Text = this.workingResources.IridiumAmount + "";
        }

        private void moveSelectedItemInQueue(bool moveUp)
        {
            int index = this.BuildQueueListBox.SelectedIndex;

            if (index == 0 && moveUp)
                return;
            else if (index == this.BuildQueueListBox.Items.Count - 1 && !moveUp)
                return;

            BuildQueueListBoxItem bqlbi = (BuildQueueListBoxItem)this.BuildQueueListBox.Items[index];
            this.BuildQueueListBox.Items.RemoveAt(index);
            this.workingBuildQueue.RemoveAt(index);
            if(moveUp)
            {
                this.BuildQueueListBox.Items.Insert(index-1, bqlbi);
                this.workingBuildQueue.Insert(index-1, bqlbi.ProductionItem);
                this.BuildQueueListBox.SelectedIndex = index - 1;
            }
            else
            {
                this.BuildQueueListBox.Items.Insert(index+1, bqlbi);
                this.workingBuildQueue.Insert(index+1, bqlbi.ProductionItem);
                this.BuildQueueListBox.SelectedIndex = index + 1;
            }

        }

        private void removeSelectedItemFromQueue()
        {
            object o = this.BuildQueueListBox.SelectedItem;
            if (o != null && o is BuildQueueListBoxItem)
            {
                BuildQueueListBoxItem bqlbi = (BuildQueueListBoxItem)o;

                double goldRefund = 0.0;
                double oreRefund = 0.0;
                double iridiumRefund = 0.0;

                bqlbi.ProductionItem.GetRefundAmount(out goldRefund, out oreRefund, out iridiumRefund);

                this.workingResources.GoldRemainder += goldRefund;
                this.workingResources.OreRemainder += oreRefund;
                this.workingResources.IridiumRemainder += iridiumRefund;
                this.workingResources.AccumulateResourceRemainders();

                this.workingBuildQueue.Remove(bqlbi.ProductionItem);
                this.BuildQueueListBox.Items.Remove(bqlbi);

                this.disableOrEnableImprovementsBasedOnSlotsAvailable();
                this.disableImprovementsBasedOnResourcesAvailable();
                this.refreshCurrentWorkingResourcesTextBoxes();
                this.showOrHideDemolishImprovementButtons();
            }
        }

        private void addSelectedItemToQueue()
        {
            object o = this.ItemsAvailableListBox.SelectedItem;
            if (o != null)
            {
                if (o is AvailableImprovementListBoxItem)
                {
                    AvailableImprovementListBoxItem lbiImprovement = (AvailableImprovementListBoxItem)o;
                    if (lbiImprovement.CanBuild)
                    {
                        //check to see if we have enough resouces 
                        if (this.workingResources.GoldAmount >= lbiImprovement.AvailablePlanetImprovement.GoldCost &&
                            this.workingResources.IridiumAmount >= lbiImprovement.AvailablePlanetImprovement.IridiumCost &&
                            this.workingResources.OreAmount >= lbiImprovement.AvailablePlanetImprovement.OreCost)
                        {
                            this.workingResources.GoldAmount -= lbiImprovement.AvailablePlanetImprovement.GoldCost;
                            this.workingResources.IridiumAmount -= lbiImprovement.AvailablePlanetImprovement.IridiumCost;
                            this.workingResources.OreAmount -= lbiImprovement.AvailablePlanetImprovement.OreCost;
                            PlanetImprovement pi = new PlanetImprovement(lbiImprovement.AvailablePlanetImprovement.Type);
                            this.workingBuildQueue.Add(pi);

                        }
                        else
                        {
                            new AlertWindow("Insufficient resources", "Insufficient resources: (Gold/Ore/Iridium)\r\nRequires  (" + lbiImprovement.AvailablePlanetImprovement.GoldCost + " / " + lbiImprovement.AvailablePlanetImprovement.OreCost + " / " + lbiImprovement.AvailablePlanetImprovement.IridiumCost + ")\r\n" +
                                            "You have (" + this.workingResources.GoldAmount + " / " + this.workingResources.OreAmount + " / " + this.workingResources.IridiumAmount + ")");
                        }
                    }
                    //else warn them?
                }
                else if (o is AvailableStarShipListBoxItem)
                {
                    AvailableStarShipListBoxItem lbiStarship = (AvailableStarShipListBoxItem)o;
                    if (lbiStarship.CanBuild)
                    {
                        //check to see if we have enough resouces 
                        if (this.workingResources.GoldAmount >= lbiStarship.AvailableStarShip.GoldCost &&
                            this.workingResources.IridiumAmount >= lbiStarship.AvailableStarShip.IridiumCost &&
                            this.workingResources.OreAmount >= lbiStarship.AvailableStarShip.OreCost)
                        {
                            this.workingResources.GoldAmount -= lbiStarship.AvailableStarShip.GoldCost;
                            this.workingResources.IridiumAmount -= lbiStarship.AvailableStarShip.IridiumCost;
                            this.workingResources.OreAmount -= lbiStarship.AvailableStarShip.OreCost;
                            StarShipInProduction ssip = new StarShipInProduction(lbiStarship.AvailableStarShip.Type);
                            this.workingBuildQueue.Add(ssip);
                        }
                        else
                        {
                            new AlertWindow( "Insufficient resources", "Insufficient resources: (Gold/Ore/Iridium)\r\nRequires  (" + lbiStarship.AvailableStarShip.GoldCost + " / " + lbiStarship.AvailableStarShip.OreCost + " / " + lbiStarship.AvailableStarShip.IridiumCost + ")\r\n" +
                                            "You have (" + this.workingResources.GoldAmount + " / " + this.workingResources.OreAmount + " / " + this.workingResources.IridiumAmount + ")");
                        }
                    }
                    //else warn them?
                }
                this.disableOrEnableImprovementsBasedOnSlotsAvailable();
                this.disableImprovementsBasedOnResourcesAvailable();
                this.refreshBuildQueueListBox();
                this.refreshCurrentWorkingResourcesTextBoxes();
            }
        }
        
        #region IDialogWindowControl Members

        public void OKClose()
        {
            this.planetMain.UpdatePopulationWorkerTypes(this.farmers, this.miners, this.workers);
            this.planetMain.ResourcesPerTurn.UpdateResourcesPerTurnBasedOnPlanetStats();

            //copy our working items to our original planet pointer
            this.planetMain.BuildQueue.Clear();
            foreach (PlanetProductionItem ppi in this.workingBuildQueue)
            {
                this.planetMain.BuildQueue.Enqueue(ppi);
            }

            this.planetMain.Owner.Resources = this.workingResources.Clone();

            if(this.BuildLastShipCheckBox.IsChecked.HasValue)
                this.planetMain.BuildLastStarShip = this.BuildLastShipCheckBox.IsChecked.Value;
        }

        public void CancelClose()
        {
            //copy back our original workers to our planet object
            this.planetMain.UpdatePopulationWorkerTypes(this.farmersOrig, this.minersOrig, this.workersOrig);
            this.planetMain.ResourcesPerTurn.UpdateResourcesPerTurnBasedOnPlanetStats();
        }

        #endregion

        private enum SliderValueClicked
        {
            None,
            Farmers,
            Miners,
            Workers
        }
    }

    class BuildQueueListBoxItem : ListBoxItem
    {
        public PlanetProductionItem ProductionItem;

        public BuildQueueListBoxItem(PlanetProductionItem productionItem)
        {
            this.ProductionItem = productionItem;
            this.Content = this.ToString();
        }

        public override string ToString()
        {
            string name = this.ProductionItem.ToString();

            string turnsToCompleteString = "";
            //only show turns to complete if we've started building
            //if (this.ProductionItem.ProductionCostComplete > 0)
                turnsToCompleteString = " (" + (this.ProductionItem.TurnsToComplete + 1) + ")";
            name += " " + this.ProductionItem.ProductionCostComplete + "/" + this.ProductionItem.BaseProductionCost + turnsToCompleteString;
            return name;
        }
    }

    class AvailableImprovementListBoxItem : ListBoxItem
    {
        public PlanetImprovement AvailablePlanetImprovement;

        private bool canBuild = true;
        public bool CanBuild
        {
            get { return this.canBuild; }
            set
            {
                this.canBuild = value;
                this.IsEnabled = value;
            }
        }

        private bool canBuildBasedOnResources = true;
        public bool CanBuildBasedOnResources
        {
            get { return this.canBuildBasedOnResources; }
            set
            {
                this.canBuildBasedOnResources = value;
                this.IsEnabled = value;
            }
        }

        public AvailableImprovementListBoxItem(PlanetImprovementType type)
        {
            this.AvailablePlanetImprovement = new PlanetImprovement(type);
            this.Content = this.ToString();

            ToolTipService.SetToolTip(this, GameTools.PlanetImprovementTypeToHelpText(type));
        }

        public override string ToString()
        {
            string text = GameTools.PlanetImprovementTypeToFriendlyName(this.AvailablePlanetImprovement.Type);
            //show build cost
            text += " (" + this.AvailablePlanetImprovement.GoldCost + " / " + this.AvailablePlanetImprovement.OreCost + " / " + this.AvailablePlanetImprovement.IridiumCost + ")";
            return text;
        }
    }

    class AvailableStarShipListBoxItem : ListBoxItem
    {
        public StarShipInProduction AvailableStarShip;

        private bool canBuild = true;
        public bool CanBuild
        {
            get { return this.canBuild; }
            set
            {
                this.canBuild = value;
                this.IsEnabled = value;
            }
        }

        private bool canBuildBasedOnResources = true;
        public bool CanBuildBasedOnResources
        {
            get { return this.canBuildBasedOnResources; }
            set
            {
                this.canBuildBasedOnResources = value;
                this.IsEnabled = value;
            }
        }

        public AvailableStarShipListBoxItem(StarShipType type)
        {
            this.AvailableStarShip = new StarShipInProduction(type);
            this.Content = this.ToString();

            ToolTipService.SetToolTip(this, GameTools.StarShipTypeToHelpText(type));
        }

        public override string ToString()
        {
            string text = GameTools.StarShipTypeToFriendlyName(this.AvailableStarShip.Type);
            //show build cost
            text += " (" + this.AvailableStarShip.GoldCost + " / " + this.AvailableStarShip.OreCost + " / " + this.AvailableStarShip.IridiumCost + ")";
            return text;
        }
    }
}
