# Frogtown Cheatsasdasdasds
This mod adds a few cheats to the settings page that the host can access, and adds chat commands for other players to use.

![In game popup](https://github.com/ToyDragon/ROR2ModChatCommandCheats/blob/master/Images/UI.png?raw=true)

![Close up](https://github.com/ToyDragon/ROR2ModChatCommandCheats/blob/master/Images/closeup.png?raw=true)

# Installation
1. Install [BepInEx Mod Pack](https://thunderstore.io/package/bbepis/BepInExPack/)
2. Install [Frogtown Mod Manager](https://thunderstore.io/package/ToyDragon/SharedModLibrary/)
3. Download the latest ToyDragon-CheatingChatCommands.zip
4. Move ChatCommandCheats.dll to your \BepInEx\plugins folder

## Chat Commands

Other players in the game can use these chat commands to give themselves items, or switch characters.

/change_char \<body name or index\>
- Switch to a different character. You can use the body name if you know it, such as:
  - /change_char BanditBody
  - /change_char EnforcerBody
  - /change_char HANDBody
  - /change_char SniperBody
  - /change_char ElectricWormBody
- Or you can use the body index such as "/change_char 9" for BeetleQueen2Body or "/change_char 33" for HaulerBody. If playing multiplayer the server host must have the mod installed, and it will work for all players in the match.  
  
/give_item \<item number\> [item count]
- Give yourself some free items. You can use the item name if you know it, such as:
  - /give_item Syringe
  - /give_item Dagger 5
  - /give_item Hoof
  - /give_item SprintOutOfCombat 10
  - /give_item Clover 4
- Or you can use the item index, for example "/give_item 5" will add a Ceremonial Dagger to your inventory, and "/give_item 21 100" will make you the fastest living thing on the planet.

/remove_item \<item number\> [item count]
- Same as give item, but instead it removes them.

/clear_items
- Removes all items from your inventory.

## Frogtown Cheats Versions
- 1.0.8
  - Update documentation to point to thunderstore instead of github.

# Other Mods
Check out the [Frogtown Mod Manager](https://thunderstore.io/package/ToyDragon/SharedModLibrary/) page to see more cool mods.