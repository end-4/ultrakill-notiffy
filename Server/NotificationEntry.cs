using Notiffy.API;
using UnityEngine;

namespace Notiffy.Server {

    /// <summary>
    /// The notification object that contains also runtime info like id and start time.
    /// Wraps around a content-only Notification.
    /// </summary>
    internal class NotificationEntry {
        public Notification Data;
        public float StartTime;
        public uint Id;

        public NotificationEntry(Notification notification, uint id) {
            UpdateNotification(notification);
            Id = id;
        }

        public void UpdateNotification(Notification notification) {
            this.Data = notification;
            this.StartTime = Time.time;
        }

        public bool IsTimedout() {
            return Time.time >= (StartTime + Data.ExpirationTimeout);
        }
    }
}
