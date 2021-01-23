# VRKanojo_Plugins
A collection of BepInEx plugins for VRKanojo.

### VRK_PlayWithoutVR
This plugin lets you play the game with mouse and keyboard if there is no HMD connected. You still need SteamVR to be installed, but you don't need a VR set. 

To use this plugin, simply disconnect and VR headsets and start the game. You should see a list of controls in the top left corner.

Expect it to be janky, playing with an HMD is always going to be a much better experience.

### VRK_UncensorLoader 
This is a simple uncensor loader that makes it possible to easily swap uncensors from plugin settings (press F1 if you have ConfigManager, or check `BepInEx\config` after running the game once). It might be necessary to reload the game for the uncensor to change (usually it's enough for the scene to change).

Uncensors need to be converted for use with this plugin. Check releases for the latest uncensor pack and use it as a template.

No game files are ever replaced so it's easy to disable and switch uncensors, and verifying local files doesn't remove the uncensor.

## Installation
1. Make sure your game is updated and has at least [BepInEx v5.4.4](https://github.com/BepInEx/BepInEx) x64 installed and properly configured (change entry point Type to MonoBehaviour). To use these plugins properly I recommend to also install MessageCenter and ConfigurationManager.
2. Download the latest release.
3. Extract contents of the release archive directly into your game's directory.
4. Start the game and see if there are any errors on screen.
