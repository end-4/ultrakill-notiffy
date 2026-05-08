using System.IO;
using Notiffy.API;

namespace Notiffy;

public static class UserHints {
    public static void IssueFirstRunNoticeIfNecessary() {
        if (ConfigManager.FirstRun.value) {
            ConfigManager.FirstRun.value = false;
            string panelToggleKeybind = (ConfigManager.UseModifierKey.value
                ? (ConfigManager.ModifierKey.value.ToString() + "+")
                : "") + ConfigManager.NotificationPanelKey.value.ToString();
            NotificationSystem.NotifySend("<color=#f2ca3a>Notiffy</color>: first run",
                $"To open notification panel, press [<color=#ff8000>{panelToggleKeybind}</color>]. This can be configured in <color=#55c7f6>Options > Plugin Config > Notiffy</color>",
                urgency: Urgency.Critical, iconFilePath: Path.Combine(NotiffyPlugin.workingDir, "icon.png"));
        }
    }

    public static void Initialize() {}

    static UserHints() {
        NotificationSystem.ReadyForScene += IssueFirstRunNoticeIfNecessary;
    }
}
