using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SaveDataEditor
{
    // --- [1] 골드 지급 치트 (1000G 추가) ---
    [MenuItem("Tools/Cheats/Give 1000 Gold")]
    public static void GiveGold()
    {
        // 기존 골드를 가져와서 1000을 더함
        int currentGold = PlayerPrefs.GetInt("PlayerGold", 15);
        PlayerPrefs.SetInt("PlayerGold", currentGold + 1000);

        PlayerPrefs.Save();
        Debug.Log($"<color=yellow>[Cheat]</color> 1000골드가 지급되었습니다. (현재: {currentGold + 1000}G)");

        // 만약 게임이 실행 중이라면 매니저에도 즉시 반영
        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ModifyGold(0); // 내부적으로 UI 갱신 유도
        }
    }

    // --- [2] 경험치 레벨 업 치트 ---
    [MenuItem("Tools/Cheats/Level Up Player")]
    public static void LevelUpCheat()
    {
        int lv = PlayerPrefs.GetInt("PlayerLevel", 1);
        float hp = PlayerPrefs.GetFloat("PlayerMaxHP", 300f);

        PlayerPrefs.SetInt("PlayerLevel", lv + 1);
        PlayerPrefs.SetFloat("PlayerMaxHP", hp + 300f);
        PlayerPrefs.Save();

        Debug.Log($"[Cheat] 플레이어 레벨업: {lv + 1}, 최대 체력: {hp + 300f}");
    }

    // --- [3] 테스트 카드 세트 지급 ---
    [MenuItem("Tools/Cheats/Give Test Card Set")]
    public static void GiveTestCards()
    {
        // 테스트하고 싶은 카드 ID 추가
        string[] newTestCards = {
            
        };

        // 기존에 저장된 카드 리스트
        string existingCards = PlayerPrefs.GetString("OwnedCards", "");
        List<string> cardList = new List<string>();

        if (!string.IsNullOrEmpty(existingCards))
        {
            cardList.AddRange(existingCards.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries));
        }

        // 테스트 카드들 추가
        foreach (string id in newTestCards)
        {
            cardList.Add(id);
        }

        string updatedCards = string.Join(",", cardList);
        PlayerPrefs.SetString("OwnedCards", updatedCards);

        PlayerPrefs.Save();
        Debug.Log($"<color=cyan>[Cheat]</color> 테스트 카드 세트가 지급되었습니다. (총 {cardList.Count}장)");

        if (Application.isPlaying && InventoryManager.Instance != null)
        {
            Debug.Log("게임 중이므로 인벤토리를 새로 고칩니다. (씬 재시작 권장)");
        }
    }


    // --- [4] 세이브 데이터 전체 초기화 ---
    [MenuItem("Tools/Clear All Save Data")]
    public static void ClearSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=red>[System]</color> 모든 세이브 데이터가 초기화되었습니다.");
    }
}