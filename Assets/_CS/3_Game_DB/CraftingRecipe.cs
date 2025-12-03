using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Game/Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [Header("제작 정보")]
    public string recipeNameKey; // UI 표시용 이름 (Localization Key)
    public int goldCost = 0;     // 제작 비용 (무료면 0)

    [Header("결과물")]
    public string resultCardID;  // 만들어질 아이템 ID (예: potion_hp_large)

    [Header("필요 재료 (최대 3개)")]
    // 필요한 재료 카드의 ID 목록 (순서는 상관없게 로직 짤 예정)
    public List<string> ingredientIDs = new List<string>();
}
