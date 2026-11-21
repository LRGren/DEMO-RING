using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUISpellManagerInputManager : MonoBehaviour
{
    PlayerControls playerControls;

    PlayerUISpellManager playerUISpellManager;

    [SerializeField] bool unequipedWeapon = false;
    [SerializeField] bool back = false;

    void Awake()
    {
        if (playerUISpellManager == null)
            playerUISpellManager = PlayerUIManager.instance.playerUISpellManager;
    }

    void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();

            playerControls.UI.B.performed += i => back = true;
            playerControls.UI.X.performed += i => unequipedWeapon = true;
        }

        playerControls.Enable();
    }

    void OnDisable()
    {
        playerControls.Disable();
    }

    void Update()
    {
        HandleBackInput();
        HandleUnequipInput();
    }

    private void HandleBackInput()
    {
        if (back)
        {
            back = false;

            playerUISpellManager.BackInputBranch();
        }
    }

    private void HandleUnequipInput()
    {
        if (unequipedWeapon)
        {
            unequipedWeapon = false;

            playerUISpellManager.UnequipSpell();
        }
    }
}
