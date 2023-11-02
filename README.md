# Photo Mode for Lonely Mountains: Downhill
A MelonLoader mod that lets you pause time and fly around to position the camera freely, perfect for taking screenshots!


## Setup Instructions
#### Preparing
Your game folder can be found by right-clicking on the game in steam and going 'Manage -> Browse local files'  

Install Melon Loader to your LMD game install folder.  
Look under 'Automated Installation':  
https://melonwiki.xyz/#/  
(v0.6.1 is the current version at time of writing)  

Run the game once then exit. (See **Known Issues & Fixes** if your game freezes on quit)  
If successful the Melon Loader splash screen should appear on launch. 

Download `PhotoModeMod.dll` from the releases and add it to the `Mods` folder in your LMD game folder.  

#### Loading The Mod In-Game
After loading into a level wait to be able to move the bike then hit `Keyboard [P]` or `Gamepad [Y]`.

#### Tweaking values
A config file is generated in `[LMD folder]/UserData/PhotoModeSettings.cfg`.  
This file can be opened with any text editor and contains all the mods settings.  


#### Keybinds
| Keyboard & Mouse      | Gamepad                   | Action                               |
| ---                   | ---                       | ---                                  |
| Mouse                 | Right Stick               | Look around                          |
| W, A, S, D            | Left Stick                | Move around                          |
| Space / LControl      | L-Trigger / R-Trigger     | Move up and down                     |
| LShift                | A Button                  | Move faster while held               |
| P                     | Y Button                  | Toggle photo mode                    |
| H                     | X Button                  | Toggle the game HUD and UI           |
| K                     | Dpad ▲                    | Reset camera rotation and FoV        |
| Mouse Scroll          | L-Bumper / R-Bumper       | Change Field of View                 |
| Q / E                 | Dpad ◄ / ►                | Tilt camera left and right           |
| Hold F + Mouse Scroll | Hold Dpad ▼ + LB / RB     | Change Depth of Field focus distance |


#### Known Issues & Fixes
- Game freezes on quitting: Use [`Alt + Tab`] to select the commandline window and then close it.
- Controls are currently not rebindable
