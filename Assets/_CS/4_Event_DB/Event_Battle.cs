using UnityEngine;

[CreateAssetMenu(fileName = "New Battle Event", menuName = "Events/Battle Event")]
public class Event_Battle : GameEvent
{
    [Header("Battle Settings")]
    public string monsterID;         // 등장할 몬스터 ID (예: "elite_wolf")
    public string[] monsterDeck;     // 몬스터가 사용할 카드들

    public override void Execute(PlayerController player)
    {
        // 1. 먼저 전투 페이지로 전환 (UI가 생성되고 InitializeUI가 실행됨)
        UIManager.Instance.SwitchToBattlePage();

        // 2. 그 후에 몬스터 정보와 덱을 세팅 (이미 생성된 UI에 데이터를 덮어씌움)
        var monster = BattleManager.Instance.monsterController;
        monster.MonsterID = monsterID;
        monster.IsBoss = isBoss;

        // 여기서 넘겨주는 monsterDeck(ID 배열)이 실제 UI 슬롯에 그려짐
        monster.SetupDeck(monsterDeck);

        // 3. 마지막으로 페이즈 전환
        GameManager.Instance.SetPhase(GameManager.GamePhase.Battle);
    }
}
