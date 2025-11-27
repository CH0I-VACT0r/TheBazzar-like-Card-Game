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
}
