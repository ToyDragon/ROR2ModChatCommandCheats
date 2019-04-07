using Harmony;
using RoR2;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace Frogtown
{
    public class ChatCommandsMain
    {
        public static bool enabled;
        public static UnityModManager.ModEntry modEntry;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            ChatCommandsMain.modEntry = modEntry;
            var harmony = HarmonyInstance.Create("com.frogtown.chatcommands");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            modEntry.OnToggle = OnToggle;
            enabled = true;
            return true;
        }

        public static void SendChat(string message)
        {
            Chat.SendBroadcastChat(new Chat.PlayerChatMessage()
            {
                networkPlayerName = new NetworkPlayerName()
                {
                    steamId = new CSteamID(1234),
                    nameOverride = "user"
                },
                baseToken = message
            });
        }
        
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            ChatCommandsMain.enabled = value;
            return true;
        }

        public static PlayerCharacterMasterController GetPlayerWithName(string playerName)
        {
            PlayerCharacterMasterController[] allPlayers = MonoBehaviour.FindObjectsOfType<PlayerCharacterMasterController>();
            foreach (PlayerCharacterMasterController player in allPlayers)
            {
                if (player.networkUser.GetNetworkPlayerName().GetResolvedName() == playerName)
                {
                    return player;
                }
            }

            return null;
        }

        public static void ChangePrefab(string playerName, GameObject prefab)
        {
            if (prefab != null)
            {
                PlayerCharacterMasterController player = GetPlayerWithName(playerName);

                if (player != null)
                {
                    var oldTransform = player.master.transform.position;
                    var oldRotation = player.master.transform.rotation;
                    player.master.DestroyBody();
                    player.master.bodyPrefab = prefab;
                    player.master.SpawnBody(prefab, oldTransform, oldRotation); //transform and rotation don't work? please fix?
                }
                else
                {
                    SendChat("Player " + player + " not found");
                }
            }
        }
    }

    [HarmonyPatch(typeof(RoR2.Chat))]
    [HarmonyPatch("AddMessage")]
    [HarmonyPatch(new Type[] { typeof(string) })]
    class ChatPatch
    {
        static void Prefix(ref string message)
        {
            if (!ChatCommandsMain.enabled)
            {
                return;
            }

            if (ParseUserAndMessage(message, out string user, out string text))
            {
                ChatCommandsMain.modEntry.Logger.Log("Recieved command " + text + " from user " + user);
                string[] pieces = text.Split(' ');

                if (pieces[0].ToUpper() == "CHAR" && pieces.Length > 1)
                {
                    int prefabIndex = -1;
                    if (!Int32.TryParse(pieces[1], out prefabIndex))
                    {
                        prefabIndex = BodyCatalog.FindBodyIndexCaseInsensitive(pieces[1]);
                    }
                    if (prefabIndex != -1)
                    {
                        GameObject prefab = BodyCatalog.GetBodyPrefab(prefabIndex);
                        
                        if(prefab != null)
                        {
                            ChatCommandsMain.ChangePrefab(user, prefab);
                            ChatCommandsMain.SendChat(user + " morphed into " + prefab.name);
                        }
                        else
                        {
                            ChatCommandsMain.SendChat("Prefab not found");
                        }
                    }
                }
            }
        }

        public static bool ParseUserAndMessage(string input, out string user, out string message)
        {
            user = "";
            message = "";
            int ix = input.IndexOf("<noparse>/");
            if (ix >= 0)
            {
                int start = "<color=#123456><noparse>".Length;
                int len = ix - "</noparse>:0123456789012345678901234".Length; // lol
                user = input.Substring(start, len);
                message = input.Substring(ix + "<noparse>/".Length);
                message = message.Substring(0, message.IndexOf("</noparse>"));
                return true;
            }

            return false;
        }
    }
}
