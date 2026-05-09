using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;

namespace MonkeyBusiness.Combat.Weapons
{
    /// <summary>
    /// Moves the weapon 
    /// </summary>
    public class WeaponMover : MonoBehaviour
    {
        Vector3 _initialPosition;

        [SerializeField]
        float _moveDistance = 0.5f;

        [SerializeField]
        bool _moveInstantly = false;

        InputAction _moveAction;


        TweenerCore<Vector3, Vector3, VectorOptions> _moveTween;

        void Awake()
        {
            _initialPosition = transform.localPosition;
            _moveAction = InputSystem.actions.FindAction("Move");
        }

        void FixedUpdate()
        {            
            //Debug.Log("Moving weapon " + _moveAction.ReadValue<Vector2>());
            var movement = _moveAction.ReadValue<Vector2>().x;
            
            if(_moveInstantly)
            {
                transform.localPosition = _initialPosition - Vector3.right * movement * _moveDistance;
            }
            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, _initialPosition - Vector3.right * movement * _moveDistance, Time.fixedDeltaTime * 10f);
            }
        }

    
        public void MoveTo(Vector3 targetLocalPosition, float duration, Ease ease = Ease.InOutQuad, bool enableOnComplete = false)
        {
            if(_moveTween != null)
            {
                DOTween.Kill(_moveTween);
            }
            _moveTween = transform.DOLocalMove(targetLocalPosition, duration).SetEase(ease).OnComplete(() =>
            {
                _moveTween = null;
                if(enableOnComplete)
                {
                    this.enabled = true;
                }  
            });
        }

    }
}