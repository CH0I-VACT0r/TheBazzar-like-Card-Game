using UnityEngine;
using System.Collections.Generic; // List 사용

[CreateAssetMenu(fileName = "New Shop Event", menuName = "Game/Events/Shop")]
public class Event_Shop : GameEvent
{
    [Header("상점 설정")]
    public float priceMultiplier = 1.0f; // 가격 배율 (0.8이면 20% 할인)
    public int rerollPrice = 3;         // 리롤 비용

    [Header("판매 물품 필터 (비워두면 랜덤)")]
    public CardRarity targetRarity;      // 특정 등급만 팔고 싶을 때
    public CardType targetType;          // 용병만? 소모품만? (None이면 다 섞어서)
    public string requiredTag;           // 예: "tag_barbarian" (비워두면 태그 무관)

    [Header("상점 주인")]
    public Sprite shopkeeperImage;       // 상점 주인 초상화
    public string greetingKey;           // 인사말 ("어서오게!")

    public override void Execute(PlayerController player)
    {
        Debug.Log($"[Event] 상점 '{eventID}' 오픈!");

        // ShopManager에게 "나(this)를 기반으로 상점을 열어줘"라고 요청
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OpenShop(this);
        }
        else
        {
            Debug.LogError("ShopManager가 씬에 없습니다!");
        }
    }
}
