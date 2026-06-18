using UnityEngine;

namespace MonkeyBusiness.Misc
{
    [RequireComponent(typeof(Animator))]
    public class RandomAnimOffset : MonoBehaviour
    {
        private static readonly int OffsetHash = Animator.StringToHash("Offset");

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            float offset = Random.value;
            GetComponent<Animator>().SetFloat(OffsetHash, offset);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
