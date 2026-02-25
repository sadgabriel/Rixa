using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameFactionFlawInputPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI factionConceptText;
    private void OnEnable()
    {
        Faction anotherFaction = stateManager.AnotherFaction;

        if (anotherFaction != null)
        {
            factionConceptText.text = anotherFaction.RawConcept;
        }
        else
        {
            factionConceptText.text = "상대 진영의 컨셉을 불러오는 데 실패했습니다.";
        }
    }
}