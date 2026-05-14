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
    internal static class NotificationController {
        private static readonly string BundlePath = Path.Combine(NotiffyPlugin.workingDir, "assets", "notiffy_ui");
        private static GameObject? _notifPanel;
        private static GameObject? _notifPopupPanel;
        private const string PanelObjectName = "NotiffyPanel";
        private const string PopupPanelObjectName = "NotiffyPopupPanel";
        private static readonly NotificationServer Server = NotificationSystem.Server;
        internal static readonly AssetBundle Bundle;

        private enum NotificationArea {
            Popup,
            Panel,
            Both
        };

        private static readonly Dictionary<uint, GameObject> NotifObjectDict =
            new Dictionary<uint, GameObject>();

        private static readonly Dictionary<uint, GameObject> PopupNotifObjectDict =
            new Dictionary<uint, GameObject>();

        private static Sprite _largeBorder = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_BorderLarge.png").WaitForCompletion();

        private static Sprite _largeFill = Addressables
            .LoadAssetAsync<Sprite>("Assets/Textures/UI/Controls/Round_FillLarge.png").WaitForCompletion();

        static NotificationController() {
            if (Bundle != null) return;
            Bundle = AssetBundle.LoadFromFile(BundlePath);
            if (Bundle == null) NotiffyPlugin.Log.LogError("Could not find asset bundle!");

            SceneManager.sceneLoaded += OnSceneLoaded;

            Server.OnNotificationAdded += OnNotificationAdded;
            Server.OnNotificationUpdated += OnNotificationUpdated;
            Server.OnNotificationClosed += OnNotificationClosed;
            Server.OnNotificationDeleted += OnNotificationDeleted;
        }

        public static void Initialize() {
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            ParentStuffToCanvas();
            UpdateSilentButtonFill();
            // We hook this here because the first scene is loaded after things are already initialized
            NotificationSystem.SignalReadyForScene();
        }

        private static void OnNotificationAdded(uint id, Notification notif) {
            OnNotificationAdded(id, notif, NotificationArea.Both);
        }

        private static void OnNotificationAdded(uint id, Notification notif, NotificationArea area) {
            // NotiffyPlugin.Log.LogInfo($"Created notification {id} with title {notif.Data.Summary}");
            // Find notif panel content
            Transform contentTransform, popupContentTransform;
            if (_notifPanel != null && area != NotificationArea.Popup) {
                contentTransform = _notifPanel.transform.Find("Scroll View/Viewport/NotiffyPanelContent");
                GameObject? newNotif = CreateNotificationGameObject(notif, id, false);
                if (newNotif != null) {
                    newNotif.transform.SetParent(contentTransform, false);
                    NotifObjectDict.Add(id, newNotif);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)contentTransform);
            }

            if (!ConfigManager.Silent.value && _notifPopupPanel != null && _notifPopupPanel.activeSelf &&
                area != NotificationArea.Panel) {
                popupContentTransform = _notifPopupPanel.transform;
                GameObject? newPopupNotif = CreateNotificationGameObject(notif, id, true);
                if (newPopupNotif != null) {
                    newPopupNotif.transform.SetParent(popupContentTransform, false);
                    PopupNotifObjectDict.Add(id, newPopupNotif);
                }

                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)popupContentTransform);
                UpdatePopupTail();
            }

            UpdatePanelEmptyPlaceholder();
        }

        private static void OnNotificationUpdated(uint id, Notification notif) {
            if (NotifObjectDict.TryGetValue(id, out var notifObject)) {
                UpdateNotificationGameObjectContent(notifObject, notif);
            }

            if (PopupNotifObjectDict.TryGetValue(id, out var popupNotifObject)) {
                UpdateNotificationGameObjectContent(popupNotifObject, notif);
            }

            UpdatePanelEmptyPlaceholder();
        }

        private static void OnNotificationClosed(uint id, ClosedReason reason = ClosedReason.Expired) {
            if (PopupNotifObjectDict.TryGetValue(id, out var popupNotifObject)) {
                Object.Destroy(popupNotifObject);
                PopupNotifObjectDict.Remove(id);
                UpdatePopupTail();
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

            UpdatePanelEmptyPlaceholder();
        }

        public static void UpdateKeybindText() {
            UpdatePanelTitle();
            UpdatePopupTail();
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
            GameObject notifPrefab = Bundle.LoadAsset<GameObject>("NotiffyNotification");
            // NotiffyPlugin.Log.LogInfo($"Prefab asset: {notifPrefab}");

            // Make the game object
            GameObject newNotifObj = Object.Instantiate(notifPrefab);
            Button closeButton = newNotifObj.transform.Find("CloseButton").GetComponent<Button>();
            closeButton.onClick.AddListener(isPopup
                ? () => { NotificationSystem.Server.CloseNotification(id, ClosedReason.Dismissed); }
                : () => { NotificationSystem.Server.DeleteNotification(id); });
            UpdateNotificationGameObjectContent(newNotifObj, notif);

            Transform actionLayout = newNotifObj.transform.Find("NotificationTextLayout/ActionLayout");
            Transform textLayout = newNotifObj.transform.Find("NotificationTextLayout");
            if (notif.Actions?.Count > 0) {
                GameObject actionButtonPrefab = Bundle.LoadAsset<GameObject>("NotiffyActionButton");
                for (int i = 0; i + 1 < notif.Actions.Count; i += 2) {
                    string identifier = notif.Actions[i];
                    string displayText = notif.Actions[i + 1];
                    GameObject actionButton = Object.Instantiate(actionButtonPrefab);
                    actionButton.transform.SetParent(actionLayout.transform, false);
                    actionButton.GetComponentInChildren<TextMeshProUGUI>().text = displayText;
                    actionButton.GetComponent<Button>().onClick.AddListener(() => {
                        NotificationSystem.Server.InvokeAction(id, identifier);
                    });
                }
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(actionLayout.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(textLayout.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(newNotifObj.GetComponent<RectTransform>());

            newNotifObj.SetActive(true);
            return newNotifObj;
        }

        private static void SyncNotificationObjects() {
            NotifObjectDict.Clear();
            IReadOnlyList<NotificationEntry> hist = NotificationSystem.Server.History;
            for (int i = 0; i < hist.Count; i++) {
                OnNotificationAdded(hist[i].Id, hist[i].Data, NotificationArea.Panel);
            }
        }

        private static void SyncPopupNotificationObjects() {
            PopupNotifObjectDict.Clear();
            foreach (var n in NotificationSystem.Server.ActiveNotifications) {
                OnNotificationAdded(n.Id, n.Data, NotificationArea.Popup);
            }
        }

        private static void UpdateSilentButtonFill() {
            if (_notifPanel == null) return;
            bool silent = ConfigManager.Silent.value;
            Image btnBg = _notifPanel.transform.Find("Header/NotiffyHeaderSilent").GetComponent<Image>();
            Image icon = _notifPanel.transform.Find("Header/NotiffyHeaderSilent/Image").GetComponent<Image>();
            btnBg.sprite = silent ? _largeFill : _largeBorder;
            icon.color = silent ? Color.black : Color.white;
        }

        public static void UpdatePanelTitle() {
            if (_notifPanel == null) return;
            string newString = $"NOTIFICATIONS<size=60%> [{ConfigManager.GetFormattedPanelKeybind()}]";
            TextMeshProUGUI headerText = _notifPanel.transform.Find("Header/NotiffyHeaderText")
                .GetComponent<TextMeshProUGUI>();
            headerText.text = newString;
        }

        public static void UpdatePanelEmptyPlaceholder() {
            if (_notifPanel is null) return;
            GameObject placeholder = _notifPanel.transform.Find("Scroll View/EmptyPlaceholder").gameObject;
            placeholder.SetActive(NotifObjectDict.Count == 0);
        }

        private static void UpdatePopupTailText() {
            if (_notifPopupPanel is null) return;
            bool hasPopupNotif = PopupNotifObjectDict.Count > 0;
            string showWhat = hasPopupNotif ? "all notifications" : "notifications";
            string newString = $"Show {showWhat}<size=80%> [{ConfigManager.GetFormattedPanelKeybind()}]";
            TextMeshProUGUI popupTailBtnText = _notifPopupPanel.transform
                .Find("ShowAllRow/ShowAllButton/ShowAllButtonText")
                .GetComponent<TextMeshProUGUI>();
            popupTailBtnText.text = newString;
        }

        public static void UpdatePopupTail() {
            if (_notifPopupPanel is null) return;
            UpdatePopupTailText();
            GameObject showAllRow = _notifPopupPanel.transform.Find("ShowAllRow").gameObject;
            bool countCondition = PopupNotifObjectDict.Count > 0 && PopupNotifObjectDict.Count < NotifObjectDict.Count;
            bool pauseCondition = ConfigManager.PauseMenuButtonType.value == PauseMenuButtonTypeEnum.Corner &&
                                  (Time.timeScale == 0 && (OptionsManager.Instance?.paused ?? false));
            showAllRow.GetComponent<HorizontalLayoutGroup>().padding.right = PopupNotifObjectDict.Count > 0 ? 6 : 0;
            showAllRow.SetActive(countCondition || pauseCondition);
        }

        private static void LoadPanelFromBundle() {
            // NotiffyPlugin.Log.LogInfo("RE-INSTANTIATING PANEL");

            // Load prefab
            GameObject panelPrefab = Bundle.LoadAsset<GameObject>("NotiffyPanel");

            // Instantiate
            _notifPanel = Object.Instantiate(panelPrefab);
            _notifPanel.SetActive(true);
            _notifPanel.SetActive(false);
            _notifPanel.name = PanelObjectName; // Give it a fixed name to find later
            Object.DontDestroyOnLoad(_notifPanel);

            // Hook buttons
            Button closeButton = _notifPanel.transform.Find("NotiffyPanelClose/CloseButton").GetComponent<Button>();
            closeButton.onClick.AddListener(ClosePanel);
            Button clearButton = _notifPanel.transform.Find("Header/NotiffyHeaderClear").GetComponent<Button>();
            clearButton.onClick.AddListener(() => { NotificationSystem.Server.ClearNotifications(); });
            Button silentButton = _notifPanel.transform.Find("Header/NotiffyHeaderSilent").GetComponent<Button>();
            silentButton.onClick.AddListener(() => {
                ConfigManager.Silent.value = !ConfigManager.Silent.value;
                List<int> ids = [];
                foreach (int id in PopupNotifObjectDict.Keys) ids.Add(id);
                foreach (uint id in ids) OnNotificationClosed(id);
                UpdateSilentButtonFill();
            });
            UpdatePanelTitle();
            UpdatePanelEmptyPlaceholder();
            SyncNotificationObjects();
        }

        private static void LoadPopupPanelFromBundle() {
            // NotiffyPlugin.Log.LogInfo("RE-INSTANTIATING POPUP PANEL");
            GameObject popupPrefab = Bundle.LoadAsset<GameObject>("NotiffyPopupContent");
            _notifPopupPanel = Object.Instantiate(popupPrefab);
            _notifPopupPanel.SetActive(true);
            _notifPopupPanel.name = PopupPanelObjectName;

            Button showAllButton = _notifPopupPanel.transform.Find("ShowAllRow/ShowAllButton").GetComponent<Button>();
            showAllButton.onClick.AddListener(OpenPanel);

            Object.DontDestroyOnLoad(_notifPopupPanel);
            SyncPopupNotificationObjects();
            UpdatePopupTail();
        }

        private static void ParentStuffToCanvas() {
            // Find or create the panels
            if (_notifPanel == null) {
                // Try finding them first (it might have survived scene load)
                _notifPanel = GameObject.Find(PanelObjectName);
                _notifPopupPanel = GameObject.Find(PopupPanelObjectName);
                if (_notifPanel == null) LoadPanelFromBundle();
                if (_notifPopupPanel == null) LoadPopupPanelFromBundle();
            }

            // Find the Canvas in the new scene
            GameObject canvas = SceneManager.GetActiveScene().GetRootGameObjects()
                .Where(obj => obj.name == "Canvas").FirstOrDefault();
            if (canvas != null && _notifPanel != null && _notifPopupPanel != null) {
                Transform t = _notifPanel.transform;
                Transform tPop = _notifPopupPanel.transform;
                // Parentize
                t.SetParent(canvas.transform, false);
                tPop.SetParent(canvas.transform, false);
                // Bring to front
                t.SetAsLastSibling();
                tPop.SetAsLastSibling();
            }
        }

        public static void TogglePanel() {
            if (_notifPanel is null || _notifPopupPanel is null) return;
            _notifPanel.SetActive(!_notifPanel.activeSelf);
            _notifPopupPanel.SetActive(!_notifPanel.activeSelf);
            if (_notifPanel.activeSelf) {
                NotificationSystem.Server.ClearNotifications(false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(_notifPanel.transform
                    .Find("Scroll View/Viewport/NotiffyPanelContent").GetComponent<RectTransform>());
            }
        }

        public static void OpenPanel() {
            if (_notifPanel is null || _notifPopupPanel is null) return;
            if (!_notifPanel.activeSelf) TogglePanel();
        }

        public static void ClosePanel() {
            if (_notifPanel is null || _notifPopupPanel is null) return;
            if (_notifPanel.activeSelf) TogglePanel();
        }
    }
}
