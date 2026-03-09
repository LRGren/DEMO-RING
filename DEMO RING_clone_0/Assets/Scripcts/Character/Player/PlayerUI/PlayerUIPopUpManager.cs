using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUIPopUpManager : MonoBehaviour
{
    [Header("You Died Pop Up")] 
    [SerializeField] private GameObject youDiedPopUpGameObject;
    [SerializeField] private TextMeshProUGUI youDiedPopUpBackgroundText;
    [SerializeField] private TextMeshProUGUI youDiedPopUpText;
    [SerializeField] private CanvasGroup youDiedPopUpCanvasGroup;

    public void SendYouDiedPopUp()
    {
        //实现某些效果 如 咒死
        
        youDiedPopUpGameObject.SetActive(true);
        youDiedPopUpBackgroundText.characterSpacing = 0;
        
        //拉伸
        StartCoroutine(StretchPopUpTextOverTime(youDiedPopUpBackgroundText, 8, 8.32f));

        //渐入
        StartCoroutine(FadeInPopUpOverTime(youDiedPopUpCanvasGroup, 5));

        //等待 渐渐淡出
        StartCoroutine(WaitThenFadeOutPopUpOverTime(youDiedPopUpCanvasGroup, 2, 5));

    }

    private IEnumerator StretchPopUpTextOverTime(TextMeshProUGUI text,float duration,float stretchAmount)
    {
        if (duration > 0)
        {
            text.characterSpacing = 0;
            float timer = 0;

            yield return null;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                text.characterSpacing = Mathf.Lerp(text.characterSpacing, stretchAmount, duration * (Time.deltaTime / 20));
                yield return null;
            }
        }
    }

    private IEnumerator FadeInPopUpOverTime(CanvasGroup canvas, float duration)
    {
        if (duration > 0)
        {
            canvas.alpha = 0;
            float timer = 0;
            
            yield return null;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                canvas.alpha = Mathf.Lerp(canvas.alpha, 1, duration * Time.deltaTime);
                yield return null;
            }
        }
        
        canvas.alpha = 1;
        yield return null;
    }

    private IEnumerator WaitThenFadeOutPopUpOverTime(CanvasGroup canvas, float duration, float delay)
    {
        if (duration > 0)
        {
            while (delay > 0)
            {
                delay -= Time.deltaTime;
                yield return null;
            }
            
            canvas.alpha = 1;
            float timer = 0;

            yield return null;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                canvas.alpha = Mathf.Lerp(canvas.alpha, 0, duration * Time.deltaTime);
                yield return null;
            }
        }

        canvas.alpha = 0;
        yield return null;
    }

}
