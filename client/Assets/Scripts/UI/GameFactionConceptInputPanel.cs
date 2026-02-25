using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameFactionConceptInputPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI contextText;

    private void OnEnable()
    {
        string context = stateManager?.CurrentGameState?.Context?.RawContextDescription;
        if (string.IsNullOrEmpty(context))
        {
            contextText.text = "게임의 배경 설정을 불러오는 데 실패했습니다.";
        }
        else
        {
            contextText.text = context;
        }
    }
}