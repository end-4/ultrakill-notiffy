using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Notiffy.API;
using Notiffy.UI;
using UnityEngine;
using Input = UnityEngine.Input;

namespace Notiffy {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("com.eternalUnion.pluginConfigurator")]
    public class NotiffyPlugin : BaseUnityPlugin {
        public const string PluginGUID = "com.github.end-4.notiffy";
        public const string PluginName = "Notiffy";
        public const string PluginVersion = "0.1.3";
        public static string workingPath = Assembly.GetExecutingAssembly().Location;
        public static string workingDir = Path.GetDirectoryName(workingPath);

        // Use the built-in Logger provided by BaseUnityPlugin
        internal static ManualLogSource Log;

        void Awake() {
            Log = BepInEx.Logging.Logger.CreateLogSource(PluginName);
            ConfigManager.Initialize();
            NotificationSystem.Initialize();
            NotificationController.Initialize();
            MenuPatcher.Initialize();
            UserHints.Initialize();
            Harmony harmony = new Harmony("Notiffy");
            harmony.PatchAll();
            Log.LogInfo("Notiffy loaded");
        }

        float previousTimeScale = 0;

        void Update() {
            if (!ConfigManager.UseModifierKey.value || Input.GetKey(ConfigManager.ModifierKey.value)) {
                if (Input.GetKeyDown(ConfigManager.NotificationPanelKey.value)) {
                    NotificationController.TogglePanel();
                }
            }

            if (Time.timeScale * previousTimeScale > 0 || Time.timeScale + previousTimeScale == 0) {
                // When pause state changed
                NotificationController.UpdatePopupTail();
            }

            previousTimeScale = Time.timeScale;
        }
    }
}
