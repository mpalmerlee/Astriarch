REM java -jar closure_compiler\compiler.jar --compilation_level ADVANCED_OPTIMIZATIONS --js_output_file astriarch.js < astriarch-manifest.txt

REM java -jar closure_compiler\compiler.jar --compilation_level ADVANCED_OPTIMIZATIONS --js_output_file astriarch.js --externs jquery-1.4.4.externs.js ^
REM --js "./js/jquery-1.4.4.min.js" ^
REM --formatting PRETTY_PRINT
java -jar closure_compiler\compiler.jar --compilation_level ADVANCED_OPTIMIZATIONS ^
--js_output_file astriarch.js ^
--externs jquery-1.4.4.externs.js ^
--externs "./js/jquery-ui-1.8.7.custom.min.js" ^
--externs "./js/js-inherit.js" ^
--externs "./js/jCanvas.js" ^
--externs "./js/audio-interface.js" ^
--externs "./js/js-listbox.js" ^
--externs "./js/jquery.ui.selectmenu.js" ^
--externs "./js/ui.checkbox.js" ^
--externs AstriarchExtern.js ^
--js "./astriarch/astriarch_globals.js" ^
--js "./astriarch/astriarch_view.js" ^
--js "./astriarch/astriarch_grid.js" ^
--js "./astriarch/astriarch_hexagon.js" ^
--js "./astriarch/astriarch_model.js" ^
--js "./astriarch/astriarch_player.js" ^
--js "./astriarch/astriarch_planet.js" ^
--js "./astriarch/astriarch_fleet.js" ^
--js "./astriarch/astriarch_turneventmessage.js" ^
--js "./astriarch/astriarch_battlesimulator.js" ^
--js "./astriarch/astriarch_ai.js" ^
--js "./astriarch/astriarch_drawnplanet.js" ^
--js "./astriarch/astriarch_drawnfleet.js" ^
--js "./astriarch/astriarch_dialog.js" ^
--js "./astriarch/astriarch_gamecontroller.js" ^
--js "./astriarch/astriarch_planetview.js" ^
--js "./astriarch/astriarch_sendshipscontrol.js" ^
--js "./astriarch/astriarch_planetaryconflictcontrol.js" ^
--js "./astriarch/astriarch_gameovercontrol.js" ^
--js "./astriarch/astriarch_alert.js" ^
--js "./astriarch/astriarch_savedgameinterface.js" 2> output-debug.txt

REM --js "./js/jquery-ui-1.8.7.custom.min.js" ^
REM --js "./js/js-inherit.js" ^
REM --js "./js/jCanvas.js" ^
REM --js "./js/js-listbox.js" ^
REM --js "./js/jquery.ui.selectmenu.js" ^
REM --js "./js/ui.checkbox.js" ^