using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;
    
    private void Awake()
    {
        Hide();
    }
    
    public void ShowReady()
    {
        statusText.text = "준비 완료";
        gameObject.SetActive(true);
    }
    
    public void ShowCompleted()
    {
        statusText.text = "제출 완료";
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}