/**
 * @constructor
 * @private
 */
function jCanvas() { };

jCanvas.Global = {layerNextZIndex: 1};

jCanvas.prototype.getLayer = function(name){ };

jCanvas.prototype.draw = function() { }:

jCanvas.prototype.layerSortFunction = function(a, b) { };

jCanvas.Layer = function(selector, name, x, y, width, height, zIndex) { };

jCanvas.Layer.prototype.draw = function() { };

jCanvas.Layer.prototype.isInBounds = function(x, y) { };

jCanvas.Layer.prototype.addChild = function(drawnObject) { };

jCanvas.DrawnObject = function() {};

