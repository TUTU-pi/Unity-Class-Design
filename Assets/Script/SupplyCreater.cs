using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyCreater : MonoBehaviour
{

    public GameObject supplyPrefab;  // 弹药箱预制体
    public float spawnInterval = 20f; // 生成间隔（秒）
    public LayerMask groundLayer;
    public float xRange = 30;
    public float zRange = 30;
    public float yRange = 5;
    public int maxSupplyCnt = 5;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreateSupply());
    }

    private void Update()
    {
        //if (supply.curSpplyCnt < maxSupplyCnt)
        //{
        //    StartCoroutine(CreateSupply());
        //}
        
    }

    IEnumerator CreateSupply()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            if (supply.curSpplyCnt < maxSupplyCnt)
            {
                Instantiate(supplyPrefab, GetPosition(), supplyPrefab.transform.rotation);
            }
            
        }
    }

    Vector3 GetPosition()
    {
        float x = UnityEngine.Random.Range(-xRange, xRange);
        float z = UnityEngine.Random.Range(-zRange, zRange);
        float y = UnityEngine.Random.Range(0.5f, yRange);
        Vector3 pos = new Vector3(x, y, z);
        return pos;
    }

    // 在Scene视图中显示生成范围（调试用）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(0f, 0f, 0f);
        Vector3 size = new Vector3(2*xRange, 2*yRange, 2*zRange);
        Gizmos.DrawWireCube(center, size);
    }
}
