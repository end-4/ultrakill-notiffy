using Notiffy.Server;
using Notiffy.UI;
using Notiffy.Utils;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace Notiffy.API {
    public static class NotificationSystem {
        internal static NotificationServer Server;

        /// <summary>
        /// A more convenient way to send notifications. It's named like this because of the similarly named tool on Linux.
        /// </summary>
        /// <param name="summary">Notification title</param>
        /// <param name="body">Notification body text</param>
        /// <param name="iconSprite">Sprite of the icon</param>
        /// <param name="iconFilePath">Path to image for the icon</param>
        /// <returns></returns>
        public static uint NotifySend(string summary, string body, Sprite iconSprite = null, string iconFilePath = "") {
            Notification n = new Notification { Summary = summary, Body = body };
            if (iconFilePath != "") {
                Sprite s = Img2Sprite.LoadNewSprite(iconFilePath);
                n.NotificationIcon = s;
            } else if (iconSprite != null) {
                n.NotificationIcon = iconSprite;
            }
            return Notify(n);
        }

        // Interface
        public static uint Notify(Notification notification) => Server.Notify(notification);
        public static void CloseNotification(uint id) => Server.CloseNotification(id);

        public static event System.Action<uint, ClosedReason> NotificationClosed {
            add => Server.NotificationClosed += value;
            remove => Server.NotificationClosed -= value;
        }

        // Init
        public static void Initialize() {}
        static NotificationSystem() {
            GameObject container = new GameObject("Notiffy_Server");
            Server = container.AddComponent<NotificationServer>();
            Object.DontDestroyOnLoad(container);
            container.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
