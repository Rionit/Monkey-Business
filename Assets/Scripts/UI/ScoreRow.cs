using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace MonkeyBusiness.UI
{
    public class ScoreRow : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _numberText, _nameText, _scoreText, _levelText;

        public void SetData(int number, string name, int score, int level)
        {
            _numberText.text = number.ToString();
            _nameText.text = name;
            _scoreText.text = score.ToString();
            _levelText.text = level.ToString();
        }

        public void SetColor(Color color)
        {
            _numberText.color = color;
            _nameText.color = color;
            _scoreText.color = color;
            _levelText.color = color;
        }

        void OnEnable()
        {
            //transform.localScale = Vector3.zero;
        }

        [Button("Animate")]
        public void Animate()
        {
            transform.DOScale(Vector3.one, 1f).From(Vector3.zero).SetEase(Ease.OutBack).SetUpdate(true);
            //StartCoroutine(AnimateCoroutine());
        }

        /*IEnumerator AnimateCoroutine()
        {



        }*/
    }
}
