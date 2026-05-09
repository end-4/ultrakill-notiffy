namespace Notiffy.API {
    /// <summary>
    /// Notification interface for applications. Based on FreeDesktop specs.
    /// https://specifications.freedesktop.org/notification/latest-single/#protocol
    /// </summary>
    public interface INotificationServer {
        /// <summary>
        /// Sends a notification to the server.
        /// </summary>
        /// <param name="notification">The notification data to display.</param>
        /// <returns>
        /// A unique ID for the notification.
        /// </returns>
        /// <remarks>
        /// This ID can be used to update the notification in a subsequent call
        /// or to close it manually via <see cref="CloseNotification"/>.
        /// </remarks>
        uint Notify(Notification notification);

        /// <summary>
        /// Causes a notification to be forcefully dismissed.
        /// </summary>
        void CloseNotification(uint id);

        /// <summary>
        /// Signal emitted when a notification is dismissed or expires (not deleted)
        /// uint: The notification ID
        /// ClosedReason: Why it was closed (Expired, Dismissed, etc.)
        /// </summary>
        event System.Action<uint, ClosedReason>? NotificationClosed;

        /// <summary>
        /// Emitted when a notification is deleted completely.
        /// This is outside the FreeDesktop spec, but that's the major flaw of it.
        /// Not reacting fast enough within 5sec-ish does not mean uninterested.
        /// uint: The notification ID
        /// </summary>
        event System.Action<uint>? NotificationDeleted;

        /// <summary>
        /// Emitted when a notification action is invoked.
        /// uint: the notification ID
        /// string: the identifier of the action
        /// </summary>
        event System.Action<uint, string>? ActionInvoked;
    }
}
