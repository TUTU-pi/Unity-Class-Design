using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class camera : MonoBehaviour
{
    public float Basic_moveSpeed = 100f;       // 基础移动速度
    public float sprintMultiplier = 10f;      // 加速倍率
    public float mouseSensitivity = 200f;    // 鼠标灵敏度
    public float scrollSeneitivity = 100;
    public float rotateSpeed = 20;
    //public GameObject panel;
    public Transform targetPos;
    


    private float xRotation = 0f;            // 上下旋转角度（限制抬头低头）

    private float horizontal;
    private float vertical;
    private float moveSpeed;


    private float curscrollSensity;
    //private bool panelEnable;
    //private Animator panelanimator;

    void Start()
    {

        // 锁定并隐藏鼠标
        curscrollSensity = scrollSeneitivity;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
        gameObject.SetActive(false);
        //panelEnable = false;
        //panelanimator = panel.GetComponent<Animator>();




    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
        //    panel.SetActive(!panelEnable);
        //    panelEnable = !panelEnable;
        //    if (panelEnable)
        //    {
        //        panelanimator.SetTrigger("PanelIn");
        //    }
        //    else
        //    {
        //        panelanimator.SetTrigger("PanelOut");
        //    }
        //}
        //if (!panelEnable)
        //{
        //    UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        //    UnityEngine.Cursor.visible = false;
        //}
        //else
        //{
        //    UnityEngine.Cursor.lockState = CursorLockMode.Confined;
        //    UnityEngine.Cursor.visible = true;
        //}


        // 重置输入
        horizontal = 0;
        vertical = 0;
        moveSpeed = Basic_moveSpeed;

        // WASD控制移动
        // 移动输入检测
        if (Input.GetKey(KeyCode.W)) vertical = 1;
        if (Input.GetKey(KeyCode.S)) vertical = -1;
        if (Input.GetKey(KeyCode.D)) horizontal = 1;
        if (Input.GetKey(KeyCode.A)) horizontal = -1;

        
        // 加速
        if (Input.GetKey(KeyCode.LeftControl))
        {
            curscrollSensity = sprintMultiplier * scrollSeneitivity;
        } else
        {
            curscrollSensity = scrollSeneitivity;
        }


        // 绕着定轴旋转视角
        if (Input.GetKey(KeyCode.Q))
        {
            Vector3 targetPosition = new Vector3(targetPos.position.x, transform.position.y, targetPos.position.z);
            transform.RotateAround(targetPosition, Vector3.up, rotateSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            Vector3 targetPosition = new Vector3(targetPos.position.x, transform.position.y, targetPos.position.z);
            transform.RotateAround(targetPosition, Vector3.up, - rotateSpeed * Time.deltaTime);
        }

        // 滚轮加速移动
        float scroll = Input.GetAxis("Mouse ScrollWheel") * scrollSeneitivity;
        transform.position += transform.forward * scroll;

        // 按住鼠标左键平移
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            Vector3 forwardNoY = transform.forward;
            forwardNoY.y = 0;

            transform.position += -transform.right * mouseX * mouseSensitivity;
            transform.position += -forwardNoY * mouseY * mouseSensitivity;
            return;
        }


        RotateCamera();

        // 执行移动
        Move();
    }

    private void Move()
    {
        Vector3 direction = (transform.forward * vertical + transform.right * horizontal).normalized;
        transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
    }


    private void RotateCamera()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 限制上下旋转角度（-80° ~ 80°），防止相机翻转
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        // 应用旋转
        transform.localRotation = Quaternion.Euler(xRotation, transform.localEulerAngles.y + mouseX, 0f);
    }

    //public void changeCamerabutton()
    //{
    //    panelEnable = !panelEnable;
    //    panel.SetActive(panelEnable);
    //}

}