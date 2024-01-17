# DevdudeX's Photo Mode for Lonely Mountains: Downhill
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/L4L5S9BK3)

A MelonLoader mod that lets you pause time and fly around to position the camera freely, perfect for taking screenshots!

![Photo mode](/images/banner.png?raw=true)

## Setup Instructions
#### Preparing
Your game folder can be found by right-clicking on the game in steam and going 'Manage -> Browse local files'  

Install Melon Loader to your LMD game install folder.  
Look under 'Automated Installation':  
https://melonwiki.xyz/#/  
(v0.6.1 is the current version at time of writing)  

Run the game once then exit. (See **Known Issues & Fixes** if your game freezes on quit)  
If successful the Melon Loader splash screen should appear on launch. 

Download `PhotoMode.dll` from the releases and add it to the `Mods` folder in your LMD game folder.  

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
| F                     | Dpad ▼                    | Switch between FoV or DoF mode       |
| Mouse Scroll          | L-Bumper / R-Bumper       | Adjust FoV or DoF focus              |
| Q / E                 | Dpad ◄ / ►                | Tilt camera left and right           |



#### Known Issues & Fixes
- Controls are currently not rebindable.  
- Game freezes on quitting: Add the `--quitfix` [MelonLoader launch option](https://github.com/LavaGang/MelonLoader#launch-options).  
On steam: right-click on LMD --> Properties --> Launch Options --> Paste the command (with `--` infront!).
