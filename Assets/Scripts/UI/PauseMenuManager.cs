using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeyBusiness.UI
{
    public class PauseMenuManager : MonoBehaviour
    {
        [SerializeField]
        string MainMenuSceneName = "MainMenu";
        public static PauseMenuManager Instance { get; private set; }

        [SerializeField]
        GameObject _controlsMenu;

        void OnEnable()
        {
            _controlsMenu.SetActive(false);
        }

        void Awake()
        {
           if(Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of PauseMenuManager detected! Replacing the old one.");
            }
            Instance = this;
        }

        public void GotoMainMenu()
        {
            SceneManager.LoadScene(MainMenuSceneName);
        }
    }
}
