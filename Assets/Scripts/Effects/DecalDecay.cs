using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace MonkeyBusiness.Effects
{
    public class DecalDecay : MonoBehaviour
    {
        [SerializeField]
        float _beforeDecayTime = 3f;

        [SerializeField]
        float _decayTime = 2f;

        [SerializeField]
        DecalProjector _decalProjector;

        int _receivedIndex = -1;

        public UnityEvent<int> OnDecayed;

        void Start()
        {
            StartCoroutine(DecayCoroutine());
        }


        IEnumerator DecayCoroutine()
        {
            _receivedIndex = DecalManager.Instance.RegisterNewDecal(this);
            yield return new WaitForSeconds(_beforeDecayTime);

            var tween = DOTween.To(() => _decalProjector.fadeFactor, x => _decalProjector.fadeFactor = x, 0f, _decayTime);

            yield return tween.WaitForCompletion();

            OnDecayed?.Invoke(_receivedIndex);
            Destroy(gameObject);
        }

        public void RemoveImmediately()
        {
            StopAllCoroutines();
            Destroy(gameObject);
        }
    }
}
