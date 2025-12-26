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
    // 싱글톤
    public static GameManager Instance;

    // --- 게임 상태 ---
    public int CurrentDay { get; private set; } = 1;

    public enum GamePhase { Preparation, Battle, Reward, DayEnd }
    public GamePhase currentPhase = GamePhase.Preparation;

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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentPhase == GamePhase.Preparation)
            {
                SetPhase(GamePhase.Battle);
            }
            else if (currentPhase == GamePhase.Reward)
            {
                StartNextDay();
            }
        }

        // [테스트용] L키 누르면 플레이어에게 경험치 10을 줘서 레벨업 테스트
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (battleManager != null && battleManager.playerController != null)
            {
                battleManager.playerController.AddExperience(10);
                Debug.Log("[Test] 경험치 10 추가 시도");
            }
        }
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
        // 안전장치: 플레이어 컨트롤러가 없으면 기본 등급 반환
        if (battleManager == null || battleManager.playerController == null)
            return CardRarity.Bronze;

        // 플레이어의 현재 레벨 가져오기
        int currentLv = battleManager.playerController.CurrentLevel;
        int roll = Random.Range(0, 100);

        // Lv 1 ~ 2: 브론즈 100%
        if (currentLv < 3) // PlayerLevel 대신 currentLv 사용
        {
            return CardRarity.Bronze;
        }
        // Lv 3 ~ 4: 브론즈 50%, 실버 50%
        else if (currentLv < 5)
        {
            if (roll < 50) return CardRarity.Bronze;
            else return CardRarity.Silver;
        }
        // ... (이하 모든 PlayerLevel을 currentLv로 변경) ...

        // 마지막 Diamond 확률 구간 예시
        else
        {
            if (roll < 80) return CardRarity.Gold;
            else return CardRarity.Diamond;
        }
    }

    // 이벤트 선택
    private void TriggerDailyEventSelection()
    {
        List<GameEvent> dailyEvents = new List<GameEvent>();

        for (int i = 0; i < 3; i++)
        {
            // 레벨에 따른 등급 결정
            CardRarity rarity = GetRandomRarityByLevel();

            if (EventManager.Instance != null)
            {
                // 해당 등급의 '모든' 후보군 가져오기
                List<GameEvent> candidates = EventManager.Instance.GetAllEventsByRarity(rarity);

                // 중복 방지
                candidates.RemoveAll(evt => dailyEvents.Contains(evt));

                // 이벤트 개수 부족 시 중복 허용
                if (candidates.Count == 0)
                {
                    candidates = EventManager.Instance.GetAllEventsByRarity(rarity);
                }

                if (candidates.Count > 0)
                {
                    int randomIndex = Random.Range(0, candidates.Count);
                    GameEvent selectedEvent = candidates[randomIndex];

                    dailyEvents.Add(selectedEvent);
                }
            }
            else
            {
                Debug.LogError("[GameManager] EventManager가 씬에 없습니다");
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEventSelectionWindow(dailyEvents);
        }
    }


}