using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 出口门脚本：鼠标悬停时提示，按下 E 键返回 3DGame 场景。
/// 挂载到出口门预制体上。
/// </summary>
public class ExitDoor : MonoBehaviour
{
    [SerializeField] private string targetScene = "3DGame";
    [SerializeField] private GameObject hintUI;

    private bool isHovered;

    private void Start()
    {
        if (hintUI != null) hintUI.SetActive(false);
    }

    private void Update()
    {
        if (isHovered && Input.GetKeyDown(KeyCode.E))
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    private void OnMouseEnter()
    {
        isHovered = true;
        if (hintUI != null) hintUI.SetActive(true);
    }

    private void OnMouseExit()
    {
        isHovered = false;
        if (hintUI != null) hintUI.SetActive(false);
    }
}
