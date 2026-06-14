using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace MonkeyBusiness.Perks
{
    public class TimeBarAnimator : MonoBehaviour
    {
        [SerializeField]
        Image _timeBarImage;

        Coroutine _animationCoroutine;

        void Awake()
        {
            _timeBarImage.type = Image.Type.Filled;
            _timeBarImage.fillMethod = Image.FillMethod.Horizontal;
            _timeBarImage.fillAmount = 0f;
        }

        public void Animate(float time)
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
            _animationCoroutine = StartCoroutine(AnimationCoroutine(time));
        }

        public void StopAnimating()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _timeBarImage.fillAmount = 0f;
            }
        }

        IEnumerator AnimationCoroutine(float time)
        {
            _timeBarImage.fillAmount = 1f;

            float originalTime = Time.time;
            float newTime = originalTime;

            while (newTime - originalTime < time)
            {
                newTime = Time.time;
                _timeBarImage.fillAmount = Mathf.Lerp(1f, 0f, (newTime - originalTime) / time);
                yield return null;
            }
            _timeBarImage.fillAmount = 0f;
            _animationCoroutine = null;
        }
    }
}
