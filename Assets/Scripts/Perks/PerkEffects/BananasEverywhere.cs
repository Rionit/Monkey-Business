using System;
using MonkeyBusiness.Items;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class BananasEverywhereEffect : PerkEffectBase
    {
        [SerializeField]
        private GameObject bananaPrefab;

        [SerializeField]
        private int bananaCount = 30;

        [SerializeField]
        private float spawnRadius = 25f;

        [SerializeField]
        private float spawnHeight = 20f;

        [SerializeField]
        private float downwardForce = 10f;

        public override void Apply()
        {
            for (int i = 0; i < bananaCount; i++)
            {
                Vector3 spawnPos = new Vector3(
                    UnityEngine.Random.Range(-spawnRadius, spawnRadius),
                    spawnHeight,
                    UnityEngine.Random.Range(-spawnRadius, spawnRadius)
                );

                GameObject bananaObj = UnityEngine.Object.Instantiate(
                    bananaPrefab,
                    spawnPos,
                    UnityEngine.Random.rotation
                );

                Banana banana = bananaObj.GetComponent<Banana>();
                Rigidbody rb = bananaObj.GetComponent<Rigidbody>();

                // Force banana into "already eaten / peel state"
                if (banana != null)
                {
                    banana.ForcePeelState();
                }

                // Drop it from the sky
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.detectCollisions = true;

                    rb.AddForce(Vector3.down * downwardForce, ForceMode.Impulse);
                }
            }
        }

        public override void Update()
        {
        }

        public override void Reset()
        {
        }
    }
}