using UnityEngine;

/// <summary>
/// 敌人脚本：持续向玩家移动，被子弹击中后消失，碰到玩家则玩家死亡。
/// 挂载到敌人预制体上。
/// </summary>
public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stopDistance = 1f;

    private Transform player;
    private EnemySpawner spawner;
    private PlayerDeathHandler deathHandler;

    private void Awake()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            deathHandler = playerObj.GetComponent<PlayerDeathHandler>();
        }
        spawner = FindObjectOfType<EnemySpawner>();
    }

    private void Update()
    {
        if (player == null) return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.magnitude > stopDistance)
        {
            transform.Translate(dir.normalized * moveSpeed * Time.deltaTime, Space.World);
            transform.forward = dir.normalized;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bullet"))
        {
            Debug.Log($"[Enemy] 被子弹击中: {gameObject.name}, spawner={(spawner != null ? "有" : "无")}");
            if (spawner == null) spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null) spawner.OnEnemyDestroyed(gameObject);
            else Debug.LogError("[Enemy] 找不到 EnemySpawner！");
            Destroy(gameObject);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            if (deathHandler != null) deathHandler.Die();
        }
    }
}
