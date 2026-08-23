using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIToggleHUD : MonoBehaviour
{
    void OnEnable()
    {
        PlayerUIManager.instance.playerUIHudManager.ToggleHUD(false);
    }

    void OnDisable()
    {
        PlayerUIManager.instance.playerUIHudManager.ToggleHUD(true);
    }
}
