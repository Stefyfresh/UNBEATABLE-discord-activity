using BepInEx;
using BepInEx.Logging;
using System.Collections.Generic;
using HarmonyLib;
using Rhythm;
using DG.Tweening;
using UnityEngine;
using Arcade.UI.SongSelect;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System;
using System.Linq;
using UnityEngine.SceneManagement;


namespace BetterDiscordActivity
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("UNBEATABLE.exe")]
    public class BetterDiscordActivity : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "net.stefyfresh.BetterDiscordActivity";
        public const string PLUGIN_NAME = "Stefyfresh Better Discord Activity";

        public const string PLUGIN_VERSION = "1.1.0";
        internal static new ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch]
    class ActivityControllerSceneLoadedPatches
    {
        static System.Reflection.MethodBase TargetMethod()
        {
            // Need to use scuffed Reflection stuff because method is weird
            return typeof(DiscordActivityController).GetMethod("OnSceneLoad", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod([typeof(Scene), typeof(LoadSceneMode)]);
        }
        static bool Prefix(ref DiscordActivityController __instance, ref Scene LoadedScene)
        {
            __instance.hasUpdatedSongActivity = false;
            __instance.storyStateActivityTimer = 0f;
            __instance.loadedSceneName = ((Scene)Convert.ChangeType(LoadedScene, typeof(Scene))).name;
            __instance.rhythmController = global::UnityEngine.Object.FindObjectOfType<RhythmController>();
            __instance.discordComponent = global::UnityEngine.Object.FindObjectOfType<DiscordComponent>();
            return false;
        }
    }


    [HarmonyPatch(typeof(JeffBezosController))]
    [HarmonyPatch("Awake")]
    internal class JeffBezosControllerPatches
    {
        static bool Prefix(ref JeffBezosController __instance)
        {
            if (JeffBezosController.instance == null)
            {
                __instance.gameObject.AddComponent<DiscordComponent>();
                __instance.gameObject.AddComponent<DiscordActivityController>();
            }
            return true;
        }
    }


    [HarmonyPatch(typeof(DiscordComponent))]
    [HarmonyPatch("Awake")]
    internal class DiscordComponentPatches
    {
        static bool Prefix(ref DiscordComponent __instance)
        {
            if (!__instance.IsConnected && __instance.name == "JeffBezos")
            {
                __instance.IsConnected = true;
                try
                {
                    // App ID removed just in case
                    __instance.discord = new global::Discord.Discord(UNBEATABLEAppID, 1UL);
                }
                catch
                {
                    __instance.IsConnected = false;
                    return false;
                }
                __instance.activityManager = __instance.discord.GetActivityManager();
                // __instance.activity.Name = "UNBEATABLE";
                // __instance.activity.Details = "Testing\nTest";
                // __instance.activity.State = "State";
                // __instance.updateActivity = true;
            }
            return false;
        }
    }


    [HarmonyPatch(typeof(DiscordActivityController))]
    [HarmonyPatch("Update")]
    internal class DiscordActivityControllerUpdatePatches
    {
        static bool Prefix(ref DiscordActivityController __instance)
        {
            if (__instance.discordComponent == null)
            {
                __instance.discordComponent = global::UnityEngine.Object.FindObjectOfType<DiscordComponent>();
            }
            if (__instance.activities == null)
            {
                __instance.activities = new DiscordActivities();
            }
            if (__instance.discordComponent == null || !__instance.discordComponent.IsConnected || __instance.hasUpdatedSongActivity)
            {
                return false;
            }
            if (__instance.rhythmController != null && __instance.rhythmController.beatmap != null)
            {
                __instance.discordComponent.activity.Details = "Playing " + __instance.rhythmController.beatmap.metadata.title + string.Format(" [{0}]", __instance.rhythmController.beatmap.metadata.GetDifficulty("Custom"));
                // __instance.discordComponent.activity.Details = "Playing " + __instance.rhythmController.beatmap.metadata.title + string.Format(" [{0}]", __instance.rhythmController.beatmap.metadata.GetDifficulty("Custom")) + " - " + __instance.rhythmController.beatmap.metadata.artist;
                string state = "";
                if (JeffBezosController.GetNoFail() > 0)
                {
                    state += "/no fail";
                }
                if (JeffBezosController.GetAssistMode() > 0)
                {
                    state += "/assist";
                }
                if (JeffBezosController.GetSongSpeed() > 0)
                {
                    int songSpeed = JeffBezosController.GetSongSpeed();
                    state += string.Format("/{0}", (songSpeed == 1) ? "halftime" : "doubletime");
                }
                if (JeffBezosController.GetCriticalMode() > 0)
                {
                    state += "/critical";
                }
                if (state != "")
                {
                    state = "/" + state;
                }
                __instance.discordComponent.activity.State = state;
                __instance.discordComponent.updateActivity = true;
                __instance.hasUpdatedSongActivity = true;
                return false;
            }
            string details = "";
            if (__instance.loadedSceneName == "ArcadeModeMenu")
            {
                details = "In Arcade Mode";
            }
            if (__instance.loadedSceneName == "C2_MainMenu")
            {
                details = "In Main Menu";
            }
            if (__instance.loadedSceneName == "ScoreScreenArcadeMode")
            {
                details = "Reviewing Scores";
            }
            if (details != __instance.discordComponent.activity.Details)
            {
                __instance.discordComponent.activity.Details = details;
                __instance.discordComponent.activity.State = "";
                __instance.discordComponent.updateActivity = true;
                __instance.hasUpdatedSongActivity = true;
                return false;
            }
            return false;
        }
    }

}