using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUIEquipmentManagerInputManager : MonoBehaviour
{
    PlayerControls playerControls;

    PlayerUIEquipmentManager playerUIEquipmentManager;

    [SerializeField] bool unequipedWeapon = false;
    [SerializeField] bool back = false;

    void Awake()
    {
        playerUIEquipmentManager = GetComponent<PlayerUIEquipmentManager>();
    }

    void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.UI.X.performed += i => unequipedWeapon = true;
            playerControls.UI.B.performed += i => back = true;
        }

        playerControls.Enable();
    }

    void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        HandlePlayerUIEquipmentInputs();
    }

    private void HandlePlayerUIEquipmentInputs()
    {
        if (unequipedWeapon)
        {
            unequipedWeapon = false;
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected == null || currentSelected.CompareTag("Equipment Slot") == false)
            {
                return;
            }

            playerUIEquipmentManager.UnequipItem();
        }

        if (back)
        {
            back = false;

            if (PlayerUIManager.instance.playerUIEquipmentManager.equipmentInventoryWindow.activeSelf)
            {
                playerUIEquipmentManager.FromEquipmentInventoryWindowBackToEquipmentMenu();
                playerUIEquipmentManager.RefreshMenu();
            }
            else if (PlayerUIManager.instance.playerUIEquipmentManager.menu.activeSelf)
            {
                PlayerUIManager.instance.playerUIEquipmentManager.CloseEquipmentMenu();
                PlayerUIManager.instance.playerUICharacterMenuManager.OpenCharacterMenu();
            }
        }
    }


}
