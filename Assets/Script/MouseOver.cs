using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseOver : MonoBehaviour
{
    Color oldColor;
    void Start()
    {
        oldColor = GetComponent<Renderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseOver()
    {
        GetComponent<Renderer>().material.color = Color.red;
        transform.Rotate(0f, 1f, 0f);
     }

    public void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = oldColor;
    }

}
