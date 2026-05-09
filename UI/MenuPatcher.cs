using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System.ComponentModel;

namespace Notiffy.UI;

internal static class MenuPatcher {
    private static string PauseNotifToggleButtonName = "ToggleNotificationPanel";

    private static GameObject? FindNestedObject(GameObject baseObject, string path, bool warnings = true) {
        Transform t = baseObject.transform;
        string[] pathItems = path.Split("/");
        for (int i = 0; i < pathItems.Length; i++) {
            string itemStr = pathItems[i];
            t = t.transform.Find(itemStr);
            if (t == null) {
                if (warnings) NotiffyPlugin.Log.LogWarning(itemStr + " not found for object path " + baseObject.name + "/" + path);
                return null;
            }
        }
        return t.gameObject;
    }

    public static void Initialize() {}
    static MenuPatcher() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        PatchPauseMenu();
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
        GameObject notifButton = Object.Instantiate(resumeButton);
        notifButton.transform.SetParent(pauseMenu.transform, false);
        notifButton.name = PauseNotifToggleButtonName;
        // Set click behavior
        Button buttonComponent = notifButton.GetComponent<Button>();
        buttonComponent.onClick = new Button.ButtonClickedEvent(); // nuke old behavior
        buttonComponent.onClick.AddListener(NotificationController.TogglePanel);
        // Sizing & positioning
        RectTransform rt = notifButton.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(rt.sizeDelta.y, rt.sizeDelta.y); // Square
        rt.localPosition = new Vector3(170, 90, 0);
        // Add icon
        GameObject? childText = FindNestedObject(notifButton, "Text");
        if (childText != null) Object.Destroy(childText);
        GameObject btnIconPrefab = NotificationController.Bundle.LoadAsset<GameObject>("NotiffyToggleIcon");
        GameObject btnIcon = Object.Instantiate(btnIconPrefab);
        btnIcon.transform.SetParent(notifButton.transform, false);

        // Ensure the icon is centered and scaled correctly within the button
        RectTransform iconRt = btnIcon.GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0.33f, 0.33f);
        iconRt.anchorMax = new Vector2(0.67f, 0.67f);

        btnIcon.SetActive(true);
        notifButton.SetActive(true);
    }

}
