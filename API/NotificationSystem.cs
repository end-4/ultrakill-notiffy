using Notiffy.Server;
using UnityEngine;
using Logger = BepInEx.Logging.Logger;

namespace Notiffy.API {
    public static class NotificationSystem {
        internal static INotificationServer _server;

        // Convenience overload
        public static uint NotifySend(string summary, string body = "") {
            return Notify(new Notification { Summary = summary, Body = body });
        }

        // Init
        public static void Initialize() {}
        static NotificationSystem() {
            GameObject container = new GameObject("Notiffy_Server");
            Object.DontDestroyOnLoad(container);
            _server = container.AddComponent<NotificationServer>();
        }

        // Interface
        public static uint Notify(Notification notification) => _server.Notify(notification);
        public static void CloseNotification(uint id) => _server.CloseNotification(id);

        public static event System.Action<uint, ClosedReason> NotificationClosed {
            add => _server.NotificationClosed += value;
            remove => _server.NotificationClosed -= value;
        }
    }
}
