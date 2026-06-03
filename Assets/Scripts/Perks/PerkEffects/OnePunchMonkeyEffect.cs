using System;
using MonkeyBusiness.Combat.Attack;
using MonkeyBusiness.Managers;
using MonkeyBusiness.Misc;
using UnityEngine;

namespace MonkeyBusiness.Perks.PerkEffects
{
    [Serializable]
    public class OnePunchMonkeyEffect : PerkEffectBase
    {
        public enum EntityTypes { PLAYER, ENEMY }
        public GameObject explosionPrefab;
        public EntityTypes targetEntityType = EntityTypes.ENEMY;
        
        public override void Apply()
        {
            if (targetEntityType == EntityTypes.PLAYER)
                StaticEvents.OnEnemyMeleeAttackUsed += Explode;
            else if (targetEntityType == EntityTypes.ENEMY)
                StaticEvents.OnPlayerMeleeAttackUsed += Explode;
        }

        void Explode()
        {
            GameObject explosion = GameObject.Instantiate(explosionPrefab, GameManager.Instance.PlayerCharacter.transform);
            explosion.GetComponent<Explosion>().targetEntityType = "Enemy";
        }

        void Explode(GameObject invoker)
        {
            GameObject explosion = GameObject.Instantiate(explosionPrefab, invoker.transform);
            explosion.GetComponent<Explosion>().targetEntityType = "Player";
        }
        
        public override void Update()
        {
            
        }

        public override void Reset()
        {
            if (targetEntityType == EntityTypes.PLAYER)
                StaticEvents.OnEnemyMeleeAttackUsed -= Explode;
            else if (targetEntityType == EntityTypes.ENEMY)
                StaticEvents.OnPlayerMeleeAttackUsed -= Explode;
        }
        /*
         
        // You can also override this function to return text with your values
        public override string GetDescription()
        {
            return description.Replace("<value>", value.ToString());
        }
        
        // Don't forget to tell the designers what <placeholder> types they can use!
        // e.g. <multiplier>, <object> etc.
        protected override string GetTooltip()
        {
            return "Available placeholders:\n<value> - Max health change amount";
        }
        
        */
    }
}
