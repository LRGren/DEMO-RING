using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableQuickSlotItem : ISerializationCallbackReceiver
{
    public int itemID;
    public int itemCount;

    public QuickSlotItem GetQuickSlotItem()
    {
        return WorldItemDatabase.instance.GetQuickSlotItemFromSerializableDate(this);
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {

    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }
}
