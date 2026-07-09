using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectUI : MonoBehaviour
{
    // Start is called before the first frame update

    private bool ruleUIOpen;
    GameObject ruleUI;
    void Start()
    {
        ruleUIOpen = false;
        ruleUI = transform.Find("RulesUI").gameObject;
        ruleUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void RuleButton()
    {
        ruleUIOpen = !ruleUIOpen;
        ruleUI.SetActive(ruleUIOpen);

    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
