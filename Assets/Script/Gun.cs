using TMPro;  
using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    [Header("�������")]
    public GameObject bulletPrefab;     // �ӵ�Ԥ����
    public Transform firePoint;         // �ӵ������λ��
    public int total_bullets = 200;
    public int magbulletcount = 30;

    [Header("���Ƶ��")]
    public float fireRate = 0.2f;       // ���������룩
    public TMP_Text UIText;

    private Animator animator;
    private bool isPlaying;


    private float nextFireTime = 0f;    // �´ο��������ʱ��
    private int curbullets;
    private int cur_total_bullets;
    private bool isReloading;


    void OnEnable()
    {
        if (UIText != null) UIText.enabled = true;
        UpdateAmmoUI();
    }

    void OnDisable()
    {
        if (UIText != null) UIText.enabled = false;
    }

    private void Start()
    {
        isReloading = false;
        isPlaying = false;
        curbullets = magbulletcount;
        cur_total_bullets = total_bullets;
        Debug.Log(curbullets);
        UpdateAmmoUI();

        animator = GetComponent<Animator>();
    }



    void Update()
    {

        if (isReloading ||( cur_total_bullets == 0 && curbullets == 0))
        {
            //gunanimator.SetTrigger("Play");
            return ;
        }
        if (curbullets <= 0)
        {
            isReloading = true;
            StartCoroutine(reload());

        }

        else if (curbullets != magbulletcount && Input.GetKey(KeyCode.R))
        {
            isReloading = true;
            StartCoroutine(reload());
        }


        if (Input.GetMouseButton(0) && Time.time > nextFireTime)
        {
            Shoot();
        }
        

    }


    private void Shoot()
    {
        //Debug.Log("�ӵ�������");
        // �����ӵ�
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        nextFireTime = Time.time + fireRate;
        curbullets --;
        //Debug.Log($"��ǰ�ӵ�����:{curbullets}");
        UpdateAmmoUI();
    }


    IEnumerator reload() {
        if (! isPlaying)
        {
            animator.SetTrigger("ChangeBullets");
            isPlaying = true;
        }
            
        yield return new WaitForSeconds(1f);
        isPlaying = false;
        int cur = magbulletcount - curbullets;
        if (cur > cur_total_bullets)
        {
            curbullets = cur_total_bullets;
            cur_total_bullets = 0;
        } else
        {
            curbullets = magbulletcount;
            cur_total_bullets -= cur;
        }
           
        isReloading = false;
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (UIText != null)
            UIText.text = $"{curbullets}/{cur_total_bullets}";
    }

    public bool GetSupply()
    {
        if (cur_total_bullets == total_bullets && curbullets == magbulletcount) return false;
        cur_total_bullets = total_bullets;
        curbullets = magbulletcount;
        UpdateAmmoUI();
        return true;
    }

    public void selfEnable()
    {
        gameObject.SetActive(true);
    }

    public void selfDisable()
    {
        gameObject.SetActive(false);
    }

    IEnumerator DisableWhenAnimationEnds()
    {
        // ���Ŷ���
        animator.Play("RemoveGun", 0, 0f);

        // �ȴ�һ֡�ö�����ʼ
        yield return null;

        // ��ȡ�������Ȳ��ȴ�
        float length = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(length);

        // ��������
        gameObject.SetActive(false);
    }
}