using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler
{
    public TextMeshProUGUI textComponent;
    public DynamicLyrics lyricsManager;
    public int wordNumber;
    private bool canHover = true;
    private bool alreadyHovered = false;

    void Update()
    {
        if (!alreadyHovered && wordNumber < lyricsManager.lastHoveredWordCount)
        {
            textComponent.color = Color.red;
            canHover = false;
            lyricsManager.handleStreakBreak();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (alreadyHovered || !canHover) return; 
        alreadyHovered = true;
        textComponent.color = Color.green;
        lyricsManager.AddScore();
        if(lyricsManager.lastHoveredWordCount+1==wordNumber) lyricsManager.StreakTextHandler();
        lyricsManager.lastHoveredWordCount = wordNumber;
    }
}
