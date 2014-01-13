/**
 * A DrawnFleet is a graphical representation of a fleet
 * @constructor
 */
Astriarch.DrawnFleet = jCanvas.DrawnObject.extend({ // drawn object class

	/**
	 * initializes this DrawnFleet
	 * @this {Astriarch.DrawnFleet}
	 */
	init: function(/*Fleet*/ f) {
		this.Fleet = f;
		this.Fleet.DrawnFleet = this;//setup backreference
		
		this.DestinationHex = null;//Hexagon
		
		//TODO: move these later
        this.travelingFromHex = null;//Hexagon
		
        //public TextBlock TravelETATextBlock = new TextBlock();
		this.TravelETATextBlockRect = new Astriarch.Rectangle();
		this.TravelETATextBlockText = "ETA";
		
        this.travelDistancePoint = new Astriarch.Point();
        this.totalTravelDistance = 0;
		
		this.TurnsToDestination = 0;
		
		//setup visual elements
		this.TravelFleetRect = new Astriarch.Rectangle();
		this.TravelFleetRect.Height = 10;
		this.TravelFleetRect.Width = 10;

		this.TravelLine = new Astriarch.Line();

	},
	
	/**
	 * Draws this DrawnFleet to the canvas
	 * @this {Astriarch.DrawnFleet}
	 */
	draw: function(ctx) {
		if(this.totalTravelDistance > 0)
		{
			//draw travel line
			ctx.strokeStyle = "green";
			ctx.lineWidth = 1.5;
			ctx.beginPath();
			ctx.moveTo(this.TravelLine.X1, this.TravelLine.Y1);
			ctx.lineTo(this.TravelLine.X2, this.TravelLine.Y2);
			ctx.closePath();
			ctx.stroke();
			
			//draw fleet image
			//TODO: should we cache these images?
			var image = new Image();
			var x = this.TravelFleetRect.X;//images sizes are 32px (TODO: shouldn't be hard-coded?) (also not sure why it needs to be off by 1 (7 instead of 8)
			var y = this.TravelFleetRect.Y;
			image.onload = function() {
				//planetImageLoaded
				ctx.drawImage(image, x, y);
			};
			image.src = "img/starship.png";
			
			//draw ETA text
			ctx.fillStyle = "green";
			ctx.font = "bold 8px sans-serif";
			ctx.textAlign = "left";
			ctx.textBaseline = 'alphabetic';
			//var textWidth = ctx.measureText(this.Planet.BoundingHex.Id);
			ctx.fillText(this.TravelETATextBlockText, this.TravelETATextBlockRect.X, this.TravelETATextBlockRect.Y);
		}
	},
	
	/**
	 * returns false because we never need hit testing for the drawn fleet
	 * @return {boolean}
	 */
	isInBounds: function(x, y) {
		return false;
	},
	
	/**
	 * Sets the DrawnFleet destination
	 * @this {Astriarch.DrawnFleet}
	 */
	SetDestination: function(/*Hexagon*/ travelingFromHex, /*Hexagon*/ destinationHex, /*int?*/ turnsToDestination, /*int?*/ totalTravelDistance) {
		this.travelingFromHex = travelingFromHex;
		this.DestinationHex = destinationHex;
		this.Fleet.DestinationHex = this.DestinationHex;//Setup 'property' backreference
		this.totalTravelDistance = totalTravelDistance;
		if(!this.totalTravelDistance)
		{
			this.totalTravelDistance = Astriarch.GameModel.GameGrid.GetHexDistance(this.travelingFromHex, this.DestinationHex);
		}
		this.TurnsToDestination = this.totalTravelDistance;
		if(turnsToDestination)
		{
			this.TurnsToDestination = turnsToDestination;
		}
		this.updateTravelLine();
	},

	//TODO: need to actually draw on the proper canvas context, or update the fleet's model and have the view or some controller handle the painting
	/**
	 * Updates the DrawnFleet destination travel line on the canvas
	 * @this {Astriarch.DrawnFleet}
	 */
	updateTravelLine: function(){
		if (this.TurnsToDestination != 0)//draw the line
		{
			this.TravelLine.X2 = this.DestinationHex.MidPoint.X;
			this.TravelLine.Y2 = this.DestinationHex.MidPoint.Y;

			var traveled = ((this.totalTravelDistance - this.TurnsToDestination) * 1.0) / (this.totalTravelDistance * 1.0);

			this.travelDistancePoint.X = this.travelingFromHex.MidPoint.X - ((this.travelingFromHex.MidPoint.X - this.TravelLine.X2) * traveled);
			this.travelDistancePoint.Y = this.travelingFromHex.MidPoint.Y - ((this.travelingFromHex.MidPoint.Y - this.TravelLine.Y2) * traveled);

			this.TravelLine.X1 = this.travelDistancePoint.X;
			this.TravelLine.Y1 = this.travelDistancePoint.Y;

			var turns = this.TurnsToDestination > 1 ? " Turns" : " Turn";
			this.TravelETATextBlockText = this.TurnsToDestination + turns;

			var offsetX = 8;
			//if the travel line points from left to right, we will instead show the TravelETATextBlock to the left of the ship indicator
			if (this.TravelLine.X2 > this.travelingFromHex.MidPoint.X)
			{
				if (this.TurnsToDestination > 9)
					offsetX = -40;
				else if (this.TurnsToDestination == 1)
					offsetX = -30;
				else
					offsetX = -35;
			}
			
			this.TravelETATextBlockRect.X = this.travelDistancePoint.X + offsetX;
			this.TravelETATextBlockRect.Y = this.travelDistancePoint.Y + 4;

			this.TravelFleetRect.X = this.travelDistancePoint.X - 5;
			this.TravelFleetRect.Y = this.travelDistancePoint.Y - 5;
		}
		else
		{
			this.totalTravelDistance = 0;
		}
		if(this.layer)
			this.layer.needsDisplay = true;//set dirty flag
	}

});