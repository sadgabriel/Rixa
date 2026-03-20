using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class GameSetupFinishPanel : NonPersistentPanel
{
    [SerializeField] private TextMeshProUGUI contextText;
    [SerializeField] private TextMeshProUGUI factionDescriptionText;
    [SerializeField] private TextMeshProUGUI factionResourceText;

    private void OnEnable()
    {
        Context context = stateManager.CurrentGameState?.Context;
        if (context == null)
        {
            contextText.text = "게임 설정을 불러오는 데 실패했습니다.";
        }
        else
        {
            contextText.text = context.Description;
        }

        Faction myFaction = stateManager.MyFaction;
        if (myFaction == null)
        {
            factionDescriptionText.text = "진영 설정을 불러오는 데 실패했습니다.";
            factionResourceText.text = "진영 설정을 불러오는 데 실패했습니다.";
        }
        else
        {
            factionDescriptionText.text = myFaction.Description;
            
            List<Resource> resources = myFaction.Resources;
            StringBuilder resourceTextBuilder = new StringBuilder();

            foreach (Resource resource in resources)
            {
                resourceTextBuilder.AppendLine($"- {resource.Name}: {resource.Description}");
            }
            factionResourceText.text = resourceTextBuilder.ToString();
        }
    }
}