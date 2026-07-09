using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GameState
{
    WaitingForSkillPhaseA,
    WaitingForPlay,
    WaitingForSkillPhaseB,
    Revealing,
    WaitingForPoolDraw,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("设置")]
    [SerializeField] private int winScore = 6;

    [Header("引用")]
    [SerializeField] private HandDisplay handDisplay;
    [SerializeField] private OpponentHandDisplay opponentHandDisplay;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private SkillHandDisplay skillHandDisplay;
    [SerializeField] private PlayerSkillBar playerSkillBar;
    [SerializeField] private NeuralAIDriver neuralAI;

    [Header("暂停面板")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button continueBtn;
    [SerializeField] private Button exitBtn;

    [Header("其他")]
    [SerializeField] private GameObject cardIntroduceBoard;

    private bool enableIntroduce;

    private List<CardType> deck = new List<CardType>();
    private CardType[] publicPool;
    private List<CardType> playerHand = new List<CardType>();
    private List<CardType> aiHand = new List<CardType>();

    private CardType playerPlayedCard;
    private CardType aiPlayedCard;

    private int playerScore;
    private int aiScore;
    private int roundNumber;
    private bool playerPicksFirstOnDraw = true;

    private GameState currentState;
    private bool waitingForPoolSelection;
    private int selectedPoolIndex;

    // Skill system
    private List<SkillType> skillDeck = new List<SkillType>();
    private List<SkillType> playerSkills = new List<SkillType>();
    private List<SkillType> aiSkills = new List<SkillType>();

    private SkillType? playerSkillPhaseA;
    private SkillType? playerSkillPhaseB;
    private SkillType? aiSkillPhaseA;
    private SkillType? aiSkillPhaseB;
    private bool playerSkillPhaseAUsed;
    private bool playerSkillPhaseBUsed;
    private bool aiSkillPhaseAUsed;
    private bool aiSkillPhaseBUsed;

    private bool playerReversalActive;
    private bool aiReversalActive;

    private int playerConsecutiveLosses;
    private int aiConsecutiveLosses;

    private bool playerReplayingCard;
    private int playerLastDrawnCard = -1; // for AI Phase 4 encoding

    // Skill phase waiting flags
    private bool waitingForSkillPhaseA;
    private bool waitingForSkillPhaseB;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        publicPool = new CardType[3];

        if (continueBtn != null)
            continueBtn.onClick.AddListener(OnContinueClick);
        if (exitBtn != null)
            exitBtn.onClick.AddListener(OnExitClick);
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (skillHandDisplay != null)
        {
            skillHandDisplay.OnSkillUsed = OnPlayerUseSkill;
            skillHandDisplay.OnSkipped = OnPlayerSkipSkill;
        }
        enableIntroduce = false;
        cardIntroduceBoard.SetActive(false);
        StartGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentState == GameState.GameOver) return;

            if (currentState == GameState.Paused)
                OnContinueClick();
            else
                Pause();
        }
    }

    private void Pause()
    {
        currentState = GameState.Paused;
        if (pausePanel != null) pausePanel.SetActive(true);
        handDisplay.SetInteractable(false);
        uiManager.SetPoolInteractable(false);
        if (skillHandDisplay != null)
            skillHandDisplay.SetInteractable(false);
    }

    private void OnContinueClick()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        if (waitingForSkillPhaseA)
            currentState = GameState.WaitingForSkillPhaseA;
        else if (waitingForSkillPhaseB)
            currentState = GameState.WaitingForSkillPhaseB;
        else if (waitingForPoolSelection)
            currentState = GameState.WaitingForPoolDraw;
        else
            currentState = GameState.WaitingForPlay;

        handDisplay.SetInteractable(currentState == GameState.WaitingForPlay);
        uiManager.SetPoolInteractable(currentState == GameState.WaitingForPoolDraw);
        if (skillHandDisplay != null)
            skillHandDisplay.SetInteractable(
                currentState == GameState.WaitingForSkillPhaseA ||
                currentState == GameState.WaitingForSkillPhaseB);
    }

    private void OnExitClick()
    {
        SceneManager.LoadScene("ModeSelect");
    }

    public void StartGame()
    {
        deck.Clear();
        for (int i = 0; i < 10; i++) deck.Add(CardType.Rock);
        for (int i = 0; i < 10; i++) deck.Add(CardType.Scissors);
        for (int i = 0; i < 10; i++) deck.Add(CardType.Paper);
        ShuffleDeck();

        InitSkillDeck();

        playerHand.Clear();
        playerHand.Add(CardType.Rock);
        playerHand.Add(CardType.Scissors);
        playerHand.Add(CardType.Paper);

        aiHand.Clear();
        aiHand.Add(CardType.Rock);
        aiHand.Add(CardType.Scissors);
        aiHand.Add(CardType.Paper);

        playerSkills.Clear();
        aiSkills.Clear();
        DrawInitialSkills();

        for (int i = 0; i < 3; i++) publicPool[i] = CardType.None;
        RefillAllPool();

        playerScore = 0;
        aiScore = 0;
        roundNumber = 0;
        playerPicksFirstOnDraw = true;
        playerPlayedCard = CardType.None;
        aiPlayedCard = CardType.None;

        playerConsecutiveLosses = 0;
        aiConsecutiveLosses = 0;
        playerReplayingCard = false;

        uiManager.UpdatePool(publicPool);
        uiManager.UpdateScores(playerScore, aiScore);
        uiManager.UpdateDeckCount(deck.Count);
        uiManager.HideResult();

        if (opponentHandDisplay != null)
            opponentHandDisplay.UpdateHand(aiHand);

        StartRound();
    }

    private void InitSkillDeck()
    {
        skillDeck.Clear();
        for (int i = 0; i < 4; i++) skillDeck.Add(SkillType.StonePower);
        for (int i = 0; i < 4; i++) skillDeck.Add(SkillType.ClothGuard);
        for (int i = 0; i < 4; i++) skillDeck.Add(SkillType.ScissorEdge);
        for (int i = 0; i < 2; i++) skillDeck.Add(SkillType.LastMinuteChange);
        for (int i = 0; i < 2; i++) skillDeck.Add(SkillType.PolarityReversal);
        ShuffleSkillDeck();
    }

    private void ShuffleSkillDeck()
    {
        for (int i = skillDeck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            SkillType temp = skillDeck[i];
            skillDeck[i] = skillDeck[j];
            skillDeck[j] = temp;
        }
    }

    private SkillType? DrawSkillCard()
    {
        if (skillDeck.Count == 0) return null;
        SkillType skill = skillDeck[0];
        skillDeck.RemoveAt(0);
        return skill;
    }

    private void DrawInitialSkills()
    {
        SkillType? ps = DrawSkillCard();
        if (ps.HasValue)
        {
            playerSkills.Add(ps.Value);
            Debug.Log("玩家初始技能: " + SkillData.GetName(ps.Value));
        }

        SkillType? ais = DrawSkillCard();
        if (ais.HasValue)
        {
            aiSkills.Add(ais.Value);
            Debug.Log("AI初始技能: " + SkillData.GetName(ais.Value));
        }

        UpdateSkillCountUI();
    }

    private void GivePlayerSkill()
    {
        if (playerSkills.Count >= 3) return;
        SkillType? s = DrawSkillCard();
        if (s.HasValue)
        {
            playerSkills.Add(s.Value);
            uiManager.AppendStatus(" [获得技能: " + SkillData.GetName(s.Value) + "]");
            Debug.Log("玩家获得技能: " + SkillData.GetName(s.Value));
        }
        UpdateSkillCountUI();
    }

    private void GiveAISkill()
    {
        if (aiSkills.Count >= 3) return;
        SkillType? s = DrawSkillCard();
        if (s.HasValue)
        {
            aiSkills.Add(s.Value);
            Debug.Log("AI获得技能: " + SkillData.GetName(s.Value));
        }
        uiManager.UpdateOpponentSkillCount(aiSkills.Count);
    }

    private void UpdateSkillCountUI()
    {
        string names = "";
        foreach (SkillType s in playerSkills)
        {
            if (names.Length > 0) names += " ";
            names += "[" + SkillData.GetName(s) + "]";
        }
        uiManager.UpdateSkillCount(playerSkills.Count, names);
        uiManager.UpdateOpponentSkillCount(aiSkills.Count);

        if (playerSkillBar != null)
            playerSkillBar.UpdateSkills(playerSkills);
    }

    private void StartRound()
    {
        roundNumber++;
        playerPlayedCard = CardType.None;
        aiPlayedCard = CardType.None;

        // Reset per-round skill state
        playerSkillPhaseA = null;
        playerSkillPhaseB = null;
        aiSkillPhaseA = null;
        aiSkillPhaseB = null;
        playerSkillPhaseAUsed = false;
        playerSkillPhaseBUsed = false;
        aiSkillPhaseAUsed = false;
        aiSkillPhaseBUsed = false;
        playerReversalActive = false;
        aiReversalActive = false;
        playerReplayingCard = false;

        uiManager.UpdateRound(roundNumber);
        uiManager.UpdateDeckCount(deck.Count);
        uiManager.ShowPlayerPlayedCard(false);
        uiManager.ShowAIPlayedCard(false);
        uiManager.SetPoolInteractable(false);

        handDisplay.UpdateHand(playerHand);
        handDisplay.SetInteractable(false);

        if (opponentHandDisplay != null)
            opponentHandDisplay.UpdateHand(aiHand);

        StartCoroutine(SkillPhaseA());
    }

    // ==================== Skill Phase A (before play) ====================

    private IEnumerator SkillPhaseA()
    {
        currentState = GameState.WaitingForSkillPhaseA;
        waitingForSkillPhaseA = true;

        // AI decides on Phase A skill
        aiSkillPhaseA = AIDecidePhaseASkill();
        uiManager.UpdateOpponentSkillCount(aiSkills.Count);

        List<SkillType> usablePhaseA = playerSkills.FindAll(s => SkillData.IsPhaseA(s));
        if (skillHandDisplay != null)
        {
            uiManager.SetStatus("阶段A：选择出牌前技能（可选）");
            skillHandDisplay.ShowPhaseA(playerSkills, usablePhaseA.Count > 0);
            skillHandDisplay.SetInteractable(true);

            while (waitingForSkillPhaseA)
                yield return null;

            skillHandDisplay.Hide();
        }
        else
        {
            uiManager.SetStatus("阶段A：跳过（未配置技能UI）");
            yield return new WaitForSeconds(0.6f);
        }

        waitingForSkillPhaseA = false;
        playerSkillPhaseAUsed = playerSkillPhaseA.HasValue;
        aiSkillPhaseAUsed = aiSkillPhaseA.HasValue;

        // Reveal Phase A skills before card play
        yield return StartCoroutine(RevealPhaseASkills());

        // Now move to play phase
        currentState = GameState.WaitingForPlay;
        handDisplay.SetInteractable(true);
        uiManager.SetStatus("请选择一张手牌");
    }

    private IEnumerator RevealPhaseASkills()
    {
        string playerMsg = playerSkillPhaseAUsed
            ? "你使用了 [" + SkillData.GetName(playerSkillPhaseA.Value) + "]"
            : "你未使用技能";
        string aiMsg = aiSkillPhaseAUsed
            ? "AI使用了 [" + SkillData.GetName(aiSkillPhaseA.Value) + "]"
            : "AI未使用技能";

        uiManager.SetStatus("阶段A结算: " + playerMsg + " | " + aiMsg);
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator RevealPhaseBSkills()
    {
        string playerMsg = "";
        string aiMsg = "";

        if (playerSkillPhaseB == SkillType.PolarityReversal)
            playerMsg = "你使用了 [两极反转]";
        else if (playerSkillPhaseB == SkillType.LastMinuteChange)
            playerMsg = "你使用了 [临阵换策]";
        else
            playerMsg = "你未使用技能";

        if (aiSkillPhaseB == SkillType.PolarityReversal)
            aiMsg = "AI使用了 [两极反转]";
        else if (aiSkillPhaseB == SkillType.LastMinuteChange)
            aiMsg = "AI使用了 [临阵换策]";
        else
            aiMsg = "AI未使用技能";

        uiManager.SetStatus("阶段B结算: " + playerMsg + " | " + aiMsg);
        yield return new WaitForSeconds(1.0f);
    }

    private void OnPlayerUseSkill(SkillType skillType)
    {
        if (currentState == GameState.WaitingForSkillPhaseA)
        {
            playerSkillPhaseA = skillType;
            playerSkills.Remove(skillType);
            waitingForSkillPhaseA = false;
        }
        else if (currentState == GameState.WaitingForSkillPhaseB)
        {
            playerSkillPhaseB = skillType;
            playerSkills.Remove(skillType);
            waitingForSkillPhaseB = false;
        }
        UpdateSkillCountUI();
    }

    private void OnPlayerSkipSkill()
    {
        if (currentState == GameState.WaitingForSkillPhaseA)
        {
            waitingForSkillPhaseA = false;
        }
        else if (currentState == GameState.WaitingForSkillPhaseB)
        {
            waitingForSkillPhaseB = false;
        }
    }

    // ==================== Play Card Phase ====================

    public void OnPlayerPlayCard(CardType cardType)
    {
        if (currentState == GameState.Paused) return;
        if (currentState != GameState.WaitingForPlay) return;
        if (playerReplayingCard)
        {
            // This is a re-play after 临阵换策
            playerPlayedCard = cardType;
            playerHand.Remove(cardType);
            playerReplayingCard = false;
            handDisplay.SetInteractable(false);
            handDisplay.UpdateHand(playerHand);
            uiManager.ShowPlayerPlayedCard(true, playerPlayedCard);
            return;
        }

        playerPlayedCard = cardType;
        playerHand.Remove(cardType);
        handDisplay.SetInteractable(false);
        handDisplay.UpdateHand(playerHand);

        // AI card selection
        if (neuralAI != null && neuralAI.IsLoaded)
        {
            List<int> uniqueCards = new List<int>();
            foreach (CardType c in aiHand)
                if (!uniqueCards.Contains((int)c))
                    uniqueCards.Add((int)c);
            // Pass player's full pre-play hand (3 cards) — Python training
            // decides both cards simultaneously with full hands
            List<CardType> playerFullHand = new List<CardType>(playerHand);
            playerFullHand.Add(playerPlayedCard);
            aiPlayedCard = neuralAI.DecideCard(
                new List<CardType>(aiHand), playerFullHand,
                aiScore, playerScore, aiConsecutiveLosses,
                aiSkillPhaseAUsed, aiSkillPhaseA, playerSkillPhaseA,
                playerSkillPhaseAUsed,
                uniqueCards.ToArray());
            aiHand.Remove(aiPlayedCard);
        }
        else
        {
            int aiIdx = Random.Range(0, aiHand.Count);
            aiPlayedCard = aiHand[aiIdx];
            aiHand.RemoveAt(aiIdx);
        }

        if (opponentHandDisplay != null)
            opponentHandDisplay.UpdateHand(aiHand);

        // Reveal both cards before Phase B
        uiManager.ShowPlayerPlayedCard(true, playerPlayedCard);
        uiManager.ShowAIPlayedCard(true, aiPlayedCard);

        StartCoroutine(SkillPhaseB());
    }

    // ==================== Skill Phase B (after play, before reveal) ====================

    private IEnumerator SkillPhaseB()
    {
        currentState = GameState.WaitingForSkillPhaseB;
        waitingForSkillPhaseB = true;

        // AI decides on Phase B skill (each phase is independent)
        aiSkillPhaseB = AIDecidePhaseBSkill();
        uiManager.UpdateOpponentSkillCount(aiSkills.Count);

        List<SkillType> usablePhaseB = playerSkills.FindAll(s => SkillData.IsPhaseB(s));
        if (skillHandDisplay != null)
        {
            uiManager.SetStatus("阶段B：选择出牌后技能（可选）");
            skillHandDisplay.ShowPhaseB(playerSkills, usablePhaseB.Count > 0);
            skillHandDisplay.SetInteractable(true);

            while (waitingForSkillPhaseB)
                yield return null;

            skillHandDisplay.Hide();
        }
        else
        {
            uiManager.SetStatus("阶段B：跳过（未配置技能UI）");
            yield return new WaitForSeconds(0.6f);
        }

        waitingForSkillPhaseB = false;
        playerSkillPhaseBUsed = playerSkillPhaseB.HasValue;
        aiSkillPhaseBUsed = aiSkillPhaseB.HasValue;

        // Reveal Phase B skills before processing effects
        yield return StartCoroutine(RevealPhaseBSkills());

        // Handle player's Phase B skill effects
        if (playerSkillPhaseB == SkillType.LastMinuteChange)
        {
            playerHand.Add(playerPlayedCard);
            playerPlayedCard = CardType.None;
            playerReplayingCard = true;

            handDisplay.UpdateHand(playerHand);
            handDisplay.SetInteractable(true);
            currentState = GameState.WaitingForPlay;
            uiManager.SetStatus("临阵换策：请重新选择一张手牌");

            while (playerReplayingCard)
                yield return null;

            handDisplay.SetInteractable(false);
        }

        if (playerSkillPhaseB == SkillType.PolarityReversal)
        {
            playerReversalActive = true;
        }

        // Handle AI's Phase B skill effects
        if (aiSkillPhaseB == SkillType.LastMinuteChange)
        {
            aiHand.Add(aiPlayedCard);
            int newAiIdx = Random.Range(0, aiHand.Count);
            aiPlayedCard = aiHand[newAiIdx];
            aiHand.RemoveAt(newAiIdx);

            if (opponentHandDisplay != null)
                opponentHandDisplay.UpdateHand(aiHand);
        }

        if (aiSkillPhaseB == SkillType.PolarityReversal)
        {
            aiReversalActive = true;
        }

        StartCoroutine(ResolveRound());
    }

    // ==================== Resolve Round ====================

    private IEnumerator ResolveRound()
    {
        currentState = GameState.Revealing;

        // Cards are already visible from before Phase B
        yield return new WaitForSeconds(1.2f);

        int result = CompareCards(playerPlayedCard, aiPlayedCard);

        // Calculate scores with skill effects
        int playerRoundScore = 0;
        int aiRoundScore = 0;

        if (result > 0)
        {
            playerRoundScore = GetPlayerScoreWithBonus(playerPlayedCard);
            aiRoundScore = 0;
        }
        else if (result < 0)
        {
            playerRoundScore = 0;
            aiRoundScore = GetAIScoreWithBonus(aiPlayedCard);
        }

        // Handle polarity reversal
        bool playerRev = playerReversalActive && playerSkillPhaseB == SkillType.PolarityReversal;
        bool aiRev = aiReversalActive && aiSkillPhaseB == SkillType.PolarityReversal;
        int reversalCount = (playerRev ? 1 : 0) + (aiRev ? 1 : 0);

        if (reversalCount == 1)
        {
            int temp = playerRoundScore;
            playerRoundScore = aiRoundScore;
            aiRoundScore = temp;
            if (playerRoundScore > aiRoundScore) result = 1;
            else if (aiRoundScore > playerRoundScore) result = -1;
            else result = 0;
        }

        playerScore += playerRoundScore;
        aiScore += aiRoundScore;

        // Update consecutive losses
        if (result > 0)
        {
            playerConsecutiveLosses = 0;
            aiConsecutiveLosses++;
            uiManager.SetStatus("你赢了这回合！+" + playerRoundScore + " 分");
        }
        else if (result < 0)
        {
            playerConsecutiveLosses++;
            aiConsecutiveLosses = 0;
            uiManager.SetStatus("AI 赢了这回合！AI +" + aiRoundScore + " 分");
        }
        else
        {
            uiManager.SetStatus("平局！");
        }

        uiManager.UpdateScores(playerScore, aiScore);

        // Show skill effects in status
        string skillInfo = BuildSkillInfoText();
        if (!string.IsNullOrEmpty(skillInfo))
            uiManager.AppendStatus(skillInfo);

        yield return new WaitForSeconds(1.5f);

        if (playerScore >= winScore || aiScore >= winScore)
        {
            EndGame();
            yield break;
        }

        yield return StartCoroutine(PoolDrawPhase(result));
    }

    private int GetPlayerScoreWithBonus(CardType playedCard)
    {
        if (playerSkillPhaseA.HasValue)
        {
            CardType bonusType = SkillData.GetBonusCardType(playerSkillPhaseA.Value);
            if (bonusType == playedCard && bonusType != CardType.None)
                return 2;
        }
        return 1;
    }

    private int GetAIScoreWithBonus(CardType playedCard)
    {
        if (aiSkillPhaseA.HasValue)
        {
            CardType bonusType = SkillData.GetBonusCardType(aiSkillPhaseA.Value);
            if (bonusType == playedCard && bonusType != CardType.None)
                return 2;
        }
        return 1;
    }

    private string BuildSkillInfoText()
    {
        string info = "";
        if (playerSkillPhaseA.HasValue)
            info += " [你使用了" + SkillData.GetName(playerSkillPhaseA.Value) + "]";
        if (playerSkillPhaseB.HasValue && playerSkillPhaseB == SkillType.PolarityReversal)
            info += " [你使用了" + SkillData.GetName(playerSkillPhaseB.Value) + "]";
        if (aiSkillPhaseA.HasValue)
            info += " [AI使用了" + SkillData.GetName(aiSkillPhaseA.Value) + "]";
        if (aiSkillPhaseB.HasValue)
            info += " [AI使用了" + SkillData.GetName(aiSkillPhaseB.Value) + "]";
        return info;
    }

    private int CompareCards(CardType a, CardType b)
    {
        if (a == b) return 0;
        if ((a == CardType.Rock && b == CardType.Scissors) ||
            (a == CardType.Scissors && b == CardType.Paper) ||
            (a == CardType.Paper && b == CardType.Rock))
            return 1;
        return -1;
    }

    // ==================== Pool Draw Phase ====================

    private IEnumerator PoolDrawPhase(int roundResult)
    {
        // Consecutive loss reward
        if (playerConsecutiveLosses >= 2)
        {
            GivePlayerSkill();
            uiManager.SetStatus("连败奖励：你获得一张技能牌！");
            yield return new WaitForSeconds(1.2f);
        }
        if (aiConsecutiveLosses >= 2)
        {
            GiveAISkill();
        }

        bool playerFirst;
        if (roundResult > 0) playerFirst = false;
        else if (roundResult < 0) playerFirst = true;
        else { playerFirst = playerPicksFirstOnDraw; playerPicksFirstOnDraw = !playerPicksFirstOnDraw; }

        playerLastDrawnCard = -1;

        for (int i = 0; i < 2; i++)
        {
            bool isPlayerTurn = (i == 0) ? playerFirst : !playerFirst;

            if (isPlayerTurn)
            {
                currentState = GameState.WaitingForPoolDraw;
                waitingForPoolSelection = true;
                uiManager.SetStatus("请从公共牌池选择一张牌");
                uiManager.SetPoolInteractable(true);

                while (waitingForPoolSelection)
                    yield return null;

                uiManager.SetPoolInteractable(false);

                CardType drawn = publicPool[selectedPoolIndex];
                playerLastDrawnCard = (int)drawn;
                playerHand.Add(drawn);
                handDisplay.UpdateHand(playerHand);
                RefillPoolSlot(selectedPoolIndex);
                uiManager.UpdatePool(publicPool);
                uiManager.UpdateDeckCount(deck.Count);
            }
            else
            {
                currentState = GameState.Revealing;
                uiManager.SetStatus("AI 正在从牌池选牌...");

                yield return new WaitForSeconds(0.8f);

                int pick;
                if (neuralAI != null && neuralAI.IsLoaded)
                {
                    pick = neuralAI.DecidePoolDraw(
                        new List<CardType>(aiHand), new List<CardType>(playerHand),
                        publicPool, new List<SkillType>(aiSkills), playerLastDrawnCard);
                }
                else
                {
                    List<int> valid = new List<int>();
                    for (int j = 0; j < publicPool.Length; j++)
                        if (publicPool[j] != CardType.None) valid.Add(j);
                    pick = valid.Count > 0 ? valid[Random.Range(0, valid.Count)] : 0;
                }

                if (publicPool[pick] != CardType.None)
                {
                    aiHand.Add(publicPool[pick]);
                    RefillPoolSlot(pick);
                    uiManager.UpdatePool(publicPool);
                    uiManager.UpdateDeckCount(deck.Count);
                    if (opponentHandDisplay != null)
                        opponentHandDisplay.UpdateHand(aiHand);
                }
            }
        }

        StartRound();
    }

    public void OnPoolCardClicked(int index)
    {
        if (currentState != GameState.WaitingForPoolDraw || !waitingForPoolSelection) return;
        if (publicPool[index] == CardType.None) return;
        selectedPoolIndex = index;
        waitingForPoolSelection = false;
    }

    private void RefillAllPool()
    {
        for (int i = 0; i < 3; i++)
            RefillPoolSlot(i);
    }

    private void RefillPoolSlot(int index)
    {
        if (deck.Count == 0) return;
        publicPool[index] = deck[0];
        deck.RemoveAt(0);
    }

    private void ShuffleDeck()
    {
        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            CardType temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }

    // ==================== AI Skill Logic ====================

    private SkillType? AIDecidePhaseASkill()
    {
        if (neuralAI != null && neuralAI.IsLoaded)
        {
            SkillType? chosen = neuralAI.DecideSkillPhaseA(
                new List<CardType>(aiHand), new List<CardType>(playerHand),
                aiScore, playerScore, aiConsecutiveLosses,
                new List<SkillType>(aiSkills), aiSkillPhaseAUsed);
            if (chosen.HasValue)
                aiSkills.Remove(chosen.Value);
            return chosen;
        }

        // Fallback: simple heuristic
        List<SkillType> usable = aiSkills.FindAll(s => SkillData.IsPhaseA(s));
        if (usable.Count == 0) return null;

        foreach (SkillType skill in usable)
        {
            CardType bonusType = SkillData.GetBonusCardType(skill);
            if (bonusType != CardType.None && aiHand.Contains(bonusType))
            {
                if (Random.value < 0.6f)
                {
                    aiSkills.Remove(skill);
                    return skill;
                }
            }
        }
        if (Random.value < 0.2f && usable.Count > 0)
        {
            SkillType selected = usable[Random.Range(0, usable.Count)];
            aiSkills.Remove(selected);
            return selected;
        }
        return null;
    }

    private SkillType? AIDecidePhaseBSkill()
    {
        if (neuralAI != null && neuralAI.IsLoaded)
        {
            SkillType? chosen = neuralAI.DecideSkillPhaseB(
                new List<CardType>(aiHand), new List<CardType>(playerHand),
                aiScore, playerScore, aiConsecutiveLosses,
                aiPlayedCard, playerPlayedCard,
                aiSkillPhaseAUsed, aiSkillPhaseA, playerSkillPhaseA,
                playerSkillPhaseAUsed,
                new List<SkillType>(aiSkills));
            if (chosen.HasValue)
                aiSkills.Remove(chosen.Value);
            return chosen;
        }

        // Fallback: simple heuristic
        List<SkillType> usable = aiSkills.FindAll(s => SkillData.IsPhaseB(s));
        if (usable.Count == 0) return null;

        if (usable.Contains(SkillType.PolarityReversal) && aiScore < playerScore)
        {
            if (Random.value < 0.4f)
            {
                aiSkills.Remove(SkillType.PolarityReversal);
                return SkillType.PolarityReversal;
            }
        }
        if (usable.Contains(SkillType.LastMinuteChange))
        {
            if (Random.value < 0.3f)
            {
                aiSkills.Remove(SkillType.LastMinuteChange);
                return SkillType.LastMinuteChange;
            }
        }
        return null;
    }

    // ==================== End Game ====================

    private void EndGame()
    {
        currentState = GameState.GameOver;
        handDisplay.SetInteractable(false);
        uiManager.SetPoolInteractable(false);
        if (skillHandDisplay != null)
            skillHandDisplay.Hide();

        string res;
        if (playerScore > aiScore) res = "你赢了！";
        else if (aiScore > playerScore) res = "你输了！";
        else res = "平局！";

        uiManager.ShowResult(res, playerScore, aiScore);
        uiManager.SetStatus("游戏结束");
    }

    public void RestartGame()
    {
        StopAllCoroutines();
        StartGame();
    }

    public void ExitButton(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }


    public void SkillIntroduceButton()
    {
        enableIntroduce = !enableIntroduce;
        cardIntroduceBoard.SetActive(enableIntroduce);
    } 
}
