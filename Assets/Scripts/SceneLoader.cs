using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    private static bool isLoading = false;

    public static void LoadGameScenes(int scene1Index, int scene2Index)
    {
        if (isLoading) return;
        CreateLoaderAndLoad(scene1Index, scene2Index);
    }

    public static void LoadGameScenes(string scene1Name, string scene2Name)
    {
        if (isLoading) return;
        CreateLoaderAndLoad(scene1Name, scene2Name);
    }

    public static void LoadSingleScene(int sceneIndex)
    {
        if (isLoading) return;
        SceneManager.LoadScene(sceneIndex);
    }

    public static void LoadSingleScene(string sceneName)
    {
        if (isLoading) return;
        SceneManager.LoadScene(sceneName);
    }

    private static void CreateLoaderAndLoad(int scene1Index, int scene2Index)
    {
        GameObject loader = new GameObject("SceneLoader");
        DontDestroyOnLoad(loader);
        loader.AddComponent<SceneLoader>().StartLoadingByIndex(scene1Index, scene2Index);
    }

    private static void CreateLoaderAndLoad(string scene1Name, string scene2Name)
    {
        GameObject loader = new GameObject("SceneLoader");
        DontDestroyOnLoad(loader);
        loader.AddComponent<SceneLoader>().StartLoadingByName(scene1Name, scene2Name);
    }

    private void StartLoadingByIndex(int scene1Index, int scene2Index)
    {
        StartCoroutine(LoadScenesCoroutine(scene1Index, scene2Index));
    }

    private void StartLoadingByName(string scene1Name, string scene2Name)
    {
        StartCoroutine(LoadScenesCoroutine(scene1Name, scene2Name));
    }

    private IEnumerator LoadScenesCoroutine(int scene1Index, int scene2Index)
    {
        isLoading = true;

        yield return SceneManager.LoadSceneAsync(scene1Index, LoadSceneMode.Single);

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.buildIndex == scene2Index && scene.isLoaded)
            {
                isLoading = false;
                Destroy(gameObject);
                yield break;
            }
        }

        yield return SceneManager.LoadSceneAsync(scene2Index, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(scene1Index));

        isLoading = false;
        Destroy(gameObject);
    }

    private IEnumerator LoadScenesCoroutine(string scene1Name, string scene2Name)
    {
        isLoading = true;

        yield return SceneManager.LoadSceneAsync(scene1Name, LoadSceneMode.Single);

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == scene2Name && scene.isLoaded)
            {
                isLoading = false;
                Destroy(gameObject);
                yield break;
            }
        }

        yield return SceneManager.LoadSceneAsync(scene2Name, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene1Name));

        isLoading = false;
        Destroy(gameObject);
    }
}