using Notiffy.API;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace Notiffy.Server {
    internal class NotificationServer : MonoBehaviour, INotificationServer {
        // Internals
        private ManualLogSource Log;
        private readonly Dictionary<uint, NotificationEntry> _activeNotifs;
        private uint _nextId = 1;
        private readonly List<uint> _removalBuffer = new();

        // For the displaying client
        public bool Silent = false;
        private readonly List<NotificationEntry> _history = [];
        public IReadOnlyList<NotificationEntry> History => _history;
        public IEnumerable<NotificationEntry> ActiveNotifications => _activeNotifs.Values;

        public event System.Action<uint, Notification>? OnNotificationAdded;
        public event System.Action<uint, Notification>? OnNotificationUpdated;
        public event System.Action<uint, ClosedReason>? OnNotificationClosed;
        public event System.Action<uint>? OnNotificationDeleted;

        // For notifying clients
        public event System.Action<uint, ClosedReason>? NotificationClosed;

        private bool NotificationIsActive(uint id) {
            return _activeNotifs.ContainsKey(id);
        }

        public void TrimHistory() {
            if (_history.Count > ConfigManager.maxHistory.value) {
                _history.RemoveRange(0, _history.Count - ConfigManager.maxHistory.value);
            }
        }

        private void AddToHistory(NotificationEntry notification) {
            _history.Add(notification);
            TrimHistory();
        }

        private void RemoveFromHistory(uint id) {
            int index = _history.FindIndex(x => x.Id == id);
            if (index == -1) return;
            _history.RemoveAt(index);
        }

        public void DeleteNotification(uint id, ClosedReason reason = ClosedReason.Dismissed) {
            if (NotificationIsActive(id)) CloseNotification(id, reason);
            if (_history.FindIndex(x => x.Id == id) != -1) {
                RemoveFromHistory(id);
                OnNotificationDeleted?.Invoke(id);
            }
        }

        public void CloseNotification(uint id, ClosedReason reason) {
            if (NotificationIsActive(id)) {
                // Log.LogInfo($"Closing notification {id}");
                _activeNotifs.Remove(id);
                NotificationClosed?.Invoke(id, reason);
                OnNotificationClosed?.Invoke(id, reason);
            }
        }

        public void ToggleSilence() {
            Silent = !Silent;
        }

        public void ClearNotifications(bool delete = true) {
            _removalBuffer.Clear();
            foreach (var entry in _history) {
                _removalBuffer.Add(entry.Id);
            }

            foreach (var t in _removalBuffer) {
                if (delete) DeleteNotification(t);
                else CloseNotification(t, ClosedReason.Dismissed);
            }
        }

        public uint Notify(Notification notification) {
            uint idToReturn;

            if (notification.ReplacesId != 0 && _activeNotifs.TryGetValue(notification.ReplacesId, out var active)) {
                active.UpdateNotification(notification);
                idToReturn = notification.ReplacesId;
                OnNotificationUpdated?.Invoke(idToReturn, notification);
                // Log.LogInfo($"Updated notification {idToReturn}");
            } else {
                idToReturn = _nextId++;
                NotificationEntry newNotifEntry = new NotificationEntry(notification, idToReturn);
                _activeNotifs.Add(idToReturn, newNotifEntry);
                AddToHistory(newNotifEntry);
                OnNotificationAdded?.Invoke(idToReturn, notification);
                // Log.LogInfo($"Created notification {idToReturn}");
            }

            return idToReturn;
        }

        /// <summary>
        /// Dismisses the notification, doesn't remove it.
        /// </summary>
        /// <param name="id">The id of the notification to close</param>
        public void CloseNotification(uint id) {
            CloseNotification(id, ClosedReason.APIClosed);
        }

        public NotificationServer() {
            Log = BepInEx.Logging.Logger.CreateLogSource("Notiffy.Server");
            _activeNotifs = new Dictionary<uint, NotificationEntry>();
            Log.LogInfo("Initialized");
        }

        // Handle timeouts
        void Update() {
            // Log.LogInfo("Update");
            if (_activeNotifs.Count == 0) return;
            float currentTime = Time.time;
            _removalBuffer.Clear();
            // See which timed out. Gotta buffer like this, or we'd be modifying the list while iterating through it
            foreach (var entry in _activeNotifs.Values) {
                // Use the individual notification's timeout if available, else fallback to global
                float timeout = entry.Data.ExpirationTimeout > 0
                    ? entry.Data.ExpirationTimeout
                    : ConfigManager.defaultTimeout.value;

                if (currentTime - entry.StartTime >= timeout) {
                    // Log.LogInfo($"Time notif {entry.Id}: {currentTime - entry.StartTime} / {timeout}");
                    _removalBuffer.Add(entry.Id);
                }
            }

            // Close the ones that timed out
            for (int i = 0; i < _removalBuffer.Count; i++) {
                CloseNotification(_removalBuffer[i], ClosedReason.Expired);
            }
        }
    }
}
