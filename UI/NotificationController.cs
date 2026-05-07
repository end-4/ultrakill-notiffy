using System.Collections.Generic;
using System.IO;
using System.Linq;
using Notiffy.API;
using Notiffy.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Notiffy.UI {
    public static class NotificationController {
        private static readonly string BundlePath = Path.Combine(NotiffyPlugin.workingDir, "assets", "notiffy_ui");
        private static GameObject? _notifPanel;
        private static GameObject? _notifPopupPanel;
        private static AssetBundle? _bundle;
        private static string _panelObjectName = "NotiffyPanel";
        private static string _popupPanelObjectName = "NotiffyPopupPanel";
        private static NotificationServer _server = NotificationSystem.Server;

        private static readonly Dictionary<uint, GameObject> NotifObjectDict =
            new Dictionary<uint, GameObject>();
        private static readonly Dictionary<uint, GameObject> PopupNotifObjectDict =
            new Dictionary<uint, GameObject>();

        private static Sprite _largeBorder = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_BorderLarge.png").WaitForCompletion();
        private static Sprite _largeFill = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_FillLarge.png").WaitForCompletion();

        public static void Initialize() {
            if (_bundle != null) return; // Already initialized
            SceneManager.sceneLoaded += OnSceneLoaded;

            _server.OnNotificationAdded += OnNotificationAdded;
            _server.OnNotificationUpdated += OnNotificationUpdated;
            _server.OnNotificationClosed += OnNotificationClosed;
            _server.OnNotificationDeleted += OnNotificationDeleted;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            ParentStuffToCanvas();
            UpdateSilentButtonFill();
        }

        private static void OnNotificationAdded(uint id, Notification notif) {
            // NotiffyPlugin.Log.LogInfo($"Created notification {id} with title {notif.Data.Summary}");
            // Find notif panel content
            Transform contentTransform, popupContentTransform;
            if (_notifPanel != null) {
                contentTransform = _notifPanel.transform.Find("Scroll View/Viewport/NotiffyPanelContent");
                GameObject? newNotif = CreateNotificationGameObject(notif, id, false);
                if (newNotif != null) {
                    newNotif.transform.SetParent(contentTransform, false);
                    NotifObjectDict.Add(id, newNotif);
                }
            }
            if (!NotificationSystem.Server.Silent && _notifPopupPanel != null && _notifPopupPanel.activeSelf) {
                popupContentTransform = _notifPopupPanel.transform.Find("Viewport/PopupContent");
                GameObject? newPopupNotif = CreateNotificationGameObject(notif, id, true);
                if (newPopupNotif != null) {
                    newPopupNotif.transform.SetParent(popupContentTransform, false);
                    PopupNotifObjectDict.Add(id, newPopupNotif);
                }
            }
        }

        private static void OnNotificationUpdated(uint id, Notification notif) {
            if (NotifObjectDict.TryGetValue(id, out var notifObject)) {
                UpdateNotificationGameObjectContent(notifObject, notif);
            }

            if (PopupNotifObjectDict.TryGetValue(id, out var popupNotifObject)) {
                UpdateNotificationGameObjectContent(popupNotifObject, notif);
            }
        }

        private static void OnNotificationClosed(uint id, ClosedReason reason = ClosedReason.Expired) {
            // NotiffyPlugin.Log.LogInfo($"Closed notif {id} with reason {reason}");
            if (PopupNotifObjectDict.TryGetValue(id, out var popupNotifObject)) {
                Object.Destroy(popupNotifObject);
                PopupNotifObjectDict.Remove(id);
            }
        }

        private static void OnNotificationDeleted(uint id) {
            if (NotifObjectDict.TryGetValue(id, out var notifObject)) {
                Object.Destroy(notifObject);
                NotifObjectDict.Remove(id);
            }

            if (PopupNotifObjectDict.TryGetValue(id, out var popupNotifObject)) {
                Object.Destroy(popupNotifObject);
                PopupNotifObjectDict.Remove(id);
            }
        }

        private static void UpdateNotificationGameObjectContent(GameObject notifObj, Notification notif) {
            TextMeshProUGUI summary = notifObj.transform.Find("NotificationTextLayout/SummaryText")
                .GetComponent<TextMeshProUGUI>();
            summary.text = notif.Summary;

            TextMeshProUGUI body = notifObj.transform.Find("NotificationTextLayout/BodyText")
                .GetComponent<TextMeshProUGUI>();
            body.text = notif.Body;

            if (notif.NotificationIcon != null) {
                Image notifIcon = notifObj.transform.Find("AppIconMask/Image").GetComponent<Image>();
                notifIcon.sprite = notif.NotificationIcon;
            }
        }

        private static GameObject? CreateNotificationGameObject(Notification notif, uint id, bool isPopup = false) {
            // Load prefab from bundle
            _bundle = AssetBundle.LoadFromFile(BundlePath);
            if (_bundle == null) {
                // NotiffyPlugin.Log.LogError("Could not find asset bundle!");
                return null;
            }

            GameObject notifPrefab = _bundle.LoadAsset<GameObject>("NotiffyNotification");
            // NotiffyPlugin.Log.LogInfo($"Prefab asset: {notifPrefab}");
            _bundle.Unload(false);

            // Make the game object
            GameObject newNotifObj = Object.Instantiate(notifPrefab);
            Button closeButton = newNotifObj.transform.Find("CloseButton").GetComponent<Button>();
            closeButton.onClick.AddListener(isPopup
                ? () => { NotificationSystem.Server.CloseNotification(id, ClosedReason.Dismissed); }
                : () => { NotificationSystem.Server.DeleteNotification(id); });
            UpdateNotificationGameObjectContent(newNotifObj, notif);

            newNotifObj.SetActive(true);
            return newNotifObj;
        }

        private static void SyncNotificationObjects() {
            NotifObjectDict.Clear();
            IReadOnlyList<NotificationEntry> hist = NotificationSystem.Server.History;
            for (int i = 0; i < hist.Count; i++) {
                OnNotificationAdded(hist[i].Id, hist[i].Data);
            }
        }

        private static void UpdateSilentButtonFill() {
            bool silent = NotificationSystem.Server.Silent;
            Image btnBg = _notifPanel.transform.Find("Header/NotiffyHeaderSilent").GetComponent<Image>();
            Image icon = _notifPanel.transform.Find("Header/NotiffyHeaderSilent/Image").GetComponent<Image>();
            btnBg.sprite = silent ? _largeFill : _largeBorder;
            icon.color = silent ? Color.black : Color.white;
        }

        private static void LoadPanelFromBundle() {
            // NotiffyPlugin.Log.LogInfo("RE-INSTANTIATING PANEL");
            _bundle = AssetBundle.LoadFromFile(BundlePath);
            if (_bundle == null) {
                // NotiffyPlugin.Log.LogError("Could not find asset bundle!");
                return;
            }

            // Load prefab
            GameObject panelPrefab = _bundle.LoadAsset<GameObject>("NotiffyPanel");
            // NotiffyPlugin.Log.LogInfo($"Prefab asset: {panelPrefab}");
            _bundle.Unload(false);

            // Instantiate
            _notifPanel = Object.Instantiate(panelPrefab);
            _notifPanel.SetActive(true);
            _notifPanel.SetActive(false);
            _notifPanel.name = _panelObjectName; // Give it a fixed name to find later
            Object.DontDestroyOnLoad(_notifPanel);

            // Hook buttons
            Button clearButton = _notifPanel.transform.Find("Header/NotiffyHeaderClose").GetComponent<Button>();
            clearButton.onClick.AddListener(() => { NotificationSystem.Server.ClearNotifications(); });
            Button silentButton = _notifPanel.transform.Find("Header/NotiffyHeaderSilent").GetComponent<Button>();
            silentButton.onClick.AddListener(() => {
                NotificationSystem.Server.ToggleSilence();
                List<int> ids = [];
                foreach (int id in PopupNotifObjectDict.Keys) {
                    ids.Add(id);
                }
                foreach (uint id in ids) {
                    OnNotificationClosed(id);
                }

                UpdateSilentButtonFill();
            });

            // Add missed stuff
            SyncNotificationObjects();
        }

        private static void LoadPopupPanelFromBundle() {
            // NotiffyPlugin.Log.LogInfo("RE-INSTANTIATING POPUP PANEL");
            _bundle = AssetBundle.LoadFromFile(BundlePath);
            if (_bundle == null) {
                // NotiffyPlugin.Log.LogError("Could not find asset bundle!");
                return;
            }

            // Load prefab
            GameObject panelPrefab = _bundle.LoadAsset<GameObject>("NotiffyPopupScrollView");
            // NotiffyPlugin.Log.LogInfo($"Prefab asset: {panelPrefab}");
            _bundle.Unload(false);

            // Instantiate
            _notifPopupPanel = Object.Instantiate(panelPrefab);
            _notifPopupPanel.SetActive(true);
            _notifPopupPanel.name = _popupPanelObjectName; // Give it a fixed name to find later
            Object.DontDestroyOnLoad(_notifPopupPanel);
        }

        private static void ParentStuffToCanvas() {
            // 1. Find or create the panels
            if (_notifPanel == null) {
                // Try finding them first (it might have survived scene load)
                _notifPanel = GameObject.Find(_panelObjectName);
                _notifPopupPanel = GameObject.Find(_popupPanelObjectName);
                if (_notifPanel == null) LoadPanelFromBundle();
                if (_notifPopupPanel == null) LoadPopupPanelFromBundle();
            }

            // 2. Find the Canvas in the new scene
            GameObject canvas = SceneManager.GetActiveScene().GetRootGameObjects()
                .Where(obj => obj.name == "Canvas").FirstOrDefault();
            if (canvas != null && _notifPanel != null && _notifPopupPanel != null) {
                Transform t = _notifPanel.transform;
                Transform tPop = _notifPopupPanel.transform;
                // 1. Parentize
                t.SetParent(canvas.transform, false);
                tPop.SetParent(canvas.transform, false);
                // 2. Bring to front
                t.SetAsLastSibling();
                tPop.SetAsLastSibling();
            }
        }

        public static void TogglePanel() {
            // NotiffyPlugin.Log.LogInfo($"Notif panel {_notifPanel} name {_notifPanel?.name}");
            _notifPanel.SetActive(!_notifPanel.activeSelf);
            _notifPopupPanel.SetActive(!_notifPanel.activeSelf);
            if (_notifPanel.activeSelf) NotificationSystem.Server.ClearNotifications(false);
        }
    }
}
