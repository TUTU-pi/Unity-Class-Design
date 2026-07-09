using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrigger : MonoBehaviour
{
    public GameObject fire;
    // Start is called before the first frame update
    public AudioClip FireOpen;
    private bool isOpen;

    private AudioSource audioSource;

    void Start()
    {
        fire.SetActive(false);
        isOpen = false;

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = FireOpen;
        // …Ë÷√—≠ª∑≤•∑≈
        audioSource.loop = true;

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseOver()
    {
        Debug.Log("ºÏ≤‚µΩ Û±Í");
        if (Input.GetKey(KeyCode.E))
        {
            Debug.Log($"ºÏ≤‚µΩEº¸ ‰»Î, isOpen{isOpen}");
            if (! isOpen)
            {
                fire.SetActive(true);
                isOpen = true;
            }
            else
            {
                fire.SetActive(false);
                isOpen = false;
            }
            
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ºÏ≤‚µΩ≈ˆ◊≤, tag:{other.tag}, isOpen{isOpen}");
        if (other.tag == "Player")
        {
            if (! isOpen)
            {
                fire.SetActive(true);
                isOpen = true;
                // ≤•∑≈“Ù∆µ
                audioSource.Play();
            }
            else
            {
                fire.SetActive(false);
                isOpen = false;
                audioSource.Stop();
            }
        }
    }
}
