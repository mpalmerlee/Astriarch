Astriarch.GameOverControl = {
	dialog:null,//instance of Astriarch.Dialog
	
	winningPlayer:null,//Player
	
	init: function() {
		
		Astriarch.GameOverControl.dialog = new Astriarch.Dialog('#gameOverDialog', 'Game Over', 424, 313, Astriarch.GameOverControl.OKClose, Astriarch.GameOverControl.CancelClose);
	},
	
	show: function(/*Player*/ winningPlayer, /*bool*/ mainPlayerWon) {
		Astriarch.GameOverControl.winningPlayer = winningPlayer;
		var score = Astriarch.GameTools.CalculateEndGamePoints(Astriarch.GameModel.MainPlayer, mainPlayerWon);
		var summary = "";

		if (mainPlayerWon)
			summary += "You conquered all of your enemies and reign supreme over the known universe!<br />You will be known as the first Astriarch - Ruler of the Stars.<br />";
		else
			summary += "You lost control over all your fleets and planets and have been crushed by the power of your enemies!<br />";

		summary += "In " + Astriarch.GameModel.Turn.Number + " turns.<br />";
		
		summary += winningPlayer.Name + " won the game with " + Astriarch.CountObjectKeys(winningPlayer.OwnedPlanets) + " planets.<br />";
		summary += "<br />Your points: " + score;
		
		AstriarchExtern.OnGameOver({'PlayerWon':mainPlayerWon, 'Turns':Astriarch.GameModel.Turn.Number, 'Score':score});
		
		$('#GameOverSummary').html(summary);
		
        Astriarch.GameOverControl.dialog.setTitle("Game Over, Player: " + winningPlayer.Name + " Wins!");
		
		Astriarch.GameOverControl.dialog.open();
	},

	OKClose: function()	{
		Astriarch.GameController.GameOverControlClosed();
	},

	CancelClose: function()
	{
		Astriarch.GameController.GameOverControlClosed();
	}
};