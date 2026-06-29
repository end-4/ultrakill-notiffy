using System;
using System.IO;
using Notiffy.API;
using UnityEngine.SceneManagement;

namespace Notiffy;

internal static class UserHints {
    private static void IssueFirstRunNoticeIfNecessary() {
        if (!ConfigManager.FirstRun.value) return;
        ConfigManager.FirstRun.value = false;
        NotificationSystem.NotifySend("<color=#f2ca3a>Notiffy</color>: instructions",
            $"To open notification panel, press [{ConfigManager.GetFormattedPanelKeybind()}]. This can be configured in <color=#55c7f6>Options > Plugin Config > Notiffy</color>",
            iconFilePath: Path.Combine(NotiffyPlugin.workingDir, "icon.png"));
    }

    private static void OnSceneLoaded(Scene s, LoadSceneMode m) {
        try {
            IssueFirstRunNoticeIfNecessary();
        } catch (Exception e) {
            NotiffyPlugin.Log.LogError("UserHints scene load error " + e);
        }
    }

    public static void Initialize() {
    }

    static UserHints() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
}
