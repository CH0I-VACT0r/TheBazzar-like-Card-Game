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
    }

    // 1. 보상 생성 (지급 X)
    public void PrepareReward(string monsterID, bool isBoss)
    {
        m_PendingGold = isBoss ? 8 : 4;
        m_PendingExp = isBoss ? 5 : 3;
        m_PendingCardID = GetMaterialByMonster(monsterID);

        // 통합 결과창 호출 (승리 상태로)
        UIManager.Instance.ShowBattleResult(true, m_PendingGold, m_PendingExp, m_PendingCardID);
    }

    // 2. 실제 보상 지급 (UI에서 버튼 누를 때 호출)
    public void ClaimRewards()
    {
        if (BattleManager.Instance == null || BattleManager.Instance.playerController == null) return;

        var player = BattleManager.Instance.playerController;

        // 골드 지급
        InventoryManager.Instance.ModifyGold(m_PendingGold);

        // 경험치 지급
        player.AddExperience(m_PendingExp);

        Card rewardCard = CardFactory.CreateCard(m_PendingCardID, null, -1);
        InventoryManager.Instance.AddCardObject(rewardCard);

        Debug.Log($"보상 수령 완료: {m_PendingGold}G, {m_PendingExp}XP, {m_PendingCardID}");

        // 보상 초기화
        m_PendingGold = 0;
        m_PendingExp = 0;
        m_PendingCardID = null;

        // 다음 날로 진행
        GameManager.Instance.StartNextDay();
    }

    private string GetMaterialByMonster(string monsterID)
    {
        if (monsterID.Contains("wolf")) return "card_wolffang";
        return null;
    }
}
