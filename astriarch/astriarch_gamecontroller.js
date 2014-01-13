//provides the glue between the html controls, Astriarch.View and the Game Model

Astriarch.GameController = {
	planetaryConflictMessages: []
};

Astriarch.GameController.SetupNewGame = function() {
	
	$('#TurnDisplay').text("Turn 1, Year 3001");
	
	//Make model, needs to use selected game options
	
	var players = []; //List<Player>
    var playerName = $('#PlayerNameTextBox').val();
	if(playerName == null || $.trim(playerName) == "")
		playerName = "Player1";
	var mainPlayer = new Astriarch.Player(Astriarch.Player.PlayerType.Human, playerName);//TODO: Make this configurable (and multiplayer!)
	players.push(mainPlayer);
	//NOTE: difficulty Combobox values correspond to Astriarch.Player.PlayerType 'enum' values
	players.push(new Astriarch.Player(Number($('select#Computer1DifficultyComboBox').selectmenu("value")), "Computer 1"));
	var opponentCount = Number($('input:radio[name=OpponentCountRadioGroup]:checked').val());
	if(opponentCount > 1)
		players.push(new Astriarch.Player(Number($('select#Computer2DifficultyComboBox').selectmenu("value")), "Computer 2"));
	if (opponentCount > 2)
		players.push(new Astriarch.Player(Number($('select#Computer3DifficultyComboBox').selectmenu("value")), "Computer 3"));
	
	var systemsToGenerate = Number($('input:radio[name=StarfieldRadioGroup]:checked').val());
	//NOTE: systems to generate combobox values correspond to Astriarch.Model.PlanetsPerSystemOption 'enum' values
	var planetsPerSystem = Number($('select#PlanetsPerSystemComboBox').selectmenu("value"));
	
	Astriarch.GameModel = new Astriarch.Model(players, mainPlayer, systemsToGenerate, planetsPerSystem);
	
	Astriarch.GameController.SetupViewFromGameModel();
	
	AstriarchExtern.OnGameStart({'Players':opponentCount+1, 'PlanetsPerSystem':planetsPerSystem, 'Systems':systemsToGenerate});
};

Astriarch.GameController.SetupViewFromGameModel = function() {
	
	Astriarch.View.audioInterface.BeginGame();

	Astriarch.View.TurnSummaryItemsListBox.clear();
	
	//add drawn planets to canvas
	for (var i in Astriarch.GameModel.Planets)
    {
		var dp = new Astriarch.DrawnPlanet(Astriarch.GameModel.Planets[i]);
		Astriarch.View.DrawnPlanets[Astriarch.GameModel.Planets[i].Id] = dp;
		Astriarch.View.CanvasPlayfieldLayer.addChild(dp);
	}
	Astriarch.View.updateCanvasForPlayer();
	
	//select our home planet
	Astriarch.GameModel.GameGrid.SelectHex(Astriarch.GameModel.MainPlayer.HomePlanet.BoundingHex);
	Astriarch.View.updateSelectedItemPanelForPlanet();
	
	Astriarch.View.updatePlayerStatusPanel();
};

Astriarch.GameController.NextTurn = function() {

	Astriarch.SavedGameInterface.saveGame();//right now we'll just save when the Next Turn button is clicked
	
	Astriarch.GameModel.Turn.Next();
	
	var year = Astriarch.GameModel.Turn.Number + 3000;
	$('#TurnDisplay').text("Turn " + Astriarch.GameModel.Turn.Number + ", Year " + year);

	var endOfTurnMessages = Astriarch.GameTools.EndTurns(Astriarch.GameModel.Players);//List<TurnEventMessage>

	//gather our planetary conflict messages
	Astriarch.GameController.planetaryConflictMessages = [];
	//if we have the option enabled to show the planetary conflict dialog window at end of turn
	if (Astriarch.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups)
	{
		for (var i in endOfTurnMessages)
		{
			var tem = endOfTurnMessages[i];//TurnEventMessage
			if (tem.Type == Astriarch.TurnEventMessage.TurnEventMessageType.AttackingFleetLost ||
				tem.Type == Astriarch.TurnEventMessage.TurnEventMessageType.DefendedAgainstAttackingFleet ||
				tem.Type == Astriarch.TurnEventMessage.TurnEventMessageType.PlanetCaptured ||
				tem.Type == Astriarch.TurnEventMessage.TurnEventMessageType.PlanetLost)
			{
				if (tem.Data != null)//just to make sure
					Astriarch.GameController.planetaryConflictMessages.push(tem);
			}
		}
	}

	Astriarch.View.updatePlayerStatusPanel();

	Astriarch.View.updateSelectedItemPanelForPlanet();

	Astriarch.View.updateCanvasForPlayer();

	Astriarch.View.TurnSummaryItemsListBox.clear();
	if (endOfTurnMessages.length > 0)
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
		var listBoxItems = [];
		for (var i in endOfTurnMessages)
		{
			var tem = endOfTurnMessages[i];//TurnEventMessage
			var tsmlbi = new Astriarch.GameController.TurnSummaryMessageListBoxItem(tem);
			switch (tsmlbi.EventMessage.Type)
			{
				case Astriarch.TurnEventMessage.TurnEventMessageType.FoodShipped:
				case Astriarch.TurnEventMessage.TurnEventMessageType.PopulationGrowth:
					tsmlbi.Foreground = "blue";
					break;
				case Astriarch.TurnEventMessage.TurnEventMessageType.DefendedAgainstAttackingFleet:
					tsmlbi.Foreground = "purple";
					break;
				case Astriarch.TurnEventMessage.TurnEventMessageType.BuildQueueEmpty:
				case Astriarch.TurnEventMessage.TurnEventMessageType.InsufficientFood:
					tsmlbi.Foreground = "yellow";
					break;
				case Astriarch.TurnEventMessage.TurnEventMessageType.PlanetCaptured:
				case Astriarch.TurnEventMessage.TurnEventMessageType.AttackingFleetLost:
					tsmlbi.Foreground = "orange";
					break;
				case Astriarch.TurnEventMessage.TurnEventMessageType.PlanetLost:
				case Astriarch.TurnEventMessage.TurnEventMessageType.PopulationStarvation:
				case Astriarch.TurnEventMessage.TurnEventMessageType.PlanetLostDueToStarvation:
				case Astriarch.TurnEventMessage.TurnEventMessageType.FoodShortageRiots:
					tsmlbi.Foreground = "red";
					break;
				default:
					tsmlbi.Foreground = "green";
					break;
			}
			listBoxItems.push(tsmlbi);
		}
		Astriarch.View.TurnSummaryItemsListBox.addItems(listBoxItems);

		Astriarch.GameController.processNextEndOfTurnPlanetaryConflictMessage();
	}
	
};

Astriarch.GameController.OnPlayerDestroyed = function(/*Player)*/ player) {
	var a = new Astriarch.Alert(player.Name + " Destroyed", "Player: <span style=\"color:"+player.Color.toString()+"\">" + player.Name + "</span> destroyed.");
};

Astriarch.GameController.OnPlayerDestroyedGameOver = function(/*Player)*/ player) {
	Astriarch.View.audioInterface.EndGame();

	var mainPlayerWins = (player == Astriarch.GameModel.MainPlayer);
	$('#PlanetViewButton,#SendShipsButton,#NextTurnButton').hide();
	
	Astriarch.GameOverControl.show(player, mainPlayerWins);
};

Astriarch.GameController.processNextEndOfTurnPlanetaryConflictMessage = function() {
	var i = Astriarch.GameController.planetaryConflictMessages.length - 1;
	if(i >= 0)
	{
		var tem = Astriarch.GameController.planetaryConflictMessages[i];//TurnEventMessage
		Astriarch.GameController.planetaryConflictMessages.splice(i, 1);
		if (Astriarch.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups)
			Astriarch.GameController.popupPlanetaryConflictControl(tem);
	}
};

Astriarch.GameController.popupPlanetaryConflictControl = function(/*TurnEventMessage*/ tem) {

	Astriarch.PlanetaryConflictControl.show(tem);
};

Astriarch.GameController.GameOverControlClosed = function() {
	
	//show playfield after game over
	Astriarch.GameModel.ShowUnexploredPlanetsAndEnemyPlayerStats = true;
	
	Astriarch.View.updateCanvasForPlayer();
	Astriarch.View.updateSelectedItemPanelForPlanet();

	$('#MainMenuButtonGameOver').show();
};

/**
 * A TurnSummaryMessageListBoxItem is one of the items shown at the end of the turn
 * @constructor
 */
Astriarch.GameController.TurnSummaryMessageListBoxItem = JSListBox.Item.extend({
	/**
	 * initializes the TurnSummaryMessageListBoxItem
	 * @this {Astriarch.GameController.TurnSummaryMessageListBoxItem}
	 */
	init: function(/* TurnEventMessage */ tem) {
		this.value = tem.Message; //what is shown in the item
		this.Foreground = null;
		this.EventMessage = tem;
	},
	
	/**
	 * renders the TurnSummaryMessageListBoxItem
	 * @this {Astriarch.GameController.TurnSummaryMessageListBoxItem}
	 * @return {string}
	 */
	render: function() {
		return '<a href="#" style="color:' + this.Foreground + '">' + this.value + '</a>'; //this allows paiting to be overridden in classes which extend JSListBox.Item
	},
	
	/**
	 * fires the TurnSummaryMessageListBoxItem click event
	 * @this {Astriarch.GameController.TurnSummaryMessageListBoxItem}
	 */
	onClick: function() {
		if(this.EventMessage != null && this.EventMessage.Planet != null)
		{
			Astriarch.View.selectPlanet(this.EventMessage.Planet);
		}
	},
	
	/**
	 * fires the TurnSummaryMessageListBoxItem double click event
	 * @this {Astriarch.GameController.TurnSummaryMessageListBoxItem}
	 */
	onDblClick: function() {
		//popup the conflict dialog for list items relating to a conflict
		//TurnSummaryMessageListBoxItem tsmlbi = (TurnSummaryMessageListBoxItem)TurnSummaryItemsListBox.SelectedItem;
		if(!this.EventMessage)
			return;
		if (this.EventMessage.Type == Astriarch.TurnEventMessage.TurnEventMessageType.AttackingFleetLost ||
			this.EventMessage.Type == Astriarch.TurnEventMessage.TurnEventMessageType.DefendedAgainstAttackingFleet ||
			this.EventMessage.Type == Astriarch.TurnEventMessage.TurnEventMessageType.PlanetCaptured ||
			this.EventMessage.Type == Astriarch.TurnEventMessage.TurnEventMessageType.PlanetLost)
		{
			Astriarch.GameController.popupPlanetaryConflictControl(this.EventMessage);
		}
		
	}
});
