using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableWeapon : ISerializationCallbackReceiver
{
    public int itemID;
    public int ashOfWarID;

    public WeaponItem GetWeapon()
    {
        return WorldItemDatabase.instance.GetWeaponFromSerializedData(this);
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {

    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }
}
