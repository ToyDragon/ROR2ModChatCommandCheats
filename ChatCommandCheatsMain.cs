using BepInEx;
using RoR2;
using System;
using UnityEngine;

namespace Frogtown
{
    [BepInDependency("com.frogtown.shared")]
    [BepInPlugin("com.frogtown.chatcheats", "Cheat Chat Commands", "1.0.4")]
    public class ChatCommandCheatsMain : BaseUnityPlugin
    {
        public FrogtownModDetails modDetails;

        private GUIStyle buttonStyle;
        private bool guiInit;
        private Vector2 itemScrollPos;
        private ItemIndex selectedIndex;
        private bool includeNoTier;
        private string selectedBodyName;
        private Vector2 bodyScrollPos;
        private bool includeNoIcon;

        public void Awake()
        {
            modDetails = new FrogtownModDetails("com.frogtown.chatcheats")
            {
                description = "Adds the /change_char and /give_item chat commands.",
                githubAuthor = "ToyDragon",
                githubRepo = "ROR2ModChatCommandCheats",
                OnGUI = OnSettingsGUI
            };
            FrogtownShared.RegisterMod(modDetails);

            FrogtownShared.AddChatCommand("change_char", OnCharCommand);
            FrogtownShared.AddChatCommand("give_item", OnGiveCommand);
            FrogtownShared.AddChatCommand("remove_item", OnRemoveCommand);
            FrogtownShared.AddChatCommand("clear_items", OnClearItemsCommand);
        }

        private void OnSettingsGUI()
        {
            if (!guiInit)
            {
                guiInit = true;

                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.padding = new RectOffset(0, 0, 0, 0);
            }

            GUILayout.Label("Item Cheats");

            includeNoTier = GUILayout.Toggle(includeNoTier, "Include unfinished items?");

            itemScrollPos = GUILayout.BeginScrollView(itemScrollPos, GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            ItemDef selectedItem = null;
            foreach (var itemIndex in ItemCatalog.allItems)
            {
                var itemDef = ItemCatalog.GetItemDef(itemIndex);
                if (!includeNoTier && itemDef.tier == ItemTier.NoTier)
                {
                    continue;
                }

                var name = Language.GetString(itemDef.nameToken);
                if (GUILayout.Toggle(selectedIndex == itemDef.itemIndex, new GUIContent(itemDef.pickupIconTexture, name), buttonStyle, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    selectedIndex = itemDef.itemIndex;
                }

                if(selectedIndex == itemDef.itemIndex)
                {
                    selectedItem = itemDef;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            if (selectedItem != null)
            {
                GUILayout.BeginHorizontal();
                var name = Language.GetString(selectedItem.nameToken);
                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    GUILayout.BeginVertical();
                    var pname = player.GetDisplayName();
                    if (GUILayout.Button("Give 1x " + name + " to " + pname, buttonStyle))
                    {
                        player.master.inventory.GiveItem(selectedIndex, 1);
                    }
                    if (GUILayout.Button("Give 5x " + name + " to " + pname, buttonStyle))
                    {
                        player.master.inventory.GiveItem(selectedIndex, 5);
                    }
                    if (GUILayout.Button("Give 25x " + name + " to " + pname, buttonStyle))
                    {
                        player.master.inventory.GiveItem(selectedIndex, 25);
                    }
                    if (GUILayout.Button("Remove all " + name + " from " + pname, buttonStyle))
                    {
                        player.master.inventory.GiveItem(selectedIndex, -player.master.inventory.GetItemCount(selectedIndex));
                    }
                    GUILayout.Space(20);
                    if (GUILayout.Button("Remove every item from " + pname, buttonStyle))
                    {
                        foreach (var itemIndex in ItemCatalog.allItems)
                        {
                            player.master.inventory.GiveItem(itemIndex, -player.master.inventory.GetItemCount(itemIndex));
                        }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Character Cheats");

            includeNoIcon = GUILayout.Toggle(includeNoIcon, "Include no icon?");

            bodyScrollPos = GUILayout.BeginScrollView(bodyScrollPos, GUILayout.ExpandHeight(false));
            GUILayout.BeginHorizontal();
            GameObject selectedPrefab = null;
            foreach (var prefab in BodyCatalog.allBodyPrefabs)
            {
                var bodyComp = prefab.GetComponent<CharacterBody>();

                name = "";
                if (bodyComp != null)
                {
                    name = bodyComp.GetDisplayName();
                }
                if (name.Length == 0)
                {
                    name = prefab.name;
                }

                if (!includeNoIcon && bodyComp?.portraitIcon == null)
                {
                    continue;
                }
                if (GUILayout.Toggle(selectedBodyName == prefab.name, new GUIContent(bodyComp.portraitIcon, name), buttonStyle, GUILayout.Width(64), GUILayout.Height(64)))
                {
                    selectedBodyName = prefab.name;
                }

                if(selectedBodyName == prefab.name)
                {
                    selectedPrefab = prefab;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            if (selectedPrefab != null)
            {
                GUILayout.BeginHorizontal();
                var bodyComp = selectedPrefab.GetComponent<CharacterBody>();

                name = "";
                if (bodyComp != null)
                {
                    name = bodyComp.GetDisplayName();
                }
                if(name.Length == 0)
                {
                    name = selectedPrefab.name;
                }

                foreach (var player in PlayerCharacterMasterController.instances)
                {
                    var pname = player.GetDisplayName();
                    if (GUILayout.Button("Respawn " + pname + " as " + name, buttonStyle))
                    {
                        var pbody = player.master.GetBodyObject();
                        if (pbody != null && pbody.transform != null)
                        {
                            var oldPos = pbody.transform.position;
                            var oldRot = pbody.transform.rotation;
                            player.master.DestroyBody();
                            player.master.bodyPrefab = selectedPrefab;
                            player.master.SpawnBody(selectedPrefab, oldPos, oldRot);
                            pbody.transform.position = oldPos;
                            pbody.transform.rotation = oldRot;
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

        }

        private bool OnRemoveCommand(string userName, string[] pieces)
        {
            if (!modDetails.enabled)
            {
                return false;
            }

            var player = FrogtownShared.GetPlayerWithName(userName);
            int index = 0, count = 0;

            if (pieces.Length >= 2)
            {
                if (!Int32.TryParse(pieces[1], out index))
                {
                    if (Enum.TryParse(pieces[1], true, out ItemIndex result))
                    {
                        index = (int)result;
                    }
                    else
                    {
                        FrogtownShared.SendChat("\"" + pieces[1] + "\" not recognized.");
                        return true;
                    }
                }
            }

            if (pieces.Length >= 3)
            {
                Int32.TryParse(pieces[2], out count);
            }
            int countPlayerHas = player.master.inventory.GetItemCount((ItemIndex)index);
            count = Math.Min(Math.Max(count, 1), countPlayerHas);

            if (count > 0)
            {
                player.master.inventory.GiveItem((ItemIndex)index, -count);
                FrogtownShared.SendChat("Took " + count + " " + ((ItemIndex)index).ToString() + " from " + userName + ".");
            }

            return true;
        }

        private bool OnClearItemsCommand(string userName, string[] pieces)
        {
            if (!modDetails.enabled)
            {
                return false;
            }

            var player = FrogtownShared.GetPlayerWithName(userName);
            foreach (ItemIndex itemIndex in ItemCatalog.allItems)
            {
                int count = player.master.inventory.GetItemCount(itemIndex);
                if(count > 0)
                {
                    player.master.inventory.GiveItem(itemIndex, -count);
                }
            }
            FrogtownShared.SendChat("Took all items from " + userName + ".");

            return true;
        }

        private bool OnGiveCommand(string userName, string[] pieces)
        {
            if (!modDetails.enabled)
            {
                return false;
            }

            var player = FrogtownShared.GetPlayerWithName(userName);
            int index = 0, count = 0;

            if (pieces.Length >= 2)
            {
                if(!Int32.TryParse(pieces[1], out index))
                {
                    if(Enum.TryParse(pieces[1], true, out ItemIndex result)){
                        index = (int)result;
                    }
                    else
                    {
                        FrogtownShared.SendChat("\"" + pieces[1] + "\" not recognized.");
                        return true;
                    }
                }
            }
            if (index < 0 || index >= (int)ItemIndex.Count)
            {
                index = (int)ItemIndex.SprintOutOfCombat;
            }

            if (pieces.Length >= 3)
            {
                Int32.TryParse(pieces[2], out count);
            }
            if(count == 0)
            {
                count = 1;
            }

            player.master.inventory.GiveItem((ItemIndex)index, count);
            FrogtownShared.SendChat("Gave " + userName + " " + count + " " + ((ItemIndex)index).ToString());

            return true;
        }

        private bool OnTestCommand(string userName, string[] pieces)
        {
            FrogtownShared.SendChat("User " + string.Join(",", pieces));
            return true;
        }

        private bool OnCharCommand(string userName, string[] pieces)
        {
            if (!modDetails.enabled)
            {
                return false;
            }

            if (pieces.Length >= 2)
            {
                int prefabIndex = -1;
                if (!Int32.TryParse(pieces[1], out prefabIndex))
                {
                    prefabIndex = BodyCatalog.FindBodyIndexCaseInsensitive(pieces[1]);
                }
                if (prefabIndex != -1)
                {
                    GameObject prefab = BodyCatalog.GetBodyPrefab(prefabIndex);

                    if (prefab != null)
                    {
                        if (FrogtownShared.ChangePrefab(userName, prefab))
                        {
                            FrogtownShared.SendChat(userName + " morphed into " + prefab.name);
                        }
                        else
                        {
                            FrogtownShared.SendChat(userName + " couldn't morph into " + prefab.name);
                        }
                    }
                    else
                    {
                        FrogtownShared.SendChat("Prefab not found");
                    }
                }
            }

            return true;
        }
    }
}