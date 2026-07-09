using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Door : MonoBehaviour
{

    private bool IsOpen = false;

    private float doorTimer = 0f;

    public float OpenTime = 3;
    public AudioClip AudioOpen;

    public AudioClip AudioClose;
    Color oldColor;




    // Start is called before the first frame update
    void Start()
    {
        OpenTime = 60 * OpenTime;
        oldColor = gameObject.GetComponent<Renderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOpen)
        {
            doorTimer++;
            if (doorTimer == OpenTime)
            {
                CloseDoor();
                IsOpen = false;
            }
        }
        


    }







    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && IsOpen == false)
        {
            if (other.GetComponent<pla>().supplyCnt > 3)
            {
                IsOpen = true;
                OpenDoor();
                doorTimer = 0;
            }
            
        }


    }

    public void OpenDoor()
    {
        IsOpen = true;

        gameObject.GetComponent<AudioSource>().PlayOneShot(AudioOpen);

        gameObject.transform.parent.GetComponent<Animation>().Play("dooropen");
    }

    public void CloseDoor()
    {
        IsOpen = false;

        gameObject.GetComponent<AudioSource>().PlayOneShot(AudioClose);

        gameObject.transform.parent.GetComponent<Animation>().Play("doorclose");
    }


    public void OnMouseOver()
    {
        GetComponent<Renderer>().material.color = Color.red;
        //transform.Rotate(0f, 1f, 0f);
        if (IsOpen) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            IsOpen = true;
            OpenDoor();
            doorTimer = 0;
        }
    }

    public void OnMouseExit()
    {
        gameObject.GetComponent<Renderer>().material.color = oldColor;
    }
}
