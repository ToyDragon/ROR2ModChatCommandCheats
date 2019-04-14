using BepInEx;
using RoR2;
using System;
using UnityEngine;

namespace Frogtown
{
    [BepInDependency("com.frogtown.shared")]
    [BepInPlugin("com.frogtown.chatcheats", "Cheat Chat Commands", "1.0.3")]
    public class ChatCommandCheatsMain : BaseUnityPlugin
    {
        public ModDetails modDetails;

        public void Awake()
        {
            modDetails = new ModDetails("com.frogtown.chatcheats")
            {
                description = "Adds the /change_char and /give_item chat commands.",
                githubAuthor = "ToyDragon",
                githubRepo = "ROR2ModChatCommandCheats",
            };
            FrogtownShared.RegisterMod(modDetails);

            FrogtownShared.AddChatCommand("change_char", OnCharCommand);
            FrogtownShared.AddChatCommand("give_item", OnGiveCommand);
            FrogtownShared.AddChatCommand("remove_item", OnRemoveCommand);
            FrogtownShared.AddChatCommand("clear_items", OnClearItemsCommand);
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