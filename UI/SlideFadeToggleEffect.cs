using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Notiffy.UI;

public class SlideFadeToggleEffect : MonoBehaviour {
    private enum State {
        Idle,
        Entering,
        Exiting
    }

    private State currentState = State.Idle;

    private RectTransform rect;
    private CanvasGroup? canvasGroup;

    private float originalAlpha;
    private Vector2 originalPos;

    /// <summary>
    /// The alpha value when completely hidden
    /// </summary>
    public float hiddenAlpha = 0;

    /// <summary>
    /// The speed of the animation
    /// </summary>
    public float speed = 18f;

    /// <summary>
    /// The offset when the object is hidden
    /// </summary>
    public Vector2 hiddenOffset = Vector2.zero;

    /// <summary>
    /// Emitted when the exiting animation is finished. You can hide/destroy the element after this.
    /// </summary>
    public event Action OnExitComplete;

    private Vector2 hiddenPos => originalPos + hiddenOffset;

    private void Initialize() {
        if ((Object)(object)rect == (Object)null) rect = ((Component)this).GetComponent<RectTransform>();
        if ((Object?)(object?)canvasGroup == (Object?)null) canvasGroup = ((Component)this).GetComponent<CanvasGroup>();

        originalAlpha = canvasGroup?.alpha ?? 1;
        originalPos = rect.anchoredPosition;
    }

    private void OnEnable() {
        rect.anchoredPosition = hiddenPos;
        if (canvasGroup is not null) canvasGroup.alpha = hiddenAlpha;
        currentState = State.Entering;
    }

    /// <summary>
    /// Starts exiting animation
    /// </summary>
    public void StartExit() {
        currentState = State.Exiting;
    }

    private void Awake() {
        Initialize();
    }

    private float currentExitSpeed = 0f;

    private void Update() {
        if (currentState == State.Idle) return;

        bool entering = currentState == State.Entering;
        Vector2 targetPos = entering ? originalPos : hiddenPos;
        float targetAlpha = entering ? originalAlpha : hiddenAlpha;

        Vector2 currentPos = rect.anchoredPosition;
        float currentAlpha = canvasGroup?.alpha ?? 1;

        float moveStep;
        float alphaStep;

        if (entering) {
            // Decelerating
            float speedMultiplier = Time.unscaledDeltaTime * speed;
            moveStep = (Vector2.Distance(currentPos, targetPos) + 0.1f) * speedMultiplier;
            alphaStep = (Mathf.Abs(targetAlpha - currentAlpha) + 0.1f) * speedMultiplier;
            currentExitSpeed = 0f; // Reset exit speed
        } else {
            // Accelerating
            currentExitSpeed += speed * 7f * Time.unscaledDeltaTime;
            moveStep = currentExitSpeed;
            alphaStep = currentExitSpeed * 0.01f;
        }

        // Apply movement
        rect.anchoredPosition = Vector2.MoveTowards(currentPos, targetPos, moveStep);
        if (canvasGroup is not null) {
            canvasGroup.alpha = Mathf.MoveTowards(currentAlpha, targetAlpha, alphaStep);
        }

        // Check Completion
        if (rect.anchoredPosition == targetPos &&
            (canvasGroup is null || Mathf.Approximately(canvasGroup.alpha, targetAlpha))) {
            State finishedState = currentState;
            currentState = State.Idle;
            currentExitSpeed = 0f;
            if (finishedState == State.Exiting) OnExitComplete?.Invoke();
        }
    }
}
