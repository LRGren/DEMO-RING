using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatBar : MonoBehaviour
{
    private Slider slider;
    
    private RectTransform rectTransform;

    [Header("Bar Options")]
    [SerializeField] private bool scaleBarLengthWithStats = true;
    [SerializeField] private float widthScaleMultiplier = 1f;
    
    
    protected virtual void Awake()
    {
        slider = GetComponent<Slider>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetStat(int newValue)
    {
        slider.value = newValue;
    }

    public void SetMaxStat(int maxValue)
    {
        slider.maxValue = maxValue;
        slider.value = maxValue;

        if (scaleBarLengthWithStats)
        {
            rectTransform.sizeDelta = new Vector2(maxValue * widthScaleMultiplier, rectTransform.sizeDelta.y);
            
            //矫正位置
            PlayerUIManager.instance.playerUIHudManager.RefreshHUD();
        }
    }
}
