using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    // 유니티 에디터에서 만든 이벤트 파일들
    [Header("모든 이벤트 데이터베이스")]
    public List<GameEvent> allEvents = new List<GameEvent>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 특정 등급의 이벤트 중 하나를 랜덤으로 뽑기
    public GameEvent GetRandomEventByRarity(CardRarity targetRarity)
    {
        List<GameEvent> filteredList = allEvents.FindAll(e => e.rarity == targetRarity);

        // 해당 등급 없으면 브론즈로 대체
        if (filteredList.Count == 0)
        {
            Debug.LogWarning($"[EventManager] {targetRarity} 등급 이벤트가 없습니다. Bronze로 대체합니다.");
            filteredList = allEvents.FindAll(e => e.rarity == CardRarity.Bronze);
        }

        if (filteredList.Count == 0) return null;

        int randomIndex = Random.Range(0, filteredList.Count);
        return filteredList[randomIndex];
    }

    // 2. [신규] 특정 등급의 '모든' 이벤트 리스트 반환 (중복 방지 로직용)
    public List<GameEvent> GetAllEventsByRarity(CardRarity targetRarity)
    {
        // 해당 등급만 싹 긁어모으기
        List<GameEvent> list = allEvents.FindAll(e => e.rarity == targetRarity);

        // 만약 해당 등급이 하나도 없으면? -> 브론즈라도 줘라 (안전장치)
        if (list.Count == 0)
        {
            list = allEvents.FindAll(e => e.rarity == CardRarity.Bronze);
        }

        // 리스트(목록) 자체를 반환합니다.
        return list;
    }

    // --- 일요일 일반/정예 전투 이벤트 가져오기 ---
    public GameEvent GetSundayMonsterEvent(int stage, List<string> excludeIDs)
    {
        // 해당 스테이지용 일요일 전투 이벤트를 모두 호출
        List<GameEvent> candidates = allEvents.FindAll(e =>
            e.targetStage == stage &&
            !e.isBoss &&
            e.eventType == EventType.Battle);

        // 이미 사용한 ID는 후보에서 제외합니다.
        candidates.RemoveAll(e => excludeIDs.Contains(e.eventID));

        // 모든 후보를 다 썼을 때의 안전장치
        if (candidates.Count == 0)
        {
            excludeIDs.Clear(); // 기록을 비우고 다시 검색
            candidates = allEvents.FindAll(e => e.targetStage == stage && !e.isBoss && e.eventType == EventType.Battle);
        }

        // 남은 후보 중 랜덤 선택
        if (candidates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);
            return candidates[randomIndex];
        }

        return null;
    }

    // --- 스테이지 보스 이벤트 가져오기 ---
    public GameEvent GetBossEvent(int stage)
    {
        foreach (var evt in allEvents)
        {
            // 조건: 해당 스테이지용이고, 보스인 것
            if (evt.targetStage == stage && evt.isBoss)
            {
                return evt;
            }
        }

        Debug.LogError($"{stage} 스테이지의 보스 이벤트를 찾지 못했습니다!");
        return null;
    }
}
