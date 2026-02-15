using UnityEngine;
using UnityEngine.UI;

public class StatusIndicator : MonoBehaviour
{
    [SerializeField] private Image indicatorImage;
    [SerializeField] private Color readyColor = Color.green;
    [SerializeField] private Color completedColor = Color.blue;
    
    private void Awake()
    {
        Hide();
    }
    
    public void ShowReady()
    {
        indicatorImage.color = readyColor;
        gameObject.SetActive(true);
    }
    
    public void ShowCompleted()
    {
        indicatorImage.color = completedColor;
        gameObject.SetActive(true);
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}