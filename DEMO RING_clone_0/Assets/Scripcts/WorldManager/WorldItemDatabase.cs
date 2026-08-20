using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldItemDatabase : MonoBehaviour
{
    public static WorldItemDatabase Instance;

    public WeaponItem unarmedWeapon;

    [Header("Weapons")]
    [SerializeField] List<WeaponItem> weapons = new List<WeaponItem>();

    [Header("Head Equipment")]
    [SerializeField] List<HeadEquipmentItem> headEquipment = new List<HeadEquipmentItem>();

    [Header("Body Equipment")]
    [SerializeField] List<BodyEquipmentItem> bodyEquipment = new List<BodyEquipmentItem>();

    [Header("Hand Equipment")]
    [SerializeField] List<HandEquipmentItem> handEquipment = new List<HandEquipmentItem>();

    [Header("Leg Equipment")]
    [SerializeField] List<LegEquipmentItem> legEquipment = new List<LegEquipmentItem>();

    [Header("Items")]
    private List<Item> items = new List<Item>();

    [Header("Item Keys")]
    public int weaponKey = 10000;
    public int headKey = 20000;
    public int bodyKey = 30000;
    public int handKey = 40000;
    public int legKey = 50000;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        foreach (var weapon in weapons)
        {
            items.Add(weapon);
        }
        foreach (var head in headEquipment)
        {
            items.Add(head);
        }
        foreach (var body in bodyEquipment)
        {
            items.Add(body);
        }
        foreach (var hand in handEquipment)
        {
            items.Add(hand);
        }
        foreach (var leg in legEquipment)
        {
            items.Add(leg);
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (i < weapons.Count)
            {
                items[i].itemID = i + weaponKey;
            }
            else if (i < weapons.Count + headEquipment.Count)
            {
                items[i].itemID = i + headKey;
            }
            else if (i < weapons.Count + headEquipment.Count + bodyEquipment.Count)
            {
                items[i].itemID = i + bodyKey;
            }
            else if (i < weapons.Count + headEquipment.Count + bodyEquipment.Count + handEquipment.Count)
            {
                items[i].itemID = i + handKey;
            }
            else
            {
                items[i].itemID = i + legKey;
            }
        }
    }

    public WeaponItem GetWeaponByID(int id)
    {
        return weapons.FirstOrDefault(w => w.itemID == id);
    }

    public HeadEquipmentItem GetHeadEquipmentByID(int id)
    {
        return headEquipment.FirstOrDefault(h => h.itemID == id);
    }

    public BodyEquipmentItem GetBodyEquipmentByID(int id)
    {
        return bodyEquipment.FirstOrDefault(b => b.itemID == id);
    }

    public HandEquipmentItem GetHandEquipmentByID(int id)
    {
        return handEquipment.FirstOrDefault(h => h.itemID == id);
    }

    public LegEquipmentItem GetLegEquipmentByID(int id)
    {
        return legEquipment.FirstOrDefault(l => l.itemID == id);
    }
}
