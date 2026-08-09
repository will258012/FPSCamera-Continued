using ColossalFramework;
using FPSCamera.Settings;
using UnityEngine;

namespace FPSCamera.Utils;

/// <summary>
/// Coordinates opacity animations for a single UI target.
/// </summary>
/// <remarks>
/// Use one stable, unique <see cref="FadeID"/> per target and implement <see cref="Opacity"/>
/// against that target. Prefer <see cref="FadeIn"/> and <see cref="FadeOut"/> for normal visibility
/// changes so completion handlers always receive an explicit direction.
/// </remarks>
public abstract class FadeHelper
{
    public enum FadeType
    {
        None,
        In,
        Out,
    }

    /// <summary>
    /// Gets the unique animation identifier used by <see cref="ValueAnimator"/>.
    /// </summary>
    public abstract string FadeID { get; }

    /// <summary>
    /// Gets or sets the target opacity, normally in the range 0-1.
    /// </summary>
    public abstract float Opacity { get; set; }

    /// <summary>
    /// Gets the requested fade direction until completion handlers have run.
    /// </summary>
    public FadeType Status { get; private set; }

    /// <summary>
    /// Gets whether this helper currently owns a running animation.
    /// </summary>
    public bool IsFading => (Status is FadeType.In or FadeType.Out) && ValueAnimator.IsAnimating(FadeID);

    public delegate void FadeCompletedEventHandler(FadeType fadeType);

    /// <summary>
    /// Raised after the final opacity is applied.
    /// </summary>
    /// <remarks>
    /// This event is raised synchronously when fading is disabled or the target opacity is already
    /// reached. During the callback, <see cref="Status"/> still contains the completed direction.
    /// </remarks>
    public event FadeCompletedEventHandler OnFadeCompleted;

    private int fadeAnimationVersion;
    private int? completingAnimationVersion;

    /// <summary>
    /// Show the panel: fade in.
    /// </summary>
    public void FadeIn() => FadeTo(1f, FadeType.In);

    /// <summary>
    /// Hide the panel: fade out.
    /// </summary>
    public void FadeOut() => FadeTo(0f, FadeType.Out);

    /// <summary>
    /// Resets the opacity and state, and cancels any running animation.
    /// </summary>
    /// <remarks>
    /// Call this before the owning UI target is disabled or destroyed.
    /// </remarks>
    public void Reset()
    {
        Opacity = 0f;
        Status = FadeType.None;
        // ValueAnimator removes a completed entry after invoking its callback; do not remove it twice.
        if (completingAnimationVersion != fadeAnimationVersion)
        {
            fadeAnimationVersion++;
            ValueAnimator.Cancel(FadeID);
        }
    }

    /// <summary>
    /// Fades to the specified opacity, optionally using an explicit completion direction.
    /// </summary>
    /// <param name="targetOpacity">Final opacity, normally in the range 0-1.</param>
    /// <param name="fadeType">
    /// Direction reported to completion handlers. When omitted, it is inferred if the opacity changes;
    /// otherwise <see cref="FadeType.None"/> is reported.
    /// </param>
    /// <remarks>
    /// A new request replaces any running animation with the same <see cref="FadeID"/>. Do not call
    /// this every frame; check <see cref="IsFading"/> first unless interruption or reversal is intended.
    /// </remarks>
    public void FadeTo(float targetOpacity, FadeType fadeType = FadeType.None)
    {
        if (fadeType == FadeType.None && !Mathf.Approximately(Opacity, targetOpacity))
            fadeType = (Opacity - targetOpacity) > 0f ? FadeType.Out : FadeType.In;

        Status = fadeType;

        if (!ModSettings.Fade || Mathf.Approximately(Opacity, targetOpacity))
        {
            Complete(targetOpacity);
            return;
        }

        var animationVersion = ++fadeAnimationVersion;

        // Named animations are replaced in place, so cancelling first would mutate the active update list.
        ValueAnimator.Animate(FadeID,
            value =>
            {
                if (animationVersion == fadeAnimationVersion)
                    Opacity = value;
            },
            new AnimatedFloat(Opacity, targetOpacity, .2f, EasingType.SineEaseOut),
            () =>
            {
                if (animationVersion == fadeAnimationVersion)
                {
                    var previousCompletingVersion = completingAnimationVersion;
                    completingAnimationVersion = animationVersion;
                    try
                    {
                        Complete(targetOpacity);
                    }
                    finally
                    {
                        completingAnimationVersion = previousCompletingVersion;
                    }
                }
            });
    }

    private void Complete(float targetOpacity)
    {
        Opacity = targetOpacity;
        var status = Status;
        var animationVersion = fadeAnimationVersion;
        try
        {
            OnFadeCompleted?.Invoke(status);
        }
        finally
        {
            if (animationVersion == fadeAnimationVersion)
                Status = FadeType.None;
        }
    }

}
