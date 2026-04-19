using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeyBusiness.UI
{
    public class MainMenuManager : MonoBehaviour
    {

        [SerializeField]
        string ArenaSceneName = "VerticalSlice";

        void Awake()
        {
            Time.timeScale = 1f;
        }

        public void StartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(ArenaSceneName);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

    }
}
