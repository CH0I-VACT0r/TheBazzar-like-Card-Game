using UnityEngine;
using System.Collections.Generic;

// (기존 Weekday 유지)
public enum Weekday
{
    Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
}

public class GameManager : MonoBehaviour
{
    // --- 참조 ---
    public BattleManager battleManager;

    // --- 게임 상태 ---
    public int CurrentDay { get; private set; } = 1;
    public int PlayerLevel { get; private set; } = 1; // 플레이어 레벨 (명성)

    public enum GamePhase { Preparation, Battle, Reward, DayEnd }
    public GamePhase currentPhase = GamePhase.Preparation;

    // 싱글톤
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        SetPhase(GamePhase.Preparation);

        // 첫날 이벤트 트리거 (랜덤 선택창)
        TriggerDailyEventSelection();
    }

    // --- 헬퍼 함수 ---
    private Weekday GetCurrentWeekday()
    {
        int dayIndex = (CurrentDay - 1) % 7;
        return (Weekday)dayIndex;
    }

    // 레벨업 함수 (나중에 경험치 찼을 때 호출)
    public void LevelUp()
    {
        PlayerLevel++;
        Debug.Log($"레벨 업! 현재 레벨: {PlayerLevel}");
        // TODO: 레벨업 축하 UI 띄우기
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentPhase == GamePhase.Preparation)
            {
                Debug.Log("Test] 스페이스바: 전투 시작!");
                SetPhase(GamePhase.Battle);
            }
            else if (currentPhase == GamePhase.Reward)
            {
                Debug.Log("[Test] 다음 날로 진행");
                StartNextDay();
            }
        }

        // [테스트용] L키 누르면 레벨업
        if (Input.GetKeyDown(KeyCode.L)) LevelUp();
    }

    // --- 페이즈 관리  ---
    public void SetPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;

        if (UIManager.Instance != null)
        {
            switch (newPhase)
            {
                case GamePhase.Preparation:
                    battleManager.IsDeckEditingAllowed = true;
                    UIManager.Instance.SetBattleState(false); // 인벤토리 열림
                    Debug.Log($"--- Day {CurrentDay} ({GetCurrentWeekday()}) 정비 단계 ---");
                    break;

                case GamePhase.Battle:
                    battleManager.IsDeckEditingAllowed = false;
                    if (UIManager.Instance.IsInventoryOpen) UIManager.Instance.CloseInventory();
                    UIManager.Instance.SetBattleState(true); // 인벤토리 잠김
                    Debug.Log("--- 전투 시작! ---");
                    break;

                case GamePhase.Reward:
                    UIManager.Instance.SetBattleState(false);
                    Debug.Log("--- 전투 종료: 보상 ---");
                    break;

                case GamePhase.DayEnd:
                    Debug.Log("--- 하루 종료 ---");
                    break;
            }
        }
    }

    public void StartNextDay()
    {
        CurrentDay++;
        Weekday today = GetCurrentWeekday();
        Debug.Log($"--- Day {CurrentDay} ({today}) 시작 ---");

        SetPhase(GamePhase.Preparation);

        // 3가지 랜덤 이벤트 선택지
        TriggerDailyEventSelection();
    }

    // --- 레벨별 카드 등급 뽑기 (가챠 로직) ---
    public CardRarity GetRandomRarityByLevel()
    {
        int roll = Random.Range(0, 100); // 0 ~ 99 

        // 레벨별 확률
        // Lv 1 ~ 2: 브론즈 100%
        if (PlayerLevel < 3)
        {
            return CardRarity.Bronze;
        }
        // Lv 3 ~ 4: 브론즈 50%, 실버 50%
        else if (PlayerLevel < 5)
        {
            if (roll < 50) return CardRarity.Bronze;
            else return CardRarity.Silver;
        }
        // Lv 5 ~ 7: 실버 100%
        else if (PlayerLevel < 8)
        {
            return CardRarity.Silver;
        }
        // Lv 8 ~ 9: 실버 70%, 골드 30% 
        else if (PlayerLevel < 10)
        {
            if (roll < 70) return CardRarity.Silver; 
            else return CardRarity.Gold;            
        }
        // Lv 10 ~ 11: 실버 30%, 골드 70%
        else if (PlayerLevel < 12)
        {
            if (roll < 30) return CardRarity.Silver;
            else return CardRarity.Gold;
        }
        // Lv 12 ~ 14: 골드 100%
        else if (PlayerLevel < 15)
        {
            return CardRarity.Gold;
        }
        // Lv 15+: 골드 80%, 다이아 20%
        else
        {
            if (roll < 80) return CardRarity.Gold;
            else return CardRarity.Diamond;
        }
    }

    // 이벤트 선택
    private void TriggerDailyEventSelection()
    {
        Debug.Log($"Day {CurrentDay}: 오늘의 랜덤 이벤트 3개를 선정합니다. (Lv.{PlayerLevel})");

        List<GameEvent> dailyEvents = new List<GameEvent>();

        for (int i = 0; i < 3; i++)
        {
            CardRarity rarity = GetRandomRarityByLevel();
            GameEvent evt = EventManager.Instance.GetRandomEventByRarity(rarity);

            if (evt != null)
            {
                dailyEvents.Add(evt);
                Debug.Log($"   - 선택지 {i + 1}: [{evt.rarity}] {evt.eventID}");
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEventSelectionWindow(dailyEvents);
        }
    }
}