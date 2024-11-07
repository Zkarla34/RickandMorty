using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimationController : MonoBehaviour
{
    public float panelFadeDuration;
    public float characterFadeDuration;
    public float characterDelay;


    public IEnumerator ShowPanelCharacters(RectTransform panel)
    {
        panel.gameObject.SetActive(true);
        panel.localScale = Vector3.zero;
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup != null ) canvasGroup.alpha = 0f;

        LeanTween.scale(panel, Vector3.one, panelFadeDuration).setEase(LeanTweenType.easeOutBack);
        LeanTween.alphaCanvas(canvasGroup, 1f, panelFadeDuration).setFrom(0);

       yield return new WaitForSeconds(panelFadeDuration);
    }


    public IEnumerator ShowCharactersSequentially(List<RectTransform> charactersItems)
    {
        foreach(var character in charactersItems)
        {
            character.gameObject.SetActive(true);
            CanvasGroup canvasGroup = character.GetComponent<CanvasGroup>();
            if (canvasGroup != null) 
                canvasGroup.alpha = 0f;

            if(canvasGroup != null)
                LeanTween.alphaCanvas(canvasGroup, 1f, characterFadeDuration).setFrom(0);

            yield return new WaitForSeconds(characterDelay);
        }
    }

    public void SlideTransition(RectTransform panel, bool isNext)
    {
        float direction = isNext ? 1 : -1;
        Vector2 startPosition = panel.anchoredPosition;
        LeanTween.moveX(panel, startPosition.x + (Screen.width * direction), 0.5f).setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => panel.anchoredPosition = startPosition); // Reset position after transition
    }
}
