using System;
using System.Runtime.InteropServices;
using BepInEx;
using DiscordRPC.Lib;
using FistVR;
using UnityEngine.SceneManagement;

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
                LoadLibrary("BepInEx\\plugins\\DiscordRPC.H3VR\\" + Common.Constants.DiscordSDK.LIBRARY_FILENAME);
            }
            catch (Exception e)
            {
                Logger.LogFatal($"FAILED TO LOAD DISCORD GAME SDK LIBRARY ({Common.Constants.DiscordSDK.LIBRARY_FILENAME})\n\nEXCEPTION:\n{e}");
                Logger.LogFatal($"MAKE SURE YOU HAVE {Common.Constants.DiscordSDK.LIBRARY_FILENAME} IN THE RIGHT LOCATION!");
                return;
            }
            
            Logger.LogInfo($"Successfully loaded Discord GameSDK library ({Common.Constants.DiscordSDK.LIBRARY_FILENAME})");
            
            Client = new Discord(Common.Constants.DiscordSDK.CLIENT_ID, (ulong)CreateFlags.NoRequireDiscord);
            
            Logger.LogInfo($"Successfully created Client!");
            
            
        }

        private void Awake()
        {
            SceneManager.sceneLoaded += SetDefault;
            Logger.LogInfo("Set default activity!");
        }

        private void Update()
        {
            Client.RunCallbacks();
            UpdateRPC(CurrentActivity);
        }

        private void SetDefault(Scene scene, LoadSceneMode mode)
        {
            CurrentActivity = new Activity
            {
                Name    = "Modded H3VR",
                State   = $"Currently in {SceneManager.GetActiveScene().name}",
                Details = $"This user is holding {HeldObject}!"
            };
        }

        public void UpdateRPC(Activity newActivity)
        {
            Client.GetActivityManager().UpdateActivity
            (
                newActivity,       
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

        public string HeldObject
        {
            get
            {
                var str = string.Empty;
                var otherHand = GM.CurrentPlayerBody.LeftHand.GetComponent<FVRViveHand>().OtherHand;
                if (otherHand.CurrentInteractable is FVRPhysicalObject currentInteractable)
                {
                    if (currentInteractable.ObjectWrapper != null)
                        str = !IM.HasSpawnedID(currentInteractable.ObjectWrapper.ItemID) ? currentInteractable.ObjectWrapper.DisplayName : IM.GetSpawnerID(currentInteractable.ObjectWrapper.ItemID).DisplayName;
                }

                return str;
            }
        }
    }
}