using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    private static SceneChanger _instance;

    public static SceneChanger Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SceneChanger");
                _instance = go.AddComponent<SceneChanger>();
            }
            return _instance;
        }
    }

    [Header("Options")]
    [SerializeField] private float _fadeTime = 0.35f;
    [SerializeField] private string _emptySceneName = "EmptyScene"; // opcional: escena vacía intermedia

    private CanvasGroup _fade;
    private bool _loading;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        BuildFade();
    }

    // Crea un Canvas negro por código, arriba de todo
    private void BuildFade()
    {
        var canvasGo = new GameObject("FadeCanvas");
        canvasGo.transform.SetParent(transform, false);

        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        _fade = canvasGo.AddComponent<CanvasGroup>();
        _fade.alpha = 0f;
        _fade.blocksRaycasts = false;
        _fade.interactable = false;

        var imgGo = new GameObject("Black");
        imgGo.transform.SetParent(canvasGo.transform, false);

        var img = imgGo.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;

        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // ---------- API pública ----------

    public void GoTo(string sceneName, UnityEvent midAction = null)
    {
        if (_loading) return;
        StartCoroutine(LoadRoutine(sceneName, -1, midAction));
    }

    public void GoToIndex(int buildIndex, UnityEvent midAction = null)
    {
        if (_loading) return;
        StartCoroutine(LoadRoutine(null, buildIndex, midAction));
    }

    public void GoToNext()
    {
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            GoToIndex(next);
    }

    public void Reload() => GoToIndex(SceneManager.GetActiveScene().buildIndex);

    // ---------- Lógica ----------
    private IEnumerator LoadRoutine(string sceneName, int buildIndex, UnityEvent midAction = null)
    {
        _loading = true;
        _fade.blocksRaycasts = true; // bloquea clicks durante la transición

        yield return Fade(0f, 1f);
        midAction?.Invoke();

        // Escena vacía intermedia: libera la memoria de la actual antes de cargar la nueva
        if (!string.IsNullOrEmpty(_emptySceneName))
        {
            yield return SceneManager.LoadSceneAsync(_emptySceneName);
            yield return Resources.UnloadUnusedAssets();
        }

        AsyncOperation op = buildIndex >= 0
            ? SceneManager.LoadSceneAsync(buildIndex)   // modo Single
            : SceneManager.LoadSceneAsync(sceneName);

        yield return op;
        yield return Resources.UnloadUnusedAssets();
        GC.Collect();

        yield return Fade(1f, 0f);

        _fade.blocksRaycasts = false;
        _loading = false;
    }

    private IEnumerator Fade(float from, float to)
    {
        if (_fadeTime <= 0f)
        {
            _fade.alpha = to;
            yield break;
        }

        for (float t = 0f; t < _fadeTime; t += Time.unscaledDeltaTime)
        {
            _fade.alpha = Mathf.Lerp(from, to, t / _fadeTime);
            yield return null;
        }
        _fade.alpha = to;
    }
}