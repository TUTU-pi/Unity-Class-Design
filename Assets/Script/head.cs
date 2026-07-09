//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class head : MonoBehaviour
//{
//    // Start is called before the first frame update
//    [Header("鼠标上下视角")]
//    public float mouseSensitivity = 20f;
//    public float minPitch = -10f;  // 最大低头
//    public float maxPitch = 10f;   // 最大抬头

//    private float pitch = 0f; // 初始就是平视

//    void Start()
//    {
        
//    }

    

//    void Update()
//    {
//        ////transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * mouseSensitivity, Space.World);
//        //transform.Rotate(Vector3.left, Input.GetAxis("Mouse Y") * mouseSensitivity, Space.Self);


//        // 鼠标上下输入
//        float mouseY = Input.GetAxis("Mouse Y");
//        pitch -= mouseY * mouseSensitivity;

//        // 限制角度，不会无限旋转钻地
//        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

//        // 只设置本地X轴旋转，初始一定平视
//        transform.localEulerAngles = new Vector3(pitch, 0, 0);

//    }
//}
