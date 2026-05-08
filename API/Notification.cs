using System.Collections.Generic;
using UnityEngine;

namespace Notiffy.API {

    /// <summary>
    /// Represents a request to display a notification to the user. Based on FreeDesktop specs.
    /// https://specifications.freedesktop.org/notification/latest-single/#basic-design
    /// </summary>
    public struct Notification {
        /// <summary>
        /// The name of the notification sender.
        /// </summary>
        public string ApplicationName;

        /// <summary>
        /// An optional ID of an existing notification that this notification is intended to replace.
        /// </summary>
        public uint ReplacesId;

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

        /// <summary>
        /// Notification hints.
        /// Extra info outside the standard fields that the notification server may or may not make use of.
        /// </summary>
        public Dictionary<string,object> Hints;

        /// <summary>
        /// Actions are sent over as a list of pairs.
        /// Each even element in the list (starting at index 0) represents the identifier for the action.
        /// Each odd element in the list is the localized string that will be displayed to the user.
        /// </summary>
        public List<string> Actions;
    }
}
