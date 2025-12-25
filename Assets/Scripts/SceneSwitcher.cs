using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string hubScene = "AlanScene";

    public void LoadSceneByName()
    {
        SceneManager.LoadScene(hubScene);
    }
}
