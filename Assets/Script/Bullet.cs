using UnityEngine;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    [Header("子弹属性")]
    public float lifeTime = 3f;         // 子弹生命周期
    public float bulletSpeed = 100f;     // 子弹速度

    private Vector3 flyDirection; // 固定方向！


    void Start()
    {
        flyDirection = transform.forward;
        // 自动销毁子弹，避免占用内存
        Destroy(gameObject, lifeTime);
    }
    
    void Update()
    {
        // 关键：永远朝固定方向，使用世界坐标系移动
        transform.Translate(flyDirection * bulletSpeed * Time.deltaTime, Space.World);
    }


    //private void OnCollisionEnter(Collision collision)
    //{
    //    Destroy(gameObject);
    //}

}