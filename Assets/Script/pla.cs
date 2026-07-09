using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class pla : MonoBehaviour
{
    // �ƶ��ٶȣ�����Inspector������
    [Header("����")]
    public float Basic_moveSpeed = 5f;
    //public float jumpspeed = 10f;
    public float G = 10;
    public float sprintMultiplier = 2f; // ����ʱ�ı���
    public float Jumpa = 5f;
    public float mouseSensitivity = 20f;


    [Header("������")]
    public float groundCheckDistance = 1f;  // ���¼�����
    public LayerMask groundLayer;             // �����ѡ����ĵ��β�



    

    private bool canShoot;
    // ���������루W/S ����ǰ��A/D �������ң�
    private float horizontal; // ˮƽ�����루���ң�
    private float vertical;   // ��ֱ�����루ǰ��
    private float ymove;  // �����ƶ� 
    private bool isMoving;
    private bool isJumping;
    private bool canMove;
    private float moveSpeed;
    private float ya = 0;
    private float g, jumpa;

    private bool isGrounded;
    [Header("����")]
    public float minPitch = -90f;  // ����ͷ
    public float maxPitch = 70f;   // ���̧ͷ
    public GameObject f3cam;


    private float pitch = 0f; // ��ʼ����ƽ��


    public GameObject panel;
    public int supplyCnt;  // ͳ���ܹ����˶��ٲ���
    public TMP_Text text;

    private bool panelEnable;
    private GameObject gun;
    private Rigidbody rb;
    private Camera cam1;
    private Camera cam2;
    private Animator gunanimator;
    private Animator panelanimator;
    private void Start()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        g = 100 * G;
        jumpa = Jumpa;
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.constraints = RigidbodyConstraints.FreezeRotation;
        canShoot = false;

        Transform upper = transform.Find("upper");
        if (upper != null)
        {
            cam1 = upper.GetComponent<Camera>();
            if (cam1 != null) cam1.enabled = true;

            Transform f3Cam = upper.Find("f3Camera");
            if (f3Cam != null)
            {
                cam2 = f3Cam.GetComponent<Camera>();
                if (cam2 != null) cam2.enabled = false;
            }

            Transform gunTransform = upper.Find("Gun");
            if (gunTransform != null)
            {
                gun = gunTransform.gameObject;
                gun.SetActive(false);
                gunanimator = gun.GetComponent<Animator>();
            }
        }

        canMove = true;

        if (panel != null)
            panelanimator = panel.GetComponent<Animator>();

        if (text != null)
            UpdateUI();
    }

    private void Update()
    {

        

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //panel.SetActive(!panelEnable);

            panelEnable = !panelEnable;
            if (panelanimator != null)
            {
                if (panelEnable)
                    panelanimator.SetTrigger("PanelIn");
                else
                    panelanimator.SetTrigger("PanelOut");
            }
        }

        if (!panelEnable)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
            TurnAround();

        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            UnityEngine.Cursor.visible = true;
        }

        if (!canMove)
        {
            return ;
        } 

        horizontal = 0; // ˮƽ�����루���ң�
        vertical = 0;   // ��ֱ�����루ǰ��
        isMoving = false;
        isJumping = true;

        

        moveSpeed = Basic_moveSpeed;

        if (Input.GetKeyDown(KeyCode.C))
        {
            cam1.enabled = !cam1.enabled;
            cam2.enabled = !cam2.enabled;
        }


        // ���W������ǰ��
        if (Input.GetKey(KeyCode.W))
        {
            vertical = 1;
            isMoving = true;
        }
        // ���S�������
        if (Input.GetKey(KeyCode.S))
        {
            vertical = -1;
            isMoving = true;
        }

        // ���D�������ң�
        if (Input.GetKey(KeyCode.D))
        {
            horizontal = 1;
            isMoving = true;
        }
        // ���A��������
        if (Input.GetKey(KeyCode.A))
        {
            horizontal = -1;
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveSpeed = sprintMultiplier * Basic_moveSpeed;
        }
        else
        {
            moveSpeed = Basic_moveSpeed;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            canShoot = !canShoot;
            if (canShoot)
            {
                gun.SetActive(true);
            }
            else
            {
                gunanimator.SetTrigger("RemoveGun");
            }
        }

        

        // 地面检测
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );

        isJumping = !isGrounded;

        // 跳跃：只有在地面上才能跳
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpa, ForceMode.Impulse);
        }

        if (isMoving || isJumping)
        {
            Move();
        }

            

    }

    private void TurnAround()
    {
        // �������ӽǵ�����ת��
        transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * mouseSensitivity, Space.World);



        // �������¿�
        float mouseY = Input.GetAxis("Mouse Y");
        pitch -= mouseY * mouseSensitivity;

        // ���ƽǶȣ�����������ת���
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // ֻ���ñ���X����ת����ʼһ��ƽ��
        if (cam1 != null)
            cam1.transform.localEulerAngles = new Vector3(pitch, 0, 0);

    }
    private void Move()
    {
        Vector3 moveDir = transform.right * horizontal + transform.forward * vertical;
        moveDir.Normalize();
        Vector3 vel = rb.velocity;
        vel.x = moveDir.x * moveSpeed;
        vel.z = moveDir.z * moveSpeed;
        rb.velocity = vel;
    }


    public void PlusCnt()
    {
        supplyCnt++;
        UpdateUI();
    }


    private void UpdateUI()
    {
        if (text != null)
            text.text = $"SupplyCnt:{supplyCnt}";
    }

    public void shootButton()
    {
        Debug.Log("检测到按下按钮");
        canShoot = !canShoot;
        if (canShoot) 
        {
            gun.SetActive(true);
        }
        else 
        {
            gunanimator.SetTrigger("RemoveGun");
        }
        
        
    }

    public void changeCamerabutton()
    {
        canMove = !canMove;
        if (canMove)
        {
            if (f3cam != null) f3cam.SetActive(false);
            if (cam1 != null) cam1.enabled = true;
        } else
        {
            if (f3cam != null) f3cam.SetActive(true);
            if (cam1 != null) cam1.enabled = false;
        }

        
        //panelEnable = !panelEnable;
        //panel.SetActive(panelEnable);
    }

}