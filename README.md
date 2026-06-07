# UNBEATABLE Better Discord Activity

A mod to improve the Discord activity in the hit rhythm game UNBEATABLE!
Note that this mod only adds support for arcade mode.

## Mod Installation Instructions

- Download the latest release of the mod from the releases page, and extract the DLL file from inside the zip
- Download BepInEx from [here](https://github.com/BepInEx/BepInEx/releases) and extract the BepInEx folder from the zip into the main UNBEATABLE game code folder (the one that contains UNBEATABLE.exe)
- Run the game once and close it
- Put the mod DLL into the BepInEx\plugins folder

The structure should then be:

<pre>
UNBEATABLE
├─── UNBEATABLE.exe
├─── UNBEATABLE_Data
├─── {some other folders and files}
├─── .doorstop_version
├─── changelog.txt
├─── doorstop_config.ini
├─── winhttp.dll
└─── BepInEx
    ├─── cache
    ├─── config
    ├─── core
    ├─── patchers
    └─── plugins
        ├─── SomeMod.dll
        └─── SomeOtherMod.dll
</pre>

Once the mod is in the folder, restart the game and it should load.
