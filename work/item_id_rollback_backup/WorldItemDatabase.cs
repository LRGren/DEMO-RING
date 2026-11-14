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

    private void RegisterItems<T>(
    List<T> categoryItems, int firstID) where T : Item
    {
        foreach (var item in categoryItems)
        {
            if (item == null)
            {
                throw new System.InvalidOperationException("物品数据库中存在空引用，请检查注册列表。");
            }

            if (item.itemID <= 0)
            {
                throw new System.InvalidOperationException($"物品 {item.name} 尚未分配 ID，" + "请在编辑器中完成 ID 分配并保存。");
            }

            Item existing = items.FirstOrDefault(
                registered => registered.itemID == item.itemID);

            if (existing != null)
            {
                throw new System.InvalidOperationException($"物品 ID 冲突：{item.itemID}，" + $"{existing.name} 与 {item.name} 重复。" + "请检查是否复制了物品，或重复添加了同一资产。");
            }

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

        if (Instantiate(GetWeaponByID(serializableWeapon.itemID)))
            weapon = Instantiate(GetWeaponByID(serializableWeapon.itemID));

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
