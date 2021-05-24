﻿using System.Collections.Generic;

namespace DiscordRPC.H3VR
{
    public static class Common
    {
        public static Dictionary<string, string> SceneNames = new()
        {
            { "MainMenu3", "Main Menu" }
        };
        
        public static class Constants
        {


            public static class DiscordSDK
            {
                public const long CLIENT_ID = 833479922760417340;
                public const string LIBRARY_FILENAME = DiscordRPC.Lib.Constants.DllName + ".dll";
            }
        }
    }
}