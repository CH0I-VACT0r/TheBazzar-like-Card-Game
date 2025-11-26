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
        // 해당 등급에 맞는 이벤트 추출
        List<GameEvent> filteredList = allEvents.FindAll(e => e.rarity == targetRarity);

        // (만약 해당 등급 이벤트가 하나도 없으면? -> 브론즈에서 찾기)
        if (filteredList.Count == 0)
        {
            Debug.LogWarning($"[EventManager] {targetRarity} 등급 이벤트가 없습니다. Bronze로 대체합니다.");
            filteredList = allEvents.FindAll(e => e.rarity == CardRarity.Bronze);
        }

        if (filteredList.Count == 0) return null; // 진짜 아무것도 없으면 null

        // 랜덤 뽑기
        int randomIndex = Random.Range(0, filteredList.Count);
        return filteredList[randomIndex];
    }
}
