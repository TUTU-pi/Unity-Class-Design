using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 传送门脚本：初始隐藏，所有敌人消灭后激活并变色，
/// 玩家触碰后重置场景并重新生成敌人。
/// 挂载到传送门预制体上。
/// </summary>
public class Portal : MonoBehaviour
{
    [Header("变色设置")]
    [SerializeField] private float colorSpeed = 3f;
    [SerializeField] private float emissionIntensity = 2f;

    private Material portalMaterial;
    private Collider portalCollider;
    private bool isActive;
    private Color[] colors = new Color[]
    {
        Color.red, Color.green, Color.blue, Color.cyan, Color.magenta, Color.yellow
    };

    private void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) portalMaterial = renderer.material;
        portalCollider = GetComponent<Collider>();
        if (portalCollider != null) portalCollider.isTrigger = true;

        Deactivate();
    }

    private void Update()
    {
        if (isActive && portalMaterial != null)
        {
            float t = Mathf.PingPong(Time.time * colorSpeed, colors.Length - 1);
            int idx0 = Mathf.FloorToInt(t);
            int idx1 = Mathf.CeilToInt(t);
            Color c = Color.Lerp(colors[idx0], colors[idx1], t - idx0);

            portalMaterial.color = c;
            portalMaterial.SetColor("_EmissionColor", c * emissionIntensity);
        }
    }

    /// <summary>激活传送门（由 EnemySpawner 调用）</summary>
    public void Activate()
    {
        isActive = true;
        Debug.Log("传送门已激活！");
        if (portalMaterial != null) portalMaterial.EnableKeyword("_EMISSION");
        if (portalCollider != null)
        {
            portalCollider.enabled = true;
            portalCollider.isTrigger = true;
        }

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = true;
    }

    /// <summary>关闭传送门</summary>
    public void Deactivate()
    {
        isActive = false;
        if (portalMaterial != null) portalMaterial.DisableKeyword("_EMISSION");
        if (portalCollider != null) portalCollider.enabled = false;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"传送门被触发: {other.name}, tag={other.tag}, isActive={isActive}");
        if (!isActive) return;
        if (other.CompareTag("Player"))
        {
            Debug.Log("玩家进入传送门，重新加载场景");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
