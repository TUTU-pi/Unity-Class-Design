using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tag : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform targetPos;
    public Transform portalPlace;
    public GameObject player;

    [Header("缩放设置")]
    public float hoverScale = 1.5f;  // 悬停时的缩放倍数
    public float duration = 0.2f;    // 过渡时间
    
    //private Vector3 originalScale;
    //private bool isHovering = false;
    //private float currentLerpTime = 0f;
    private Color oldColor;

    void Start()
    {
        //originalScale = transform.localScale;
        //oldColor = gameObject.GetComponent<Renderer>().material.color;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(targetPos);
    }

    private void OnMouseOver()
    {
        Debug.Log("鼠标已放上来");
        //GetComponent<Material>().color = Color.red;
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("检测到鼠标点击");
            player.transform.position = portalPlace.position;
        }
    }

    private void OnMouseExit()
    {
        //GetComponent<Material>().color = oldColor;
    }

}
