namespace Notiffy.API {
    /// <summary>
    /// Notification interface for applications. Based on FreeDesktop specs.
    /// https://xdg-specs-technobaboo-f55ac9d85e73073a0c8831695ba0fb110849811c0.pages.freedesktop.org/notification-spec/latest/ar01s09.html
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
    }
}
