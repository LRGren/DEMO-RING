using UnityEngine;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public sealed class ItemIconPreview : MonoBehaviour
{
    [FormerlySerializedAs("armor")] public Item item;

    [HideInInspector] public GameObject generatedPreview;
}
