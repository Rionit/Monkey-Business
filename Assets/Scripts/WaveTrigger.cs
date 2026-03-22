using UnityEngine;
using System.Collections.Generic;
public class WaveTrigger : MonoBehaviour
{

    [SerializeField]
    List<GameObject> enemiesToActivate;

    bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if(!triggered)
        {
            Debug.Log(other.gameObject.name + " entered wave trigger");
            if(other.CompareTag("Player"))
            {
                Debug.Log("Player triggered the wave");
                foreach(var enemy in enemiesToActivate)
                {
                    enemy.SetActive(true);
                }
                triggered = true;
            }

        }
    }
}
