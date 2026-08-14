using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public bool hasFloatingHPBarUI = true;
    [SerializeField] private UI_Character_HP_Bar characterHPBar;

    public void OnHPChanged(int oldValue, int newValue)
    {
        characterHPBar.oldHealthValue = oldValue;
        characterHPBar.SetStat(newValue);
    }
}
