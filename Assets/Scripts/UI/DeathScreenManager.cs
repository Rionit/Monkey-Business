using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using MonkeyBusiness.Managers;

namespace MonkeyBusiness.UI
{
    public class DeathScreenManager : MonoBehaviour
    {
        [SerializeField]
        string MainMenuSceneName = "MainMenu";


        void OnEnable()
        {
            
        }

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