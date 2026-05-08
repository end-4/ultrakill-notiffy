using System;
using System.Collections.Generic;
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

        public static readonly FloatField DefaultTimeout;
        public static readonly IntField MaxHistory;
        public static readonly BoolField UseModifierKey;
        public static readonly KeyCodeField ModifierKey;
        public static readonly KeyCodeField NotificationPanelKey;
        public static readonly BoolField ShowDebugOptions;
        public static readonly BoolField FirstRun;
        public static readonly ButtonField SendTestNotificationButton;
        public static readonly ButtonField SendUrgentNotificationButton;

        private static readonly List<uint> ActiveNotifIds = new List<uint>();

        public static void Initialize() {
        }

        private static void UpdateDebugOptionsVisibility() {
            bool show = ShowDebugOptions.value;
            SendTestNotificationButton.hidden = !show;
            SendUrgentNotificationButton.hidden = !show;
            FirstRun.hidden = !show;
        }

        static ConfigManager() {
            config = PluginConfigurator.Create("Notiffy", NotiffyPlugin.PluginGUID);
            string iconPath = Path.Combine(NotiffyPlugin.workingDir, "icon.png");
            if (File.Exists(iconPath)) config.SetIconWithURL(iconPath);

            new ConfigHeader(config.rootPanel, "", 10);
            new ConfigHeader(config.rootPanel, "-- NOTIFICATION SYSTEM --", 24);
            DefaultTimeout = new FloatField(config.rootPanel, "Default timeout (secs)",
                "defaultTimeout", 6);
            MaxHistory = new IntField(config.rootPanel, "Max history length", "maxHistory", 50);
            UseModifierKey = new BoolField(config.rootPanel, "Use modifier key", "modifierKey", true);
            UseModifierKey.postValueChangeEvent += (bool newValue) => {
                if (ModifierKey == null) return;
                ModifierKey.interactable = newValue;
            };
            ModifierKey =
                new KeyCodeField(config.rootPanel, "Modifier key", "notificationPanelModkey", KeyCode.LeftAlt);
            ModifierKey.interactable = UseModifierKey.value;
            NotificationPanelKey =
                new KeyCodeField(config.rootPanel, "Toggle panel keybind", "notificationPanelKey", KeyCode.N);

            new ConfigHeader(config.rootPanel, "", 10);
            new ConfigHeader(config.rootPanel, "-- DEBUG --", 24);
            ShowDebugOptions = new BoolField(config.rootPanel, "Show debug options", "showDebugOptions", false);
            ShowDebugOptions.postValueChangeEvent += (bool b) => {
                UpdateDebugOptionsVisibility();
            };
            SendTestNotificationButton =
                new ButtonField(config.rootPanel, "Send test notification", "testNotification");
            SendTestNotificationButton.onClick += () => {
                uint newId = NotificationSystem.NotifySend("Test notification",
                    "Lorem ipsum ippai dashite tung tung tung tung random long bs text to test wrapping!!!",
                    iconFilePath: iconPath, actions: new Dictionary<string, string>() {
                        { "yes", "Hell yeah!" },
                        { "no", "I'll pass this time..." }
                    });
                ActiveNotifIds.Add(newId);
            };
            SendUrgentNotificationButton =
                new ButtonField(config.rootPanel, "Send urgent test notification", "testNotificationUrgent");
            SendUrgentNotificationButton.onClick += () => {
                NotificationSystem.NotifySend("Urgent notification",
                    "This message should not disappear on its own.", urgency: Urgency.Critical);
            };

            FirstRun = new BoolField(config.rootPanel, "First run", "firstRun", true, true);
            UpdateDebugOptionsVisibility();

            // Internal inits
            NotificationSystem.ActionInvoked += OnActionInvoked;
            NotificationSystem.NotificationClosed += OnNotificationClosed;
            NotificationSystem.NotificationDeleted += OnNotificationDeleted;
        }

        private static void PrintActiveNotifIds() {
            NotiffyPlugin.Log.LogInfo("ACTIVE NOTIFS");
            foreach (var id in ActiveNotifIds) {
                NotiffyPlugin.Log.LogInfo(id);
            }
        }

        private static void OnActionInvoked(uint id, string actionIdentifier) {
            PrintActiveNotifIds();
            NotiffyPlugin.Log.LogInfo(
                $"Some action invoked: {id}, {actionIdentifier}; current tracked {ActiveNotifIds} -> {ActiveNotifIds.Contains(id)}");
            if (ActiveNotifIds.Contains(id)) {
                NotificationSystem.NotifySend("Action invoked", $"ID: {id}\nAction ID: {actionIdentifier}");
                NotiffyPlugin.Log.LogInfo($"INVOKED notification with ID {id}, actionIdentifier: {actionIdentifier}");
                ActiveNotifIds.Remove(id);
            }
        }

        private static void OnNotificationClosed(uint id, ClosedReason reason) {
            if (ActiveNotifIds.Contains(id)) {
                NotiffyPlugin.Log.LogInfo($"DISMISSED notification with ID {id}");
            }
        }

        private static void OnNotificationDeleted(uint id) {
            if (ActiveNotifIds.Contains(id)) {
                ActiveNotifIds.Remove(id);
                NotiffyPlugin.Log.LogInfo($"DELETED notification with ID {id}");
            }
        }
    }
}
