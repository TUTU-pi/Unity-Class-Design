using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("文字")]
    [SerializeField] private Text scoreText;
    [SerializeField] private Text roundText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text deckCountText;
    [SerializeField] private Text skillCountText;
    [SerializeField] private Text opponentSkillCountText;

    [Header("公共牌池 (3张牌)")]
    [SerializeField] private Card[] poolCards;

    [Header("出牌展示区")]
    [SerializeField] private Card playerPlayedCard;
    [SerializeField] private Card aiPlayedCard;

    [Header("结果面板")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultText;
    [SerializeField] private Text resultScoreText;
    [SerializeField] private Button restartBtn;

    private void Start()
    {
        if (restartBtn != null)
            restartBtn.onClick.AddListener(() => GameManager.Instance.RestartGame());

        for (int i = 0; i < poolCards.Length; i++)
        {
            if (poolCards[i] == null) continue;
            int index = i;
            poolCards[i].OnClicked = (card) => GameManager.Instance.OnPoolCardClicked(index);
        }

        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    public void UpdateScores(int player, int ai)
    {
        if (scoreText != null) scoreText.text = "你 " + player + " : " + ai + " AI";
    }

    public void UpdateRound(int round)
    {
        if (roundText != null) roundText.text = "第 " + round + " 回合";
    }

    public void UpdateDeckCount(int count)
    {
        if (deckCountText != null) deckCountText.text = "牌堆剩余: " + count;
    }

    public void UpdateSkillCount(int count, string skillNames)
    {
        if (skillCountText != null)
            skillCountText.text = "你的技能: " + count + "张 " + skillNames;
    }

    public void UpdateOpponentSkillCount(int count)
    {
        if (opponentSkillCountText != null)
            opponentSkillCountText.text = "对手技能: " + count + "张";
    }

    public void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    public void AppendStatus(string msg)
    {
        if (statusText != null) statusText.text += msg;
    }

    public void UpdatePool(CardType[] pool)
    {
        for (int i = 0; i < poolCards.Length; i++)
        {
            if (poolCards[i] == null) continue;
            if (i < pool.Length && pool[i] != CardType.None)
            {
                poolCards[i].SetCardType(pool[i]);
                poolCards[i].gameObject.SetActive(true);
            }
            else
            {
                poolCards[i].gameObject.SetActive(false);
            }
        }
    }

    public void SetPoolInteractable(bool interactable)
    {
        foreach (var card in poolCards)
            if (card != null && card.gameObject.activeSelf)
                card.IsInteractable = interactable;
    }

    public void ShowPlayerPlayedCard(bool show, CardType type = CardType.None)
    {
        if (playerPlayedCard == null) return;
        playerPlayedCard.gameObject.SetActive(show);
        if (show) playerPlayedCard.SetCardType(type);
    }

    public void ShowAIPlayedCard(bool show, CardType type = CardType.None)
    {
        if (aiPlayedCard == null) return;
        aiPlayedCard.gameObject.SetActive(show);
        if (show) aiPlayedCard.SetCardType(type);
    }

    public void ShowResult(string result, int pScore, int aScore)
    {
        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null) resultText.text = result;
        if (resultScoreText != null)
            resultScoreText.text = "最终比分: 你 " + pScore + " : " + aScore + " AI";
    }

    public void HideResult()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }
}
