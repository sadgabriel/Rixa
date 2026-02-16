using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameContextInputPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI mainText;
    private void OnEnable()
    {
        GameState gameState = stateManager.CurrentGameState;
        if (gameState == null || gameState.LeadPlayerId == null)
        {
            Debug.LogWarning("GameState or LeadPlayerId is null");
            return;
        }

        ClientState clientState = stateManager.CurrentClientState;
        if (clientState == null || clientState.PlayerId == null)
        {
            Debug.LogWarning("ClientState or PlayerId is null");
            return;
        }

        if (gameState.LeadPlayerId == clientState.PlayerId)
        {
            mainText.text = "게임의 배경 설정을 입력해 주세요";
        }
        else
        {
            mainText.text = "다른 플레이어가 배경 설정을 입력하는 중입니다";
        }
    }
}