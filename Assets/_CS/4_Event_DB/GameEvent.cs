using UnityEngine;

// 이벤트 종류
public enum EventType
{
    Shop,            // 상점
    Blacksmith,      // 대장간 (수리/강화)
    Training,        // 훈련소 (스탯 상승)
    Battle,          // 전투
    RandomEncounter, // 랜덤 만남
    Crafting,        // 제작
    Quest            // 의뢰  
}

public abstract class GameEvent : ScriptableObject
{
    [Header("이벤트 기본 정보")]
    public string eventID;          // 구분을 위한 ID
    public string titleKey;         // 제목 (Localization Key)
    public string descKey;          // 설명 (Localization Key)
    public Sprite eventImage;       // 썸네일 이미지

    [Header("Progression Info")]
    public int targetStage = 1; // 어느 스테이지에서 등장하는가?
    public bool isBoss = false;  // 보스전인가?

    [Header("등급 및 타입")]
    public CardRarity rarity;       // 이벤트 등급 (브론즈, 실버, 골드, 다이아몬드)
    public EventType eventType;     // 이벤트 종류

    [Header("Lord Settings")]
    public LordType targetLord = LordType.Common;

    public abstract void Execute(PlayerController player);

    // 카드 조건 검사
    public virtual bool IsValidCard(Card card, out string failReason)
    {
        failReason = "";
        return true;
    }

    // 실제 효과 적용 (버튼 눌렀을 때)
    public virtual void ApplyEffect(Card card)
    {
        // 자식 클래스에서 구현
    }
}
