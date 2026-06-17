using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace MonkeyBusiness.UI
{
    public class HealthAmmoEffectAnimator : MonoBehaviour
    {
        [SerializeField]
        float _flyHeightBase = 800f;

        [SerializeField]
        Image _image;

        [SerializeField]
        float _scaleRange = 0.2f;

        [SerializeField]
        Vector3 _baseScale = Vector3.one;

        Sequence _sequence;

        Vector2 _originalPosition;

        float _actualScale;

        float _randomDelay;

        [SerializeField]
        float _maxDelayRange = 0.2f;

        float _randomHeightOffset = 200;

        float _actualHeight;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            _originalPosition = (transform as RectTransform).anchoredPosition;

            var randomRangeFloat = Random.Range(0f, 1f);
            _actualScale = 1f + Random.Range(-_scaleRange, _scaleRange);
            _randomDelay = _maxDelayRange * randomRangeFloat * randomRangeFloat;
            _actualHeight = _flyHeightBase + Random.Range(-_randomHeightOffset, _randomHeightOffset);
        }

        public void SetColor(Color color)
        {
            _image.color = color;
        }

        void OnEnable()
        {
            Animate();
        }

        [Button("Animate")]
        public void Animate()
        {
            if(_sequence != null && _sequence.IsActive())
                _sequence.Kill();

            _sequence = DOTween.Sequence();

            var colorSequence = DOTween.Sequence();
            var scaleSequence = DOTween.Sequence();

            colorSequence.Append(DOTween.To(
                () => _image.color, x => _image.color = x, new Color(_image.color.r, _image.color.g, _image.color.b, 1), 0.45f)
                .From(new Color(_image.color.r, _image.color.g, _image.color.b, 0f))
                .SetEase(Ease.OutQuad));
            colorSequence.Append(DOTween.To(
                () => _image.color, x => _image.color = x, new Color(_image.color.r, _image.color.g, _image.color.b, 0), 0.45f)
                .SetEase(Ease.InQuad));

            scaleSequence.Append((transform as RectTransform).DOScale(_baseScale * _actualScale, 0.25f).From(Vector3.zero).SetEase(Ease.OutQuad));
            scaleSequence.Append((transform as RectTransform).DOScale(Vector3.zero, 0.65f).SetEase(Ease.InQuad));

            _sequence.AppendInterval(_randomDelay);
            _sequence.Append(DOTween.To(() => (transform as RectTransform).anchoredPosition.y, x => (transform as RectTransform).anchoredPosition = new Vector2((transform as RectTransform).anchoredPosition.x, x), _originalPosition.y + _actualHeight, .9f)
                .From(_originalPosition.y)
                .SetEase(Ease.OutQuad));
            _sequence.Join(colorSequence);
            _sequence.Join(scaleSequence);  
        }


    }
}
