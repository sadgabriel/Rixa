using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameFactionFlawInputPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI factionConceptText;
    private void OnEnable()
    {
        GameState gameState = stateManager.CurrentGameState;
        int myIndex = 0;
        while (myIndex < gameState.Players.Count && gameState.Players[myIndex].Id != stateManager.MyPlayer.Id)
        {
            myIndex++;
        }

        if (myIndex < gameState.Players.Count)
        {
            int otherIndex = (myIndex + gameState.SetupOffset) % gameState.Players.Count;
            Faction otherFaction = stateManager.GetFactionById(gameState.Players[otherIndex].FactionId);
            if (otherFaction != null)
            {
                factionConceptText.text = otherFaction.RawConcept;
            }
            else
            {
                factionConceptText.text = "상대 진영의 컨셉을 불러오는 데 실패했습니다.";
            }
        }
    }
}