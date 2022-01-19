# Touch Disable Tray

# Introduction

A simple application meant to live in your system tray and enable you to quickly and easily disable the touch screen on your device.  Intended  for those switching between touch and pen usage on hybrid devices.  Originally developed for and runs by default with the [Eve V](https://evedevices.com/pages/eve-v).

# Installation
Run immediately by launching `release/TouchDisableTray.exe`.   

### For an installation that runs at startup:
*  copy files from the `Release` directory to a new folder in C:/Program Files/TouchDisableTray (or wherever you like).
*  Hold `Alt` and drag `TouchDisableTray.exe` to create a shortcut.
*  Press `Windows Key + R` to open run menu and enter `shell:startup` to open the startup folder
*  Drag the shortcut you created into the startup folder


## Acknowledgements
All credit to [Kevin Anderson](https://bitbucket.org/kevinanderson42/touchdisabletray/), this repo is simply to open the code to the github community.

This project uses the back-end from the [WinDeviceManagerLight](https://github.com/Shinao/WinDeviceManagerLight/) application.  This made it much easier to use the back-end windows libraries for device management.

I will also acknowledge the tutorial for C#-based tray applications from Michael Sorens located at [Redgate Hub](https://www.red-gate.com/simple-talk/dotnet/net-framework/creating-tray-applications-in-net-a-practical-guide/).  The Custom Application Context class is partially copied & modified from this.

