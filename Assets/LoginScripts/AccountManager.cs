using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class AccountData
{
    public string username;
    public string password;

    public AccountData(string user, string pass)
    {
        username = user;
        password = pass;
    }
}

[System.Serializable]
public class AccountList
{
    public List<AccountData> accounts = new List<AccountData>();
}

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    private List<AccountData> accounts = new List<AccountData>();
    private string filePath;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        filePath = Path.Combine(Application.dataPath, "LoginScripts/accounts.json");
        LoadAccounts();
    }

    public bool Register(string username, string password, out string message)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            message = "用户名不能为空";
            return false;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            message = "密码不能为空";
            return false;
        }

        foreach (var acc in accounts)
        {
            if (acc.username == username)
            {
                message = "该账号已存在";
                return false;
            }
        }

        accounts.Add(new AccountData(username, password));
        SaveAccounts();
        message = "注册成功";
        return true;
    }

    public bool Login(string username, string password, out string message)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            message = "用户名不能为空";
            return false;
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            message = "密码不能为空";
            return false;
        }

        foreach (var acc in accounts)
        {
            if (acc.username == username && acc.password == password)
            {
                message = "登录成功";
                return true;
            }
        }

        message = "用户名或密码错误";
        return false;
    }

    private void SaveAccounts()
    {
        AccountList list = new AccountList { accounts = this.accounts };
        string json = JsonUtility.ToJson(list, true);
        File.WriteAllText(filePath, json);
    }

    private void LoadAccounts()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            AccountList list = JsonUtility.FromJson<AccountList>(json);
            if (list != null && list.accounts != null)
                accounts = list.accounts;
        }
    }

}
