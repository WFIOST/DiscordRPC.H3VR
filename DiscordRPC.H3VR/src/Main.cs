using System;
using System.Runtime.InteropServices;
using BepInEx;
using DiscordRPC.Lib;
using FistVR;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace DiscordRPC.H3VR
{
    [
        BepInPlugin
        ( 
            "com.wfiost.discordrpc.h3vr",
            "H3VR Discord RPC",
            "1.0.0"
        )
    ]
    public class Plugin : BaseUnityPlugin
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string path);
        
        public Discord Client { get; }

        public Activity CurrentActivity { get; private set; }

        public Plugin()
        {
            Logger.LogInfo("Started DiscordRPC!");
            
            try
            {
                LoadLibrary("BepInEx\\plugins\\DiscordRPC.H3VR\\" + Common.DiscordSDK.LIBRARY_FILENAME);
            }
            catch (Exception e)
            {
                Logger.LogFatal($"FAILED TO LOAD DISCORD GAME SDK LIBRARY ({Common.DiscordSDK.LIBRARY_FILENAME})\n\nEXCEPTION:\n{e}");
                Logger.LogFatal($"MAKE SURE YOU HAVE {Common.DiscordSDK.LIBRARY_FILENAME} IN THE RIGHT LOCATION!");
                return;
            }
            
            Logger.LogInfo($"Successfully loaded Discord GameSDK library ({Common.DiscordSDK.LIBRARY_FILENAME})");
            
            Client = new Discord(Common.DiscordSDK.CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
            
            Logger.LogInfo($"Successfully created Client!");
            
            
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += NewSceneLoaded;

            CurrentActivity = new Activity
            {
                Name    = "H3VR",
                State   = $"Currently in {SceneManager.GetActiveScene().name}"
            };
            Logger.LogInfo("Set default activity!");
        }

        private void Update()
        {
            Client.RunCallbacks();
            UpdateRPC(CurrentActivity);
        }

        private void NewSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.LogInfo($"CURRENT SCENE: {scene.name}");

            if (scene.name == "TakeAndHoldClassic")
            {
                Logger.LogInfo($"SETTING ACTIVITY TO {scene.name}");
                CurrentActivity = new Activity 
                {
                    State = $"Take and Hold - {GM.TNH_Manager.C.DisplayName}, {TNH_EQUIPMENTMODE_NAMES[GM.TNH_Manager.EquipmentMode]}, {TNH_HEALTHMODE_NAMES[GM.TNH_Manager.HealthMode]}",
                    Details = $"On hold {GM.TNH_Manager.m_curHoldIndex}, with {GM.TNH_Manager.m_numTokens} tokens"
                };

                //CurrentActivity = SCENE_SPECIFIC_ACTIVITY[scene.name];
                return;
            }

            CurrentActivity = new Activity
            {
                Name    = "H3VR",
                State   = $"Currently in {scene.name}"
            };
        }

        public void UpdateRPC(Activity activity)
        {
            Client.GetActivityManager().UpdateActivity
            (
                activity,       
                result =>
                {
                    if (result != Result.Ok)
                    {
                        Logger.LogInfo("ERROR! Could not update activity!");
                        return;
                    }
                }
            );
        }

        /*
        public static readonly Dictionary<string, Activity> SCENE_SPECIFIC_ACTIVITY = new()
        {
            
            { 
                "TakeAndHoldClassic",
                new Activity 
                {
                    State = $"Take and Hold - {GM.TNH_Manager.C.DisplayName}, {TNH_EQUIPMENTMODE_NAMES[GM.TNH_Manager.EquipmentMode]}, {TNH_HEALTHMODE_NAMES[GM.TNH_Manager.HealthMode]}",
                    Details = $"On hold {GM.TNH_Manager.m_curHoldIndex}, with {GM.TNH_Manager.m_numTokens} tokens"
                } 
            }
            
        };
        */


        private static readonly Dictionary<TNHSetting_EquipmentMode, string> TNH_EQUIPMENTMODE_NAMES = new()
        {
            { TNHSetting_EquipmentMode.LimitedAmmo,     "Limited Ammo" },
            { TNHSetting_EquipmentMode.Spawnlocking,    "Spawnlocked" }
        };

        private static readonly Dictionary<TNHSetting_HealthMode, string> TNH_HEALTHMODE_NAMES = new()
        {
            { TNHSetting_HealthMode.StandardHealth,     "Standard Health" },
            { TNHSetting_HealthMode.HardcoreOneHit,     "One-hit" },
            { TNHSetting_HealthMode.CustomHealth,       "Custom health" }
        };
    }
}