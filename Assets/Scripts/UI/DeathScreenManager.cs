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

        [SerializeField]
        TMP_Text _score;

        [SerializeField]
        TMP_Text _highScore;

        [SerializeField]
        GameObject _newHighScore;


        void OnEnable()
        {
            _score.text = GameManager.Score.ToString();
            _highScore.text = GameManager.HighScore.ToString();
            _newHighScore.SetActive(GameManager.Score > GameManager.HighScore);

            if(GameManager.Score > GameManager.HighScore)
            {
                GameManager.HighScore = GameManager.Score;
            }
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