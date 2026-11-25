using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WorldItemDatabase : MonoBehaviour
{
    public static WorldItemDatabase instance;

    public WeaponItem unarmedWeapon;
    public GameObject characterDropPickUpItemPrefab;

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

    [Header("Ashes Of War")]
    [SerializeField] List<AshOfWar> ashesOfWar = new List<AshOfWar>();

    [Header("Spells")]
    [SerializeField] List<SpellItem> spells = new List<SpellItem>();

    [Header("Projectiles")]
    [SerializeField] List<RangedProjectileItem> projectiles = new List<RangedProjectileItem>();

    [Header("QuickSlotItems")]
    [SerializeField] List<QuickSlotItem> quickSlotItems = new List<QuickSlotItem>();

    [Header("Items")]
    private List<Item> items = new List<Item>();

    [Header("Item Keys")]
    public int weaponKey = 10000;
    public int headKey = 20000;
    public int bodyKey = 30000;
    public int handKey = 40000;
    public int legKey = 50000;
    public int ashOfWarKey = 60000;
    public int spellKey = 70000;
    public int projectileKey = 80000;
    public int quickSlotItemKey = 90000;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        items.Clear();

        RegisterItems(weapons, weaponKey);
        RegisterItems(headEquipment, headKey);
        RegisterItems(bodyEquipment, bodyKey);
        RegisterItems(handEquipment, handKey);
        RegisterItems(legEquipment, legKey);
        RegisterItems(ashesOfWar, ashOfWarKey);
        RegisterItems(spells, spellKey);
        RegisterItems(projectiles, projectileKey);
        RegisterItems(quickSlotItems, quickSlotItemKey);
    }

    private void RegisterItems<T>(List<T> category, int firstID) where T : Item
    {
        for (int i = 0; i < category.Count; i++)
        {
            T item = category[i];

            if (item == null)
                throw new System.InvalidOperationException($"物品列表存在空引用，下标：{i}");

            if (items.Contains(item))
                throw new System.InvalidOperationException($"物品重复注册：{item.name}");

            item.itemID = firstID + i;
            items.Add(item);
        }
    }

    public Item GetItemByID(int id)
    {
        return items.FirstOrDefault(i => i.itemID == id);
    }

    public WeaponItem GetWeaponFromSerializedData(SerializableWeapon serializableWeapon)
    {
        WeaponItem weapon = null;

        WeaponItem definition = serializableWeapon != null ? GetWeaponByID(serializableWeapon.itemID) : null;
        if (definition != null)
            weapon = Instantiate(definition);

        if (weapon == null)
            return Instantiate(unarmedWeapon);

        if (GetAshOfWarByID(serializableWeapon.ashOfWarID))
        {
            AshOfWar ashOfWar = Instantiate(GetAshOfWarByID(serializableWeapon.ashOfWarID));
            weapon.ashOfWar_Action = ashOfWar;
        }

        return weapon;
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

    public AshOfWar GetAshOfWarByID(int id)
    {
        return ashesOfWar.FirstOrDefault(a => a.itemID == id);
    }

    public SpellItem GetSpellByID(int id)
    {
        return spells.FirstOrDefault(s => s.itemID == id);
    }

    public RangedProjectileItem GetProjectileByID(int id)
    {
        return projectiles.FirstOrDefault(p => p.itemID == id);
    }

    public RangedProjectileItem GetProjectileFromSerializedData(SerializableRangedProjectile serializableRangedProjectile)
    {
        if (serializableRangedProjectile == null || serializableRangedProjectile.itemID == -1)
        {
            return null;
        }

        var tmp = Instantiate(GetProjectileByID(serializableRangedProjectile.itemID));
        if (tmp == null)
        {
            return null;
        }

        RangedProjectileItem projectile = Instantiate(tmp);
        if (projectile != null)
        {
            projectile.currentAmmoAmount = serializableRangedProjectile.ammoAmount;
        }

        return projectile;
    }

    public QuickSlotItem GetQuickSlotItemByID(int id)
    {
        return quickSlotItems.FirstOrDefault(q => q.itemID == id);
    }
    public QuickSlotItem GetQuickSlotItemFromSerializableDate(SerializableQuickSlotItem serializableQuickSlotItem)
    {
        if (serializableQuickSlotItem == null || serializableQuickSlotItem.itemID == -1)
        {
            return null;
        }

        var tmp = Instantiate(GetQuickSlotItemByID(serializableQuickSlotItem.itemID));
        if (tmp == null)
        {
            return null;
        }

        QuickSlotItem quickSlotItem = Instantiate(tmp);
        if (quickSlotItem != null)
        {
            quickSlotItem.itemCount = serializableQuickSlotItem.itemCount;
        }

        return quickSlotItem;
    }

}
