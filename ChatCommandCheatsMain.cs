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
            enabled = true;
            return true;
        }
        
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            FrogtownShared.ModToggled(value);
            return true;
        }

        private static bool OnCharCommand(string userName, string[] pieces)
        {
            if (!enabled)
            {
                return false;
            }

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
                    if(FrogtownShared.ChangePrefab(userName, prefab))
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
            return true;
        }
    }
}