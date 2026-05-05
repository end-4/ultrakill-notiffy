namespace Notiffy.API {
    /// <summary>
    /// The reason why a notification was closed. Matches the XDG Notifications Desktop Specification.
    /// https://specifications.freedesktop.org/notification/latest-single/#signal-notification-closed
    /// </summary>
    public enum ClosedReason : uint {
        /// <summary> The notification expired due to its timeout. </summary>
        Expired = 1,

        /// <summary> The notification was dismissed by the user. </summary>
        Dismissed = 2,

        /// <summary> The notification was closed by a call to CloseNotification. </summary>
        APIClosed = 3,

        /// <summary> Undefined or reserved reason. </summary>
        Undefined = 4
    }
}
