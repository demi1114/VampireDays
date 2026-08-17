using UnityEngine;
using UnityEngine.SceneManagement;

public class UniversalButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName;

    public void TransitionToScene()
    {
        SceneManager.LoadScene(targetSceneName);
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
};