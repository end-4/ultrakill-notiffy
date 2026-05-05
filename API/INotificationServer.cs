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
        /// Signal emitted when a notification is closed or expires.
        /// uint: The ID of the notification.
        /// ClosedReason: Why it was closed (Expired, Dismissed, etc.)
        /// </summary>
        event System.Action<uint, ClosedReason>? NotificationClosed;
    }
}
