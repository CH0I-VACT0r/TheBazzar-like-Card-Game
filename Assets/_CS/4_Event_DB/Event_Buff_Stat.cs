using UnityEngine;
using System.Collections.Generic;

// 버프 종류 정의
public enum StatBuffType
{
    Damage,
    Shield,
    Heal,
    HealDot,
    Bleed,
    Poison,
    Burn,
    FreezeDuration,
    CooldownReductionPercent, // 쿨타임 감소 (0.1 = 10%)
    PriceIncrease // 가치 증가 (상점에 비싸게 팔기 위함)
}

[CreateAssetMenu(fileName = "New Buff Event", menuName = "Game/Events/Stat Buff")]
public class Event_Buff_Stat : GameEvent
{
    [Header("버프 설정")]
    public StatBuffType buffType;
    public float amount; // 증가량 (정수형 스탯은 int로 캐스팅해서 사용)

    [Header("필터")]
    public CardType requiredType;

    [Header("필터 (비워두면 모든 카드 허용)")]
    public List<string> requiredTags = new List<string>();

    // 1. UI 열기
    public override void Execute(PlayerController player)
    {
        Debug.Log($"[Event] {eventID} 시작: 훈련소(버프) UI 오픈");

        if (EventInteractionManager.Instance != null)
            EventInteractionManager.Instance.StartInteraction(this);
    }

    // 2. 유효성 검사
    public override bool IsValidCard(Card card, out string failReason)
    {
        // A. 쿨타임 감소 버프인데 패시브 카드라면?
        if (buffType == StatBuffType.CooldownReductionPercent && !card.ShowCooldownUI)
        {
            failReason = "This card has no cooldown.";
            return false;
        }

        // B. [신규] 타입 체크 (설정된 경우에만)
        if ((int)requiredType != 0 && card.ItemType != requiredType)
        {
            failReason = $"Only {requiredType} type cards are allowed."; // 예: "Only Mercenary..."
            return false;
        }

        // C. 태그 체크 (설정된 경우에만)
        if (requiredTags.Count > 0)
        {
            bool hasTag = false;
            foreach (string tag in requiredTags)
            {
                if (card.HasTagKey(tag))
                {
                    hasTag = true;
                    break;
                }
            }

            if (!hasTag)
            {
                failReason = "This training is not suitable for this class.";
                return false;
            }
        }

        failReason = "";
        return true;
    }

    // 3. 효과 적용 (매니저가 버튼 누르면 호출)
    public override void ApplyEffect(Card card)
    {
        if (card == null) return;

        switch (buffType)
        {
            case StatBuffType.Damage:
                card.IncreaseBaseDamage(amount);
                break;
            case StatBuffType.Shield:
                card.IncreaseBaseShield(amount);
                break;
            case StatBuffType.Heal:
                card.IncreaseBaseHeal(amount);
                break;
            case StatBuffType.HealDot:
                card.IncreaseHealStack((int)amount);
                break;

            case StatBuffType.Bleed:
                card.IncreaseBleedStack((int)amount);
                break;
            case StatBuffType.Poison:
                card.IncreasePoisonStack((int)amount);
                break;
            case StatBuffType.Burn:
                card.IncreaseBurnStack((int)amount);
                break;

            case StatBuffType.FreezeDuration:
                card.IncreaseFreezeDuration(amount);
                break;

            case StatBuffType.CooldownReductionPercent:
                card.ReduceBaseCooldownPercent(amount);
                break;

            case StatBuffType.PriceIncrease:
                // 가격을 올려서 나중에 비싸게 팔 수 있게 함
                card.ChangePrice((int)amount);
                Debug.Log($"[{card.CardNameKey}] 가치 {amount}G 상승!");
                break;
        }

        Debug.Log($"[Event] {card.CardNameKey} -> {buffType} 강화 완료! (+{amount})");
    }
}