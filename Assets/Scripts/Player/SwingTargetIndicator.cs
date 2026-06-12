using UnityEngine;

namespace MonkeyBusiness.Player
{
    public class SwingTargetIndicator : MonoBehaviour
    {

        private MeshRenderer _meshRenderer;

        [SerializeField]
        private PlayerCharacter _playerCharacter;

        [SerializeField]
        private float _offset = 0.75f;

        [SerializeField]
        private float _minimalDistanceToPlayer = 2.0f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_playerCharacter.IsSwingOnCooldown())
            {
                _meshRenderer.enabled = false;
                return;
            }
            
            var target = _playerCharacter.HookScan();

            if(target is null)
            {
                _meshRenderer.enabled = false;
                return;
            }
            
            Vector3 targetPosition = (Vector3) target;
            Transform camera = UnityEngine.Camera.main.transform;
            
            if(Vector3.Distance(targetPosition, camera.position) < _minimalDistanceToPlayer)
            {
                _meshRenderer.enabled = false;
                return;
            }

           

            Vector3 directionToCamera = (camera.position - targetPosition).normalized;

            _meshRenderer.enabled = true;
            transform.position = targetPosition + _offset * directionToCamera;

            transform.LookAt(camera.position);
        }
    }
}
