using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSetupPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI contextText;
    [SerializeField] private TextMeshProUGUI factionConceptText;
    [SerializeField] private TextMeshProUGUI factionFlawText;

    private void OnEnable()
    {
        Context context = stateManager.CurrentGameState?.Context;
        if (context == null)
        {
            contextText.text = "게임 설정을 불러오는 데 실패했습니다.";
        }
        else
        {
            contextText.text = $"게임 설정: {context.RawContextDescription}";
        }

        Faction myFaction = stateManager.MyFaction;
        if (myFaction == null)
        {
            factionConceptText.text = "진영 설정을 불러오는 데 실패했습니다.";
            factionFlawText.text = "진영 설정을 불러오는 데 실패했습니다.";
        }
        else
        {
            factionConceptText.text = $"진영 컨셉: {myFaction.RawConcept}";
            factionFlawText.text = $"진영 결점: {myFaction.RawFlaw}";
        }
    }
}