using System;
using System.Collections.Generic;
using Notiffy.Server;
using Notiffy.UI;
using Notiffy.Utils;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;
using Object = UnityEngine.Object;

namespace Notiffy.API {
    /// <summary>
    /// The singleton that serves as the endpoint for interactions between Notiffy and other mods.
    /// </summary>
    /// <remarks>
    /// Explore the NotifySend method to get started.
    /// </remarks>
    public static class NotificationSystem {
        internal static NotificationServer Server;

        /// <summary>
        /// Sends a notification using specified properties, with arguments similar to the notify-send utility on Linux.
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

        /// <summary>
        /// Sends a Notification object.
        /// </summary>
        /// <param name="notification">The Notification object to send</param>
        /// <returns>The ID of the created or updated notification</returns>
        public static uint Notify(Notification notification) => Server.Notify(notification);

        /// <summary>
        /// Closes (not delete) notification with the specified ID.
        /// </summary>
        /// <param name="id">ID of the closed notification</param>
        public static void CloseNotification(uint id) => Server.CloseNotification(id);
        

        /// <summary>
        /// Emitted when a notification is closed (not deleted). You most likely want NotificationDeleted instead.
        /// The uint is notification ID and ClosedReason is the closed reason
        /// </summary>
        public static event Action<uint, ClosedReason> NotificationClosed {
            add => Server.NotificationClosed += value;
            remove => Server.NotificationClosed -= value;
        }

        /// <summary>
        /// Emitted when a notification is deleted. 
        /// uint: ID of the notification
        /// </summary>
        /// <remarks>
        /// You can use this to know when to stop listening for an action response.
        /// </remarks>
        public static event Action<uint> NotificationDeleted {
            add => Server.NotificationDeleted += value;
            remove => Server.NotificationDeleted -= value;
        }

        /// <summary>
        /// Emitted when an action is chosen by the user.
        /// uint: ID of the notification
        /// string: identifier of the action
        /// </summary>
        public static event Action<uint, string> ActionInvoked {
            add => Server.ActionInvoked += value;
            remove => Server.ActionInvoked -= value;
        }

        /// <summary>
        /// [DEPRECATED] Emitted when the notification panels are ready for the currently loaded scene.
        /// This was a hack to deal with race conditions and is no longer necessary
        /// </summary>
        [Obsolete]
        public static event Action ReadyForScene;

        internal static void SignalReadyForScene() {
            ReadyForScene?.Invoke();
        }

        internal static void Initialize() {
        }

        static NotificationSystem() {
            GameObject container = new GameObject("Notiffy_Server");
            Server = container.AddComponent<NotificationServer>();
            Object.DontDestroyOnLoad(container);
            container.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
