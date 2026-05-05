using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Notiffy.API;
using UnityEngine;

namespace Notiffy {
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class NotiffyPlugin : BaseUnityPlugin {
        public const string PluginGUID = "com.github.end-4.notiffy";
        public const string PluginName = "Notiffy";
        public const string PluginVersion = "0.1.0";
        public static string workingPath = Assembly.GetExecutingAssembly().Location;
        public static string workingDir = Path.GetDirectoryName(workingPath);

        // Use the built-in Logger provided by BaseUnityPlugin
        internal static ManualLogSource Log;

        private void Awake() {
            Log = BepInEx.Logging.Logger.CreateLogSource(PluginName);
            Log.LogInfo("Notiffy is waking up...");
            ConfigManager.Initialize();
            NotificationSystem.Initialize();

            Log.LogInfo("Notiffy Server and API Initialized.");

        }
    }
}
