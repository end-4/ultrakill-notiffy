using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.ComponentModel;

namespace Notiffy.UI;

internal static class MenuPatcher {
    private static string PauseNotifToggleButtonName = "ToggleNotificationPanel";
    private static GameObject _notifButton;

    private static GameObject? FindNestedObject(GameObject baseObject, string path, bool warnings = true) {
        Transform t = baseObject.transform;
        string[] pathItems = path.Split("/");
        for (int i = 0; i < pathItems.Length; i++) {
            string itemStr = pathItems[i];
            t = t.transform.Find(itemStr);
            if (t == null) {
                if (warnings)
                    NotiffyPlugin.Log.LogWarning(itemStr + " not found for object path " + baseObject.name + "/" +
                                                 path);
                return null;
            }
        }

        return t.gameObject;
    }

    public static void Initialize() {
    }

    static MenuPatcher() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        PatchPauseMenu();
    }

    public static void UpdateAppearance() {
        // Positioning
        RectTransform rt = _notifButton.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.y, rt.sizeDelta.y); // Square
        bool alignWithConfiggy = ConfigManager.PauseMenuButtonType.value == PauseMenuButtonTypeEnum.MenuConfiggyAligned;
        rt.localPosition = new Vector3(alignWithConfiggy ? 107 : 170, 90, 0);
        // Show?
        _notifButton.SetActive(ConfigManager.PauseMenuButtonType.value == PauseMenuButtonTypeEnum.MenuRight ||
                               ConfigManager.PauseMenuButtonType.value == PauseMenuButtonTypeEnum.MenuConfiggyAligned);
    }

    private static void PatchPauseMenu() {
        // Find og
        GameObject canvas = SceneManager.GetActiveScene().GetRootGameObjects()
            .Where(obj => obj.name == "Canvas").FirstOrDefault();
        if (FindNestedObject(canvas, $"PauseMenu/{PauseNotifToggleButtonName}", warnings: false)) return;
        GameObject? pauseMenu = FindNestedObject(canvas, "PauseMenu");
        GameObject? resumeButton = FindNestedObject(canvas, "PauseMenu/Resume");
        if (pauseMenu == null || resumeButton == null) return;
        // Clone
        _notifButton = Object.Instantiate(resumeButton);
        _notifButton.transform.SetParent(pauseMenu.transform, false);
        _notifButton.name = PauseNotifToggleButtonName;
        // Set click behavior
        Button buttonComponent = _notifButton.GetComponent<Button>();
        buttonComponent.onClick = new Button.ButtonClickedEvent(); // nuke old behavior
        buttonComponent.onClick.AddListener(NotificationController.TogglePanel);
        // Add icon
        GameObject? childText = FindNestedObject(_notifButton, "Text");
        if (childText != null) Object.Destroy(childText);
        GameObject btnIconPrefab = NotificationController.Bundle.LoadAsset<GameObject>("NotiffyToggleIcon");
        GameObject btnIcon = Object.Instantiate(btnIconPrefab);
        btnIcon.transform.SetParent(_notifButton.transform, false);

        // Ensure the icon is centered and scaled correctly within the button
        RectTransform iconRt = btnIcon.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.33f, 0.33f);
        iconRt.anchorMax = new Vector2(0.67f, 0.67f);

        btnIcon.SetActive(true);
        UpdateAppearance();
    }
}
