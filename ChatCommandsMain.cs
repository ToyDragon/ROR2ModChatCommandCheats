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
        
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            ChatCommandsMain.enabled = value;
            return true;
        }

        public static void ChangePrefab(string playerName, string prefabName)
        {
            PlayerCharacterMasterController[] allPlayers = MonoBehaviour.FindObjectsOfType<PlayerCharacterMasterController>();
            PlayerCharacterMasterController found = null;
            foreach (PlayerCharacterMasterController player in allPlayers)
            {
                if (player.networkUser.GetNetworkPlayerName().GetResolvedName() == playerName)
                {
                    found = player;
                }
            }

            if (found != null)
            {
                var prefab = BodyCatalog.FindBodyPrefab(prefabName);
                if (prefab != null)
                {
                    var oldTransform = found.master.transform.position;
                    var oldRotation = found.master.transform.rotation;
                    found.master.DestroyBody();
                    found.master.bodyPrefab = prefab;
                    found.master.SpawnBody(prefab, oldTransform, oldRotation);
                }
                else
                {
                    Chat.SendBroadcastChat(new Chat.PlayerChatMessage()
                    {
                        networkPlayerName = new NetworkPlayerName()
                        {
                            steamId = new CSteamID(1234),
                            nameOverride = "user"
                        },
                        baseToken = prefabName + " not found"
                    });
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
            ChatCommandsMain.modEntry.Logger.Log("Recieved message[" + ChatCommandsMain.enabled + "]: " + message);
            if (!ChatCommandsMain.enabled)
            {
                return;
            }
            string backmessage = message;
            int ix = message.IndexOf("<noparse>/");
            if (ix >= 0)
            {
                string user = message.Substring("<color=#123456><noparse>".Length, ix - "</noparse>:0123456789012345678901234".Length);
                message = message.Substring(ix + "<noparse>/".Length);
                message = message.Substring(0, message.IndexOf("</noparse>"));

                string[] pieces = message.Split(' ');

                if (pieces[0].ToUpper() == "CHAR" && pieces.Length > 1)
                {
                    ChatCommandsMain.ChangePrefab(user, pieces[1]);
                }
                else if (pieces[0].ToUpper() == "BODIES")
                {
                    message = "Bodies: ";
                    message += string.Join(",", Resources.LoadAll<GameObject>("Prefabs/CharacterBodies/").Select(obj => obj.name));
                }
                else
                {
                    message = "\"" + user + "\" \"" + pieces[0] + "\"";
                    Chat.SendBroadcastChat(new Chat.PlayerChatMessage()
                    {
                        networkPlayerName = new NetworkPlayerName()
                        {
                            steamId = new CSteamID(1234),
                            nameOverride = "user"
                        },
                        baseToken = "\"" + user + "\" said \"" + pieces[0] + "\""
                    });
                }
            }
            message = backmessage;
        }
    }
}
