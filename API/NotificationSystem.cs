using System;
using System.Collections.Generic;
using Notiffy.Server;
using Notiffy.UI;
using Notiffy.Utils;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;
using Object = UnityEngine.Object;

namespace Notiffy.API {
    public static class NotificationSystem {
        internal static NotificationServer Server;

        /// <summary>
        /// A more convenient way to send notifications. Inspired by the notify-send utility on Linux.
        /// </summary>
        /// <param name="summary">Notification title</param>
        /// <param name="body">Notification body text</param>
        /// <param name="iconSprite">Sprite of the icon</param>
        /// <param name="iconFilePath">Path to image for the icon</param>
        /// <param name="urgency">Urgency of the notification</param>
        /// <param name="appName">The name of the sender</param>
        /// <param name="replaceId">The id to replace, if any</param>
        /// <param name="actions">A Dictionary that maps action identifiers to display text</param>
        /// <param name="expireTime">The timeout of a notification in milliseconds. Leave as -1 for default.</param>
        /// <returns>The ID of the created or updated notification</returns>
        public static uint NotifySend(string summary, string body, Sprite iconSprite = null, string iconFilePath = "",
            Urgency urgency = Urgency.Normal, string appName = "NotifySend", uint replaceId = 0,
            Dictionary<string, string> actions = null, int expireTime = -1) {
            Notification n = new Notification { Summary = summary, Body = body };

            // App name
            n.ApplicationName = appName;

            // Icon
            if (iconFilePath != "") {
                Sprite s = Img2Sprite.LoadNewSprite(iconFilePath);
                n.NotificationIcon = s;
            } else if (iconSprite != null) {
                n.NotificationIcon = iconSprite;
            }

            // Timeout
            n.ExpirationTimeout = expireTime;

            // Replace ID
            if (replaceId != 0) n.ReplacesId = replaceId;

            // Actions
            if (actions != null) {
                n.Actions = new List<string>();
                foreach (KeyValuePair<string, string> kvp in actions) {
                    n.Actions.Add(kvp.Key);
                    n.Actions.Add(kvp.Value);
                }
            }

            // Hints
            if (urgency != Urgency.Normal) {
                n.Hints = new Dictionary<string, object>();
                if (urgency != Urgency.Normal) {
                    n.Hints.Add("urgency", urgency);
                }
            }

            return Notify(n);
        }

        // Interface
        public static uint Notify(Notification notification) => Server.Notify(notification);
        public static void CloseNotification(uint id) => Server.CloseNotification(id);

        public static event Action<uint, ClosedReason> NotificationClosed {
            add => Server.NotificationClosed += value;
            remove => Server.NotificationClosed -= value;
        }

        public static event Action<uint> NotificationDeleted {
            add => Server.NotificationDeleted += value;
            remove => Server.NotificationDeleted -= value;
        }

        public static event Action<uint, string> ActionInvoked {
            add => Server.ActionInvoked += value;
            remove => Server.ActionInvoked -= value;
        }

        public static event Action ReadyForScene;

        internal static void SignalReadyForScene() {
            ReadyForScene?.Invoke();
        }

        // Init
        public static void Initialize() {
        }

        static NotificationSystem() {
            GameObject container = new GameObject("Notiffy_Server");
            Server = container.AddComponent<NotificationServer>();
            Object.DontDestroyOnLoad(container);
            container.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
