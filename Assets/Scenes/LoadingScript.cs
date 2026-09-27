using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Async loading screen. Loads the target scene in the background
/// and activates it once ready and the minimum display time has passed.
/// </summary>
public class LoadingScript : MonoBehaviour
{
    [Tooltip("Minimum time to show the loading screen (seconds)")]
    public float loadTime = 3f;

    [Tooltip("Build index of the scene to load (default: 1 = Main Menu)")]
    public int targetSceneIndex = 1;

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Begin loading in background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneIndex);
        asyncLoad.allowSceneActivation = false;

        float elapsed = 0f;

        // Wait for both: scene loaded AND minimum display time
        while (!asyncLoad.isDone)
        {
            elapsed += Time.deltaTime;

            // AsyncOperation.progress stops at 0.9 until allowSceneActivation = true
            if (asyncLoad.progress >= 0.9f && elapsed >= loadTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
