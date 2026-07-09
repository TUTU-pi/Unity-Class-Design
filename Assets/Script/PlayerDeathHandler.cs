using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 玩家死亡处理：被敌人碰到时显示死亡提示，3 秒后回到 3DGame。
/// 挂载到玩家 GameObject 上。
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private float delay = 3f;

    private bool isDead;

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("玩家死亡！");

        if (deathPanel != null) deathPanel.SetActive(true);

        // 根据你的玩家脚本类型禁用移动
        var movement = GetComponent<pla>();
        if (movement != null) movement.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;

        StartCoroutine(BackToScene());
    }

    private IEnumerator BackToScene()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("3DGame");
    }
}
