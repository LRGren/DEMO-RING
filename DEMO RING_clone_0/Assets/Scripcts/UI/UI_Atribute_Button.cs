using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Atribute_Button : MonoBehaviour
{
    PlayerUISiteOfGraceManager playerUISiteOfGraceManager;
    Image image;

    public PlayerAtrribute thisAttribute;

    public void Select()
    {
        if (playerUISiteOfGraceManager == null)
            playerUISiteOfGraceManager = PlayerUIManager.instance.playerUISiteOfGraceManager;

        playerUISiteOfGraceManager.selectedPlayerAtrribute = thisAttribute;

        if (image == null)
            image = GetComponent<Image>();
        image.color = Color.white;
    }

    public void Deselect()
    {
        if (playerUISiteOfGraceManager == null)
            playerUISiteOfGraceManager = PlayerUIManager.instance.playerUISiteOfGraceManager;

        playerUISiteOfGraceManager.selectedPlayerAtrribute = PlayerAtrribute.None;

        if (image == null)
            image = GetComponent<Image>();
        image.color = Color.grey;
    }
}
