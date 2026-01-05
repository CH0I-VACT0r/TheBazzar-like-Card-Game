using UnityEngine;

[CreateAssetMenu(fileName = "New Battle Event", menuName = "Events/Battle Event")]
public class Event_Battle : GameEvent
{
    [Header("Battle Settings")]
    public string monsterID;          // 등장할 몬스터 ID (예: "elite_wolf")
    public float monsterMaxHP = 100f; // 몬스터 체력
    public int monsterLevel = 1;      // 몬스터 레벨
    public string[] monsterDeck;      // 몬스터가 사용할 카드들
     
    [Header("Reward Settings (추가)")]
    public int rewardGold;           // 승리 시 지급할 골드
    public int rewardExp;            // 승리 시 지급할 경험치
    public string rewardMaterialID;  // 승리 시 지급할 재료 카드
    

    public override void Execute(PlayerController player)
    {
        // 1. 먼저 전투 페이지로 전환 (UI가 생성되고 InitializeUI가 실행됨)
        UIManager.Instance.SwitchToBattlePage();
        BattleManager.Instance.SetCurrentEvent(this);
        // 2. 그 후에 몬스터 정보와 덱을 세팅 (이미 생성된 UI에 데이터를 덮어씌움)
        var monster = BattleManager.Instance.monsterController;
        monster.SetupMonster(monsterMaxHP, monsterLevel, monsterID, isBoss);
        monster.SetupDeck(monsterDeck);

        // 3. 마지막으로 페이즈 전환
        GameManager.Instance.SetPhase(GameManager.GamePhase.Battle);
    }
}
