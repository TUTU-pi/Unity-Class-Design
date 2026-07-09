using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class supply : MonoBehaviour
{
    public static int curSpplyCnt = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Gun gun = other.GetComponentInChildren<Gun>();
            
            if (gun != null && gun.GetSupply())
            {
                Destroy(gameObject);
                curSpplyCnt --;
            }

            pla player = other.GetComponent<pla>();
            player.PlusCnt();

            Destroy(gameObject);
        }
    }
    void Start()
    {
        curSpplyCnt++;
    }

    
    void Update()
    {
        transform.Rotate(0, 1, 0, Space.World);
    }
}
