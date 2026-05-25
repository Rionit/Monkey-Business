using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using MonkeyBusiness.Managers;

namespace MonkeyBusiness.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        string ArenaSceneName = "VerticalSlice";

        [SerializeField]
        TMP_Text _highScoreText;

        void Awake()
        {
            Time.timeScale = 1f;
            GameManager.HighScore = PlayerPrefs.GetInt("HighScore", 0);
            _highScoreText.text = $"High Score: {GameManager.HighScore}";
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
