using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameContextInputPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI mainText;
    private void OnEnable()
    {
        if (stateManager.IsLeader())
        {
            mainText.text = "게임의 배경 설정을 입력해 주세요";
        }
        else
        {
            mainText.text = "다른 플레이어가 배경 설정을 입력하는 중입니다";
        }
    }
}