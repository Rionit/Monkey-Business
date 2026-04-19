using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeyBusiness.UI
{
    public class MainMenuManager : MonoBehaviour
    {

        [SerializeField]
        string ArenaSceneName = "VerticalSlice";

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
