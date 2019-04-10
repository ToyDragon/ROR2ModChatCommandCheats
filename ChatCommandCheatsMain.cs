using RoR2;
using System;
using UnityEngine;
using UnityModManagerNet;

namespace Frogtown
{
    public class ChatCommandCheatsMain
    {
        public static bool enabled;
        public static UnityModManager.ModEntry modEntry;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            ChatCommandCheatsMain.modEntry = modEntry;
            modEntry.OnToggle = OnToggle;
            FrogtownShared.AddChatCommand("char", OnCharCommand);
            FrogtownShared.AddChatCommand("give", OnGiveCommand);
            enabled = true;
            return true;
        }
        
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            FrogtownShared.ModToggled(value);
            return true;
        }

        private static bool OnGiveCommand(string userName, string[] pieces)
        {
            if (!enabled)
            {
                return false;
            }

            var player = FrogtownShared.GetPlayerWithName(userName);
            int index = 0, count = 0;

            if (pieces.Length >= 2)
            {
                Int32.TryParse(pieces[1], out index);
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

        private static bool OnCharCommand(string userName, string[] pieces)
        {
            if (!enabled)
            {
                return false;
            }

            if(pieces.Length >= 2)
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