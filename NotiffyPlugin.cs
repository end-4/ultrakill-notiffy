using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Notiffy.API;
using Notiffy.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Input = UnityEngine.Input;

namespace Notiffy;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("com.eternalUnion.pluginConfigurator")]
public class NotiffyPlugin : BaseUnityPlugin {
    public const string PluginGUID = "com.github.end-4.notiffy";
    public const string PluginName = "Notiffy";
    public const string PluginVersion = "0.1.4";
    public static string workingPath = Assembly.GetExecutingAssembly().Location;
    public static string workingDir = Path.GetDirectoryName(workingPath);

    internal static ManualLogSource Log;
    internal static event Action SafeSceneLoaded;

    void Awake() {
        Log = BepInEx.Logging.Logger.CreateLogSource(PluginName);
        ConfigManager.Initialize();
        NotificationSystem.Initialize();
        NotificationController.Initialize();
        MenuPatcher.Initialize();
        UserHints.Initialize();
        Harmony harmony = new Harmony("Notiffy");
        harmony.PatchAll();
        SceneManager.sceneLoaded += OnSceneLoaded;
        Log.LogInfo("Notiffy loaded");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        try {
            if (string.IsNullOrEmpty(SceneHelper.CurrentScene) || SceneHelper.CurrentScene == "Bootstrap" ||
                SceneHelper.CurrentScene == "Intro") return;
            SafeSceneLoaded?.Invoke();
        } catch (Exception e) {
            Log.LogInfo($"Error on scene load: {e}");
        }
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
