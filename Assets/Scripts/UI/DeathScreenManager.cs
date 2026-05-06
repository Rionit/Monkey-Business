using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeyBusiness.UI
{
    public class DeathScreenManager : MonoBehaviour
    {
        [SerializeField]
        string MainMenuSceneName = "MainMenu";

        public void PlayAgain()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }
}