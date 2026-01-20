using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader instance;

    public static void LoadGameScenes(int scene1Index, int scene2Index)
    {
        CreateLoaderAndLoad(scene1Index, scene2Index);
    }

    public static void LoadGameScenes(string scene1Name, string scene2Name)
    {
        CreateLoaderAndLoad(scene1Name, scene2Name);
    }

    public static void LoadSingleScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public static void LoadSingleScene(string sceneName)
    {
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
        yield return SceneManager.LoadSceneAsync(scene1Index, LoadSceneMode.Single);
        yield return SceneManager.LoadSceneAsync(scene2Index, LoadSceneMode.Additive);

        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(scene1Index));
        Destroy(gameObject);
    }

    private IEnumerator LoadScenesCoroutine(string scene1Name, string scene2Name)
    {
        yield return SceneManager.LoadSceneAsync(scene1Name, LoadSceneMode.Single);
        yield return SceneManager.LoadSceneAsync(scene2Name, LoadSceneMode.Additive);

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene1Name));
        Destroy(gameObject);
    }
}