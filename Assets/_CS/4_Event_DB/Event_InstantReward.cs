using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Event_InstantReward", menuName = "Events/Instant/GeneralReward")]
public class Event_InstantReward : GameEvent
{
    [Header("골드 보상")]
    public int minGold = 0;
    public int maxGold = 0;

    [Header("카드 보상 (비어있으면 미지급)")]
    public List<string> rewardCardPool = new List<string>();
    [Tooltip("카드 풀에서 몇 장을 줄지 결정")]
    public int cardCount = 1;

    public override void Execute(PlayerController player)
    {
        // 1. 골드 보상 처리
        if (maxGold > 0)
        {
            int goldReward = Random.Range(minGold, maxGold + 1);
            InventoryManager.Instance.ModifyGold(goldReward);
            Debug.Log($"[Event] {goldReward} 골드 획득!");
        }

        // 2. 카드 보상 처리
        if (rewardCardPool != null && rewardCardPool.Count > 0)
        {
            for (int i = 0; i < cardCount; i++)
            {
                string randomID = rewardCardPool[Random.Range(0, rewardCardPool.Count)];
                InventoryManager.Instance.AddCard(randomID);
                Debug.Log($"[Event] 카드 획득: {randomID}");
            }
        }

        // 3. 다음 날로 진행
        GameManager.Instance.StartNextDay();
    }
}