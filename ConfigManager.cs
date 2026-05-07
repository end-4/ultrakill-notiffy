using System;
using System.IO;
using Notiffy.API;
using PluginConfig.API;
using PluginConfig.API.Decorators;
using PluginConfig.API.Fields;
using PluginConfig.API.Functionals;
using UnityEngine;

namespace Notiffy {
    public class ConfigManager {
        public static PluginConfigurator? config;

        public static FloatField defaultTimeout;
        public static IntField maxHistory;
        public static KeyCodeField notificationPanelKey;

        public static ButtonField sendTestNotificationButton;

        public static void Initialize() {
            config = PluginConfigurator.Create("Notiffy", NotiffyPlugin.PluginGUID);
            string iconPath = Path.Combine(NotiffyPlugin.workingDir, "icon.png");
            if (File.Exists(iconPath)) config.SetIconWithURL(iconPath);

            new ConfigHeader(config.rootPanel, "", 10);
            new ConfigHeader(config.rootPanel, "-- NOTIFICATION SYSTEM --", 24);
            defaultTimeout = new FloatField(config.rootPanel, "Default timeout (secs)",
                "defaultTimeout", 6);
            maxHistory = new IntField(config.rootPanel, "Max history length", "maxHistory", 50);
            notificationPanelKey =
                new KeyCodeField(config.rootPanel, "Toggle panel keybind", "notificationPanelKey", KeyCode.N);

            new ConfigHeader(config.rootPanel, "", 10);
            new ConfigHeader(config.rootPanel, "-- DEBUG --", 24);
            sendTestNotificationButton = new ButtonField(config.rootPanel, "Send test notification", "testNotification");
            sendTestNotificationButton.onClick += () => {
                NotificationSystem.NotifySend("Test notification",
                    "Lorem ipsum ippai dashite tung tung tung tung random long bs text to test wrapping!!!", iconFilePath: iconPath);
            };

        }
    }
}
