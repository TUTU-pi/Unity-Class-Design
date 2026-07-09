using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour
{
    [Header("登录面板")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private InputField loginUsernameInput;
    [SerializeField] private InputField loginPasswordInput;
    [SerializeField] private Button loginBtn;
    [SerializeField] private Button goRegisterBtn;

    [Header("注册面板")]
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private InputField registerUsernameInput;
    [SerializeField] private InputField registerPasswordInput;
    [SerializeField] private Button registerBtn;
    [SerializeField] private Button goLoginBtn;

    [Header("提示面板")]
    [SerializeField] private GameObject tipPanel;
    [SerializeField] private Text tipText;
    [SerializeField] private Button tipCloseBtn;




    private void Start()
    {
        loginBtn.onClick.AddListener(OnLoginClick);
        registerBtn.onClick.AddListener(OnRegisterClick);
        goRegisterBtn.onClick.AddListener(SwitchToRegister);
        goLoginBtn.onClick.AddListener(SwitchToLogin);
        tipCloseBtn.onClick.AddListener(HideTip);

        ShowLoginPanel();
    }

    private void OnLoginClick()
    {
        string user = loginUsernameInput.text;
        string pass = loginPasswordInput.text;

        if (AccountManager.Instance.Login(user, pass, out string msg))
        {
            ShowTip(msg);
            SceneManager.LoadScene("3DGame");
        }
        else
        {
            ShowTip(msg);
        }
    }

    private void OnRegisterClick()
    {
        string user = registerUsernameInput.text;
        string pass = registerPasswordInput.text;

        if (AccountManager.Instance.Register(user, pass, out string msg))
        {
            ShowTip(msg);
            // 注册成功后切回登录面板
            ShowLoginPanel();
            loginUsernameInput.text = user;
            loginPasswordInput.text = "";
        }
        else
        {
            ShowTip(msg);
        }
    }

    private void SwitchToLogin()
    {
        ShowLoginPanel();
    }

    private void SwitchToRegister()
    {
        registerPanel.SetActive(true);
        //loginPanel.SetActive(false);
        tipPanel.SetActive(false);
        registerUsernameInput.text = "";
        registerPasswordInput.text = "";


    }

    private void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        tipPanel.SetActive(false);
    }

    private void ShowTip(string msg)
    {
        tipText.text = msg;
        tipPanel.SetActive(true);
    }

    private void HideTip()
    {
        tipPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}
