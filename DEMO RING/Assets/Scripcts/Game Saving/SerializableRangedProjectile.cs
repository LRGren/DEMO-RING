using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableRangedProjectile : ISerializationCallbackReceiver
{
    public int itemID;
    public int ammoAmount;

    public RangedProjectileItem GetProjectile()
    {
        return WorldItemDatabase.instance.GetProjectileFromSerializedData(this);
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {

    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }
}
