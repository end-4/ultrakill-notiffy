namespace Notiffy.API {
    /// <summary>
    /// Urgency level. See https://specifications.freedesktop.org/notification/latest-single/#urgency-levels
    /// </summary>
    public enum Urgency : uint {
        /// <summary>
        /// Unimportant notification
        /// </summary>
        Low = 0,
        /// <summary>
        /// Standard urgency
        /// </summary>
        Normal = 1,
        /// <summary>
        /// Important. Messages with this urgency do not expire without user action.
        /// </summary>
        /// <remarks>
        /// Use this for first-time instructions that are critical to read
        /// </remarks>
        Critical = 2,
    }
}
