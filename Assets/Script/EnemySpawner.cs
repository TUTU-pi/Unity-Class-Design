using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人生成器：在给定位置范围内分批生成敌人，追踪存活敌人数量，
/// 全部消灭后激活传送门。挂载到场景中的空 GameObject 上。
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("生成设置")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnRange = 3f;
    [SerializeField] private float spawnInterval = 0.8f;
    [SerializeField] private int totalEnemies = 10;

    [Header("UI")]
    [SerializeField] private Text enemyCountText;

    [Header("传送门")]
    [SerializeField] private Portal portal;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private int spawnedCount;
    private bool spawningDone;

    private void Start()
    {
        UpdateUI();
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < totalEnemies; i++)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
        spawningDone = true;
        Debug.Log($"[EnemySpawner] 生成完毕, 存活: {aliveEnemies.Count}, 已生成: {spawnedCount}");
        CleanupNullRefs();
        TryActivatePortal();
    }

    private void SpawnOne()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Vector3 pos = point.position + Random.insideUnitSphere * spawnRange;
        pos.y = point.position.y;

        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        aliveEnemies.Add(enemy);
        spawnedCount++;
    }

    /// <summary>敌人被消灭时由 Enemy 脚本调用</summary>
    public void OnEnemyDestroyed(GameObject enemy)
    {
        aliveEnemies.Remove(enemy);
        UpdateUI();
        CleanupNullRefs();
        Debug.Log($"[EnemySpawner] 敌人被消灭, 存活(清理后): {aliveEnemies.Count}, 生成完成: {spawningDone}");
        TryActivatePortal();
    }

    private void CleanupNullRefs()
    {
        aliveEnemies.RemoveAll(e => e == null);
    }

    private void TryActivatePortal()
    {
        if (spawningDone && aliveEnemies.Count == 0)
        {
            Debug.Log("[EnemySpawner] 所有敌人已消灭，激活传送门！");
            if (portal != null)
            {
                portal.Activate();
            }
            else
            {
                Debug.LogWarning("[EnemySpawner] portal 未赋值！请在 Inspector 中拖入传送门对象");
            }
        }
    }

    private void UpdateUI()
    {
        if (enemyCountText != null)
        {
            int killed = spawnedCount - aliveEnemies.Count;
            enemyCountText.text = $"消灭: {killed} / {totalEnemies}";
        }
    }

}
