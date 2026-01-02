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
    public GameEvent GetSundayMonsterEvent(int stage)
    {
        // 모든 이벤트 중 'Sunday' 태그가 있거나 특정 네이밍 규칙을 가진 이벤트를 찾음.
        // 여기서는 예시로 'isSundayEvent'라는 필드가 GameEvent에 있다고 가정하거나,
        // 특정 rarity 중 하나를 반환하도록 구성할 수 있다.

        foreach (var evt in allEvents) // allEvents는 현재 등록된 모든 GameEvent 리스트
        {
            // 조건: 해당 스테이지용이고, 보스가 아니며, 일요일용 전투인 것
            if (evt.targetStage == stage && !evt.isBoss && evt.eventType == EventType.Battle)
            {
                return evt;
            }
        }

        // 못 찾았을 경우 대비용 (안전장치)
        Debug.LogWarning($"{stage} 스테이지의 일요일 이벤트를 찾지 못했습니다. 기본 이벤트를 반환합니다.");
        return allEvents.Count > 0 ? allEvents[0] : null;
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
