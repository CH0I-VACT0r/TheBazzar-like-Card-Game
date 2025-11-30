using UnityEngine;
using System.Collections.Generic;

// 버프 종류 정의
public enum StatBuffType
{
    Damage, Shield, Heal, HealDot,
    Bleed, Poison, Burn,
    FreezeDuration,
    CooldownReductionPercent,
    PriceIncrease
}

[CreateAssetMenu(fileName = "New Buff Event", menuName = "Game/Events/Stat Buff")]
public class Event_Buff_Stat : GameEvent
{
    [Header("버프 설정")]
    public StatBuffType buffType;
    public float amount;

    [Header("필터")]
    // [수정] 타입을 검사할지 여부 (체크하면 모든 타입 허용, 해제하면 requiredType만 허용)
    public bool ignoreTypeCheck = false;
    public CardType requiredType; // ignoreTypeCheck가 false일 때만 검사

    [Header("태그 필터 (비워두면 무시)")]
    public List<string> requiredTags = new List<string>();

    // 1. UI 열기
    public override void Execute(PlayerController player)
    {
        if (EventInteractionManager.Instance != null)
            EventInteractionManager.Instance.StartInteraction(this);
    }

    // 2. 유효성 검사
    public override bool IsValidCard(Card card, out string failReason)
    {
        // A. 쿨타임 감소 예외 처리
        if (buffType == StatBuffType.CooldownReductionPercent && !card.ShowCooldownUI)
        {
            failReason = "This card has no cooldown.";
            return false;
        }

        // B. [수정] 타입 체크
        // ignoreTypeCheck가 켜져있으면 통과, 꺼져있으면 타입이 정확히 일치해야 함
        if (!ignoreTypeCheck && card.ItemType != requiredType)
        {
            failReason = $"Only {requiredType} type cards are allowed.";
            return false;
        }

        // C. 태그 체크
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

    // 3. 효과 적용
    public override void ApplyEffect(Card card)
    {
        if (card == null) return;

        switch (buffType)
        {
            case StatBuffType.Damage: card.IncreaseBaseDamage(amount); break;
            case StatBuffType.Shield: card.IncreaseBaseShield(amount); break;
            case StatBuffType.Heal: card.IncreaseBaseHeal(amount); break;
            case StatBuffType.HealDot: card.IncreaseHealStack((int)amount); break;

            case StatBuffType.Bleed: card.IncreaseBleedStack((int)amount); break;
            case StatBuffType.Poison: card.IncreasePoisonStack((int)amount); break;
            case StatBuffType.Burn: card.IncreaseBurnStack((int)amount); break;

            case StatBuffType.FreezeDuration: card.IncreaseFreezeDuration(amount); break;

            case StatBuffType.CooldownReductionPercent: card.ReduceBaseCooldownPercent(amount); break;

            case StatBuffType.PriceIncrease:
                card.ChangePrice((int)amount);
                break;
        }
        Debug.Log($"[Event] {card.CardNameKey} -> {buffType} 강화 완료! (+{amount})");
    }
}