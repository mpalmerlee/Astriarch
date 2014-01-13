Astriarch.PlanetaryConflictControl = {
	dialog:null,//instance of Astriarch.Dialog
	
	planetaryConflictMessage:null,//TurnEventMessage
	
	init: function() {
		
		Astriarch.PlanetaryConflictControl.dialog = new Astriarch.Dialog('#planetaryConflictDialog', 'Planetary Conflict', 424, 323, Astriarch.PlanetaryConflictControl.OKClose, Astriarch.PlanetaryConflictControl.CancelClose);
	},
	
	show: function(/*TurnEventMessage*/ planetaryConflictMessage) {
		Astriarch.PlanetaryConflictControl.planetaryConflictMessage = planetaryConflictMessage;
		
		var summary = "";
		
		summary += planetaryConflictMessage.Message + "<br />";
		//show resources looted if a planet changed hands
		var resourcesLootedMessage = "";
		if(planetaryConflictMessage.Type == Astriarch.TurnEventMessage.TurnEventMessageType.PlanetCaptured ||
				planetaryConflictMessage.Type == Astriarch.TurnEventMessage.TurnEventMessageType.PlanetLost) {
			resourcesLootedMessage = "No Resources Looted.";
			if(planetaryConflictMessage.Planet.Resources.FoodAmount != 0 || 
			   planetaryConflictMessage.Data.GoldAmountLooted != 0 ||
			   planetaryConflictMessage.Planet.Resources.OreAmount != 0 ||
			   planetaryConflictMessage.Planet.Resources.IridiumAmount != 0) {
			   resourcesLootedMessage = "";
			   resourcesLootedMessage = resourcesLootedMessage + (planetaryConflictMessage.Planet.Resources.FoodAmount != 0 ? planetaryConflictMessage.Planet.Resources.FoodAmount + " Food" : "");
			   resourcesLootedMessage = resourcesLootedMessage + (planetaryConflictMessage.Data.GoldAmountLooted != 0 ? ((resourcesLootedMessage ? ", " : "") + planetaryConflictMessage.Data.GoldAmountLooted + " Gold") : "");
			   resourcesLootedMessage = resourcesLootedMessage + (planetaryConflictMessage.Planet.Resources.OreAmount != 0 ? ((resourcesLootedMessage ? ", " : "") + planetaryConflictMessage.Planet.Resources.OreAmount + " Ore") : "");
			   resourcesLootedMessage = resourcesLootedMessage + (planetaryConflictMessage.Planet.Resources.IridiumAmount != 0 ? ((resourcesLootedMessage ? ", " : "") + planetaryConflictMessage.Planet.Resources.IridiumAmount + " Iridium") : "");
			   
			   resourcesLootedMessage = "Resources looted: " + resourcesLootedMessage;
			}
		}
		summary += resourcesLootedMessage + "<br />";
		var attackingFleetStrength = planetaryConflictMessage.Data.AttackingFleet.DetermineFleetStrength();
		summary += "Attacking Fleet (strength " + attackingFleetStrength + ", Chance to Win: " + planetaryConflictMessage.Data.AttackingFleetChances + "%): <br />";
		summary += planetaryConflictMessage.Data.AttackingFleet.ToString() + "<br /><br />";
		var defendingFleetStrength = planetaryConflictMessage.Data.DefendingFleet.DetermineFleetStrength();
		summary += "Defending Fleet (strength " + defendingFleetStrength + "): <br />";
		summary += planetaryConflictMessage.Data.DefendingFleet.ToString() + "<br /><br />";
		summary += "Ships Remaining: <br />";
		summary += planetaryConflictMessage.Data.WinningFleet.ToString() + "<br />";

		if (!Astriarch.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups) {
			$("#NeverShowPlanetaryConflictPopupsCheckbox").prop('checked', true);
		}
		else {
			$("#NeverShowPlanetaryConflictPopupsCheckbox").prop('checked', false);
		}
		
		$('#PlanetaryConflictSummary').html(summary);
		
		var attackingPlayerName = planetaryConflictMessage.Data.AttackingPlayer.Name;
        var defendingPlayerName = Astriarch.GameTools.PlanetOwnerToFriendlyName(planetaryConflictMessage.Planet, planetaryConflictMessage.Data.DefendingPlayer);
        Astriarch.PlanetaryConflictControl.dialog.setTitle(attackingPlayerName + " Attacked " + defendingPlayerName + " at Planet: " + planetaryConflictMessage.Planet.Name);
		
		Astriarch.PlanetaryConflictControl.dialog.open();
	},
	
	OKClose: function()	{
		Astriarch.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups = true;
		if ($('#NeverShowPlanetaryConflictPopupsCheckbox').attr('checked'))
		{
			Astriarch.GameModel.MainPlayer.Options.ShowPlanetaryConflictPopups = false;
		}
		//this is probably not the best way to do this, but it can't be syncronous because it needs to pop this up again
		setTimeout(function() { Astriarch.GameController.processNextEndOfTurnPlanetaryConflictMessage(); }, 100);
	},

	CancelClose: function()
	{
		//this is probably not the best way to do this, but it can't be syncronous because it needs to pop this up again
		setTimeout(function() { Astriarch.GameController.processNextEndOfTurnPlanetaryConflictMessage(); }, 100);
	}
};