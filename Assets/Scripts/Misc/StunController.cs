using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DG.Tweening;
using Ami.BroAudio;

namespace MonkeyBusiness.Misc
{
    public class StunController : MonoBehaviour
    {
        enum VFXType
        {
            ENEMY,
            PLAYER,
        }

        [SerializeField]
        [Tooltip("Components that are gonna be turned off for the stun duration")]
        List<Behaviour> _stunnedComponents; 

        [BoxGroup("Stun VFX")]
        [EnumButtons]
        [SerializeField]
        VFXType _vfxType = VFXType.ENEMY;


        [SerializeField]
        [BoxGroup("Stun VFX")]
        float _stunVFXDuration = 0.3f;

        [SerializeField]
        StunAnimController _stunAnimController;

        [SerializeField]
        Animator _animator;

        /// <summary>
        /// Stuns the components for the specified duration.
        /// </summary>
        /// <param name="duration"></param>
        [BoxGroup("Debug")]
        [Button("Stun for seconds", DisplayParameters = true)]
        public void Stun(float duration)
        {
            StartCoroutine(StunEnemyCoroutine(duration));
        }


        IEnumerator StunEnemyCoroutine(float duration)
        {
            if(_vfxType == VFXType.ENEMY)
            {
                Debug.Log("Animating " + gameObject.name + " stun animation for " + duration + " seconds.");
                _stunAnimController.Animate(_stunVFXDuration);
            }

            if (_animator)
            {
                _animator.SetTrigger("Stun");
            }

            Debug.Log("Stunned for " + duration + " seconds!");
            foreach (var component in _stunnedComponents)
            {
                if (component != null)
                    component.enabled = false;
            }

            yield return new WaitForSeconds(duration);

            foreach (var component in _stunnedComponents)
            {
                if (component != null)
                    component.enabled = true;
            }
        }

    }
}
