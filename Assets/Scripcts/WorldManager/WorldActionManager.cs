using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldActionManager : MonoBehaviour
{
    //用来存储所有的weapon actions，玩家在执行动作时会从这个manager中寻找对应的action来执行
    public static WorldActionManager Instance;

    [Header("Weapon Actions")]
    public WeaponItemAction[] weaponItemActions;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        for(int i = 0; i < weaponItemActions.Length; i++)
        {
            weaponItemActions[i].actionID = i;
        }
    }

    public WeaponItemAction GetWeaponActionByID(int actionID)
    {
        return weaponItemActions.FirstOrDefault(action => action.actionID == actionID);
    }
}
