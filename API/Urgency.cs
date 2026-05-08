namespace Notiffy.API {
    /// <summary>
    /// Urgency level. See https://specifications.freedesktop.org/notification/latest-single/#urgency-levels
    /// </summary>
    public enum Urgency : uint {
        Low = 0,
        Normal = 1,
        Critical = 2,
    }
}
