using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [Header("Current Deck IDs")]
    [SerializeField] private string[] m_EquippedIDs = new string[7];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDeck();
        }
        else { Destroy(gameObject); }
    }

    // 덱 정보 저장 (카드를 장착/해제할 때 호출)
    public void SaveDeck(Card[] currentCards)
    {
        for (int i = 0; i < 7; i++)
        {
            m_EquippedIDs[i] = (currentCards[i] != null) ? currentCards[i].CardID : "";
            PlayerPrefs.SetString($"Deck_Slot_{i}", m_EquippedIDs[i]);
        }
        PlayerPrefs.Save();
        Debug.Log("[DeckManager] 파티 편성 저장 완료.");
    }

    // 저장된 ID 배열 반환
    public string[] GetEquippedIDs()
    {
        for (int i = 0; i < m_EquippedIDs.Length; i++)
        {
            if (!string.IsNullOrEmpty(m_EquippedIDs[i]))
                Debug.Log($"[DeckManager] {i}번 슬롯에 저장된 ID: {m_EquippedIDs[i]}");
        }
        return m_EquippedIDs;
    }

    private void LoadDeck()
    {
        for (int i = 0; i < 7; i++)
        {
            m_EquippedIDs[i] = PlayerPrefs.GetString($"Deck_Slot_{i}", "");
            if (!string.IsNullOrEmpty(m_EquippedIDs[i]))
                Debug.Log($"[DeckManager] 슬롯 {i} 로드됨: {m_EquippedIDs[i]}");
        }
    }
}
