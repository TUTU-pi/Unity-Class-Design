using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{

    private Animation animation;
    // Start is called before the first frame update
    void Start()
    {
        animation = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("¼ì²âµ½Åö×²");
        if (other.tag == "bullet")
        {
            Debug.Log("¼ì²âµ½×Óµ¯Åö×²");
            animation.Play("Take 001");
            Destroy(other.gameObject);
        }
    }

}
