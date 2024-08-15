using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public enum Scene
    {
        None,
        MainMenuScene,
        LoadingScene,
        GameScene,
        LobbyScene,
        CharacterSelectScene
    }

    public static Scene targetScene = Scene.None;
    private static Action<Scene> lastLoadFunction;
    public static event Action<Scene> OnSceneLoaded;

    static SceneLoader()
    {
        targetScene = Scene.MainMenuScene;
        lastLoadFunction = LoadLocally;
    }

    static void LoadNetwork(Scene scene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    static void LoadLocally(Scene scene)
    {
        SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
    }

    public static void Load(Scene scene, bool useLoaderScene, bool isNetwork)
    {
        Action<Scene> loadFunction = isNetwork ? LoadNetwork : LoadLocally;

        if (useLoaderScene)
        {
            targetScene = scene;
            lastLoadFunction = loadFunction;
            scene = Scene.LoadingScene;
        }

        loadFunction(scene);

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private static void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, LoadSceneMode arg1)
    {
        OnSceneLoaded?.Invoke(StringNameToEnum(arg0.name));
    }

    private static Scene StringNameToEnum(string name)
    {
        var values = Enum.GetNames(typeof(Scene));
        var index = Array.IndexOf(values, name);
        return (Scene)index;
    }

    public static void LoaderCallback()
    {
        if (lastLoadFunction == null)
            return;

        lastLoadFunction(targetScene);
        OnSceneLoaded?.Invoke(targetScene);
        targetScene = Scene.None;
        lastLoadFunction = null;
    }
}
