using UnityEngine;
using System.Collections.Generic;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance;

    // 현재 대기 중인 보상 데이터
    private int m_PendingGold;
    private int m_PendingExp;
    private string m_PendingCardID;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 보상 생성
    public void PrepareReward(string monsterID, bool isBoss, bool isQuest)
    {
        // 보상 수치 결정 (의뢰: 약함 / 일반: 중간 / 보스: 강력)
        if (isQuest)
        {
            m_PendingGold = 2;  // 기존(4)의 절반
            m_PendingExp = 1;   // 기존(3)의 절반 이하
        }
        else if (isBoss)
        {
            m_PendingGold = 8;
            m_PendingExp = 5;
        }
        else // 일반 일요일 정예 전투
        {
            m_PendingGold = 4;
            m_PendingExp = 3;
        }

        // 재료 카드 결정 (의뢰 포함 모든 전투에서 몬스터 ID 기반으로 획득)
        m_PendingCardID = GetMaterialByMonster(monsterID);

        // 통합 결과창 호출
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowBattleResult(true, m_PendingGold, m_PendingExp, m_PendingCardID);
        }
    }

    public void ClaimRewards()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.playerController == null) return;

        var player = BattleManager.Instance.playerController;

        // 골드 및 경험치 지급
        player.ModifyGold(m_PendingGold);
        player.AddExperience(m_PendingExp);

        // 카드 보상 지급 (재료 카드가 있을 때만)
        if (!string.IsNullOrEmpty(m_PendingCardID))
        {
            Card rewardCard = CardFactory.CreateCard(m_PendingCardID, null, -1);
            InventoryManager.Instance.AddCardObject(rewardCard);
        }

        Debug.Log($"보상 수령 완료: {m_PendingGold}G, {m_PendingExp}XP, {m_PendingCardID}");

        // 데이터 초기화 및 다음 날로 진행
        m_PendingGold = 0;
        m_PendingExp = 0;
        m_PendingCardID = null;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNextDay();
        }
    }
    public void SetPendingRewards(int gold, int exp, string cardID)
    {
        m_PendingGold = gold;
        m_PendingExp = exp;
        m_PendingCardID = cardID;

        Debug.Log($"[Reward] 보상 예약 완료: {gold}G, {exp}XP, 재료:{cardID}");
    }

    private string GetMaterialByMonster(string monsterID)
    {
        if (monsterID.Contains("wolf")) return "card_wolffang";
        return null;
    }
}
