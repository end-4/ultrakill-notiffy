using UnityEngine;

namespace Notiffy.API {

    /// <summary>
    /// Represents a request to display a notification to the user. Based on FreeDesktop specs.
    /// https://xdg-specs-technobaboo-f55ac9d85e73073a0c8831695ba0fb110849811c0.pages.freedesktop.org/notification-spec/latest/ar01s02.html
    /// </summary>
    public struct Notification {
        /// <summary>
        /// The name of the notification sender.
        /// </summary>
        public string ApplicationName;

        /// <summary>
        /// An optional ID of an existing notification that this notification is intended to replace.
        /// </summary>
        public int ReplacesId;

        /// <summary>
        /// The icon for the notification.
        /// </summary>
        public Sprite NotificationIcon;

        /// <summary>
        /// Single-line title, for example "MyMegaFireMod Updated to 0.6.9!"
        /// </summary>
        public string Summary;

        /// <summary>
        /// The body text, possibly multi-line.
        /// </summary>
        public string Body;

        /// <summary>
        /// How long the notification stays on screen in milliseconds.
        /// -1: Use the default
        /// 0: Never expire
        /// </summary>
        public int ExpirationTimeout;

        // Not available (yet): Actions, Hints
    }
}
