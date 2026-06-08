using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using MonkeyBusiness.Managers;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections.Generic;

namespace MonkeyBusiness.UI
{
    public class ScorePrompt : MonoBehaviour
    {
        [SerializeField]
        GameObject _scoreHeader; 

        [SerializeField]
        RectTransform _scoreTextTransform;

        [SerializeField]
        TMP_Text _scoreText;

        [SerializeField]
        RectTransform _scorePrompt; 

        [SerializeField]
        Button _confirmButton;

        [SerializeField]
        TMP_InputField _promptInput;

        [FoldoutGroup("Tween durations")]
        [SerializeField]
        float _headerAnimationDuration = 1f;

        [FoldoutGroup("Tween durations")]
        [SerializeField]
        float _scoreTextAnimationDuration = 3f;

        [FoldoutGroup("Tween durations")]
        [SerializeField]
        float _promptAnimationDuration = 1f;

        [FoldoutGroup("Tween durations")]
        [SerializeField]
        float _headerScoreDelay = 0.5f;

        [FoldoutGroup("Tween durations")]
        [SerializeField]
        float _scorePromptDelay = 0.5f;

        float _currentShownScore;

        float _CurrentShownScore
        {
            get => _currentShownScore;
            set
            {
                _currentShownScore = value;
                _scoreText.text = Mathf.RoundToInt(_currentShownScore).ToString();
            }
        }

        void OnEnable()
        {
            _currentShownScore = 0;
            _scoreText.text = string.Empty;

            _scoreHeader.transform.localScale = Vector3.zero;
            _scoreTextTransform.localScale = Vector3.zero;
            _scorePrompt.localScale = Vector3.zero;

            Animate();
        }

        [Button("Animate")]
        public void Animate()
        {
            StartCoroutine(AnimationCoroutine());
        }

        public void StoreScore()
        {

            Debug.Log("Storing score for " + _promptInput.text + ": " + GameManager.Score + " ... level = " + GameManager.LevelReached);
            if(GameManager.Scoreboard.ContainsKey(GameManager.Score))
            {
                GameManager.Scoreboard[GameManager.Score].Add(new GameManager.ScoreEntry(_promptInput.text, GameManager.LevelReached));
            }
            else
            {
                GameManager.Scoreboard.Add(
                    GameManager.Score,
                    new List<GameManager.ScoreEntry>()
                    { 
                        new GameManager.ScoreEntry(_promptInput.text, GameManager.LevelReached) 
                    });
            }
        }

        public void Hide()
        {
            _scoreText.text = string.Empty;
            _scoreHeader.transform.localScale = Vector3.zero;
            _scoreTextTransform.localScale = Vector3.zero;
            _scorePrompt.localScale = Vector3.zero;
            _promptInput.text = string.Empty;
            _confirmButton.interactable = false;

        }

        IEnumerator AnimationCoroutine()
        {
            var animSequence = DOTween.Sequence();
            animSequence.SetUpdate(true);
            animSequence.Append(_scoreHeader.transform.DOScale(1, _headerAnimationDuration).From(0).SetEase(Ease.OutCubic));
            animSequence.AppendInterval(_headerScoreDelay);
            animSequence.Append(_scoreTextTransform.DOScale(1, _scoreTextAnimationDuration).From(0).SetEase(Ease.OutBack));
            animSequence.Join(DOTween.To(() => _CurrentShownScore, x => _CurrentShownScore = x, GameManager.Score, 2f).SetEase(Ease.InOutCubic));
            animSequence.AppendInterval(_scorePromptDelay);
            animSequence.Append(_scorePrompt.DOScale(1, _promptAnimationDuration).From(0).SetEase(Ease.OutCubic));
            animSequence.onComplete += () => _confirmButton.interactable = true;
            yield return animSequence.WaitForCompletion();
        }
    }
}
