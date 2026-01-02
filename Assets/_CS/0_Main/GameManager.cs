using UnityEngine;
using System.Collections.Generic;


// 요일 열거형
public enum Weekday
{
    Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // --- 참조 ---
    public BattleManager battleManager;

    [Header("Stage Settings")]
    public string[] stageNameKeys = { "stage_name_01", "stage_name_02", "stage_name_03", "stage_name_04" };

    [Header("Progression History")]
    private List<string> usedSundayEventIDs = new List<string>(); // 이번 스테이지에서 사용한 일요일 이벤트 ID

    // --- 게임 진행 상태 ---
    [Header("Game Progression")]
    public int currentStage = 1;       // 현재 스테이지 (예: 1지역, 2지역...)
    public int currentWeek = 1;        // 현재 주차 (1~4주)
    public int currentDayInWeek = 1;   // 현재 요일 수치 (1:월 ~ 7:일)

    public enum GamePhase { Preparation, Battle, Reward, DayEnd }
    public GamePhase currentPhase = GamePhase.Preparation;

    private bool m_IsTransitioning = false; // 중복 실행 방지용

    public LordType currentLord = LordType.SevereCold;

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

        // 초기 페이즈 설정
        SetPhase(GamePhase.Preparation);

        // 게임 시작 시 HUD 정보 첫 업데이트
        UpdateStageUI();

        // 첫날 이벤트 트리거
        TriggerDailyEventSelection();
    }

    // --- 헬퍼 함수: 현재 요일 반환 ---
    public Weekday GetCurrentWeekday()
    {
        // 1(월) -> index 0, ..., 7(일) -> index 6
        return (Weekday)(currentDayInWeek - 1);
    }

    void Update()
    {
        // [테스트용] 스페이스바 페이즈 전환
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    if (currentPhase == GamePhase.Preparation)
        //    {
        //        SetPhase(GamePhase.Battle);
        //    }
        //    else if (currentPhase == GamePhase.Reward)
        //    {
        //        StartNextDay();
        //    }
        //}

        // [테스트용] L키 레벨업 테스트 (PlayerController의 경험치 시스템 이용)
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    if (battleManager != null && battleManager.playerController != null)
        //    {
        //        battleManager.playerController.AddExperience(10);
        //    }
        //}
    }

    // --- 페이즈 관리 ---
    public void SetPhase(GamePhase newPhase)
    {
        currentPhase = newPhase;

        if (UIManager.Instance != null)
        {
            switch (newPhase)
            {
                case GamePhase.Preparation:
                    battleManager.IsDeckEditingAllowed = true;
                    UIManager.Instance.SetBattleState(false);
                    Debug.Log($"--- Day {currentDayInWeek} ({GetCurrentWeekday()}) 정비 단계 ---");
                    break;

                case GamePhase.Battle:
                    battleManager.IsDeckEditingAllowed = false;
                    if (UIManager.Instance.IsInventoryOpen) UIManager.Instance.CloseInventory();
                    UIManager.Instance.SetBattleState(true);
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

    // --- 하루를 마치고 다음 날로 진행 ---
    public void StartNextDay()
    {
        if (m_IsTransitioning) return;
        m_IsTransitioning = true;

        currentDayInWeek++;

        // 7일(일요일)이 지나면 다음 주로 이동
        if (currentDayInWeek > 7)
        {
            currentDayInWeek = 1;
            currentWeek++;
        }

        // 4주차가 종료되면 다음 스테이지로 이동
        if (currentWeek > 4)
        {
            currentWeek = 1;
            currentStage++;
            usedSundayEventIDs.Clear();
            TriggerStageTransition();
        }

        UpdateStageUI();                  // UI에 변경된 날짜/스테이지 정보 반영
        SetPhase(GamePhase.Preparation);  // 정비 단계로 전환
        TriggerDailyEventSelection();     // 새 날의 이벤트 생성

        Invoke(nameof(ResetTransitionFlag), 0.1f); // 플래그 해제
    }
    private void ResetTransitionFlag() => m_IsTransitioning = false;

    private void TriggerStageTransition()
    {
        string msg = $"STAGE {currentStage} 진입\n새로운 등급의 카드들이 해금되었습니다!";
        Debug.LogWarning(msg);

        // UI 로직: 큰 자막으로 화면 중앙에 띄우기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowStageBanner(currentStage, "더 높은 등급의 용병과 장비를 사용할 수 있습니다.");
        }
    }

    // HUD 정보 업데이트 중계
    private void UpdateStageUI()
    {
        if (UIManager.Instance != null)
        {
            // 1. 현재 스테이지에 맞는 키 가져오기
            string key = (currentStage <= stageNameKeys.Length)
                ? stageNameKeys[currentStage - 1]
                : "stage_name_unknown";

            // 2. LocalizationManager를 통해 번역된 텍스트 가져오기
            string localizedStageName = LocalizationManager.GetText(key);

            // 3. UIManager에 전달 (UIManager는 받은 문자열을 그대로 출력)
            UIManager.Instance.UpdateStageInfo(currentStage, localizedStageName, currentWeek, GetCurrentWeekday());
        }
    }

    // --- 스테이지 기반 등급 결정 로직 (가챠 로직) ---
    public CardRarity GetRandomRarityByProgression()
    {
        int roll = Random.Range(0, 100);

        // 스테이지가 올라갈수록 하위 등급은 퇴출되고 상위 등급이 기본이 됨
        switch (currentStage)
        {
            case 1: // 1스테이지: 브론즈 위주, 실버 희귀
                if (roll < 85) return CardRarity.Bronze;
                else return CardRarity.Silver;

            case 2: // 2스테이지: 브론즈와 실버 반반
                if (roll < 50) return CardRarity.Bronze;
                else return CardRarity.Silver;

            case 3: // 3스테이지: 실버 확정 해금 (언급하신 부분)
                    // 여기서부터 "실버 용병이 대거 합류합니다!" 알림 가능
                if (roll < 70) return CardRarity.Silver;
                else return CardRarity.Gold;

            case 4: // 4스테이지 (보스 스테이지 지역): 골드 위주
                if (roll < 40) return CardRarity.Silver;
                else if (roll < 90) return CardRarity.Gold;
                else return CardRarity.Diamond;

            default: // 그 이상의 지옥 난이도
                return CardRarity.Gold;
        }
    }

    // --- 일일 이벤트 선택 생성 ---
    private void TriggerDailyEventSelection()
    {
        List<GameEvent> dailyEvents = new List<GameEvent>();

        List<GameEvent> GetFilteredCandidates(CardRarity rarity)
        {
            List<GameEvent> candidates = EventManager.Instance.GetAllEventsByRarity(rarity);

            // [핵심 필터 1] 현재 영주와 맞지 않는 이벤트 제거 (Common은 통과)
            candidates.RemoveAll(evt => evt.targetLord != LordType.Common && evt.targetLord != currentLord);

            return candidates;
        }

        // 1. [일요일] 고정 전투 (정예/보스)
        if (currentDayInWeek == 7)
        {
            GameEvent specialEvent = null;
            if (currentWeek == 4)
                specialEvent = EventManager.Instance.GetBossEvent(currentStage);
            else
                specialEvent = EventManager.Instance.GetSundayMonsterEvent(currentStage, usedSundayEventIDs);

            if (specialEvent != null)
            {
                dailyEvents.Add(specialEvent);
                if (currentWeek != 4) usedSundayEventIDs.Add(specialEvent.eventID);
            }
        }
        // 매 스테이지 1주차 월요일: 무조건 상점 이벤트만 등장
        else if (currentWeek == 1 && currentDayInWeek == 1)
        {
            // 상점 중에서도 영주 전용 상점 또는 공용 상점만 필터링
            List<GameEvent> shopCandidates = EventManager.Instance.allEvents.FindAll(e =>
                e.eventType == EventType.Shop &&
                (e.targetLord == LordType.Common || e.targetLord == currentLord));

            for (int i = 0; i < 3; i++)
            {
                if (shopCandidates.Count > 0)
                {
                    int randomIndex = Random.Range(0, shopCandidates.Count);
                    dailyEvents.Add(shopCandidates[randomIndex]);
                }
            }
        }
        // [월~토] 일반적인 랜덤 이벤트 (전투 제외, 의뢰 포함)
        else
        {
            for (int i = 0; i < 3; i++)
            {
                CardRarity rarity = GetRandomRarityByProgression();
                List<GameEvent> candidates = GetFilteredCandidates(rarity); // 필터 적용된 후보 가져오기

                candidates.RemoveAll(evt => evt.eventType == EventType.Battle);
                candidates.RemoveAll(evt => dailyEvents.Contains(evt));

                if (candidates.Count > 0)
                    dailyEvents.Add(candidates[Random.Range(0, candidates.Count)]);
            }
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEventSelectionWindow(dailyEvents);
        }

            // UI에 이벤트 목록 전달 (일요일은 리스트에 1개만 담김)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowEventSelectionWindow(dailyEvents);
        }
    }
}