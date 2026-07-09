using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetinShootGame : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject inPanel;
    void Start()
    {
        inPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        inPanel.SetActive(true);
    //    }
        
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.tag == "Player")
    //    {
    //        inPanel.SetActive(false);
    //    }
    //}
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            inPanel.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            inPanel.SetActive(false);
        }
    }
}
