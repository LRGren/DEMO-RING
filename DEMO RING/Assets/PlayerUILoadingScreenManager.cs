using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerUILoadingScreenManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    void Start()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        DeactivateLoadingScreen();
    }

    public void ActivateLoadingScreen()
    {
        if (loadingScreen.activeSelf)
            return; // If the loading screen is already active, do nothing

        loadingScreen.SetActive(true);
        canvasGroup.alpha = 1f; // Set the alpha to 1 to make it fully visible
    }

    public void DeactivateLoadingScreen(float delay = 1f)
    {
        if (!loadingScreen.activeSelf)
            return; // If the loading screen is already inactive, do nothing

        if (fadeCoroutine != null)
            return; // If a fade coroutine is already running, do nothing

        fadeCoroutine = StartCoroutine(FadeOutLoadingScreen(1, delay));
    }

    private IEnumerator FadeOutLoadingScreen(float duration, float delay)
    {
        while (WorldAIManager.instance.isPerformingLoadingScreenSpawn)
        {
            yield return null; // Wait until the loading screen spawn is complete
        }

        loadingScreen.SetActive(true); // Ensure the loading screen is active before starting the fade

        if (duration > 0f)
        {
            yield return new WaitForSeconds(delay); // Wait for the specified delay before starting the fade

            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
                yield return null;
            }
        }
        canvasGroup.alpha = 0f; // Ensure alpha is set to 0 at the end
        loadingScreen.SetActive(false); // Deactivate the loading screen
        fadeCoroutine = null; // Reset the coroutine reference
    }
}
