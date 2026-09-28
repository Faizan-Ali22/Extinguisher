using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Loading screen with async scene loading and optional loading bar support.
/// Shows the loading screen for a minimum duration while the target scene
/// loads in the background, then transitions smoothly.
/// </summary>
public class LoadingScript : MonoBehaviour
{
    [Tooltip("Minimum time to display the loading screen (seconds)")]
    public float loadTime;

    [Tooltip("Build index of the scene to load (default: 1 = Main Menu)")]
    public int targetSceneIndex = 1;

    [Header("Loading Bar (Optional)")]
    [Tooltip("Assign an Image with Fill type to show loading progress")]
    public Image loadingBarImage;

    [Tooltip("Assign a Slider to show loading progress")]
    public Slider loadingBarSlider;

    void Start()
    {
        // Enforce minimum load time — the original code hardcoded 8 seconds.
        // The serialized value in the scene might be 0 (it was overridden in Start()).
        if (loadTime <= 0f)
            loadTime = 8f;

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Begin loading the target scene in the background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneIndex);
        asyncLoad.allowSceneActivation = false;

        float elapsed = 0f;

        while (!asyncLoad.isDone)
        {
            elapsed += Time.deltaTime;

            // Calculate display progress (0 to 1) based on elapsed time
            // This drives the loading bar smoothly regardless of actual load speed
            float displayProgress = Mathf.Clamp01(elapsed / loadTime);

            // Update loading bar visuals
            UpdateLoadingBar(displayProgress);

            // AsyncOperation.progress caps at 0.9 until allowSceneActivation = true.
            // We wait for BOTH the scene to be ready AND the minimum display time.
            bool sceneReady = asyncLoad.progress >= 0.9f;
            bool timeReached = elapsed >= loadTime;

            if (sceneReady && timeReached)
            {
                // Ensure bar shows 100% before transitioning
                UpdateLoadingBar(1f);
                yield return new WaitForSeconds(0.3f); // Brief pause at 100%

                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Updates any assigned loading bar UI element.
    /// Supports both Image (fillAmount) and Slider approaches.
    /// </summary>
    void UpdateLoadingBar(float progress)
    {
        if (loadingBarImage != null)
            loadingBarImage.fillAmount = progress;

        if (loadingBarSlider != null)
            loadingBarSlider.value = progress;
    }
}
