using UnityEngine;

public class PlayerBodyManager : MonoBehaviour
{
    public PlayerManager player;

    public const string NoHairID = "none";
    public const string DefaultHeadID = "head00";
    public const string DefaultEyebrowID = "eyebrow01";
    public const string NoFacialHairID = "none";

    [Header("Body Objects")]
    public GameObject maleObject;
    public GameObject femaleObject;

    [Header("Hair")]
    public GameObject hair;
    public GameObject maleFacialHair;

    [Header("Male")]
    [Tooltip("指向 Male_Head_All_Elements 父物体，切换脸型只操作它的子物体。")]
    public GameObject maleHead;
    public GameObject[] maleBody;
    public GameObject[] maleArms;
    public GameObject[] maleLegs;
    public GameObject maleEyebrows;

    [Header("Female")]
    [Tooltip("指向 Female_Head_All_Elements 父物体。")]
    public GameObject femaleHead;
    public GameObject[] femaleBody;
    public GameObject[] femaleArms;
    public GameObject[] femaleLegs;
    public GameObject femaleEyebrows;

    [Header("Appearance")]
    public HairStyleCatalog hairStyleCatalog;
    public string currentHairID;
    public string CurrentHairID => currentHairID;

    [Header("Head Style")]
    public HeadStyleCatalog headStyleCatalog;
    public string currentHeadID = DefaultHeadID;
    public string CurrentHeadID => currentHeadID;

    [Header("Eyebrow Style")]
    public EyebrowStyleCatalog maleEyebrowStyleCatalog;
    public EyebrowStyleCatalog femaleEyebrowStyleCatalog;
    public string currentMaleEyebrowID = DefaultEyebrowID;
    public string currentFemaleEyebrowID = DefaultEyebrowID;
    public bool IsMaleBody => maleObject != null && maleObject.activeSelf;
    public string CurrentEyebrowID => IsMaleBody ? currentMaleEyebrowID : currentFemaleEyebrowID;
    public EyebrowStyleCatalog CurrentEyebrowStyleCatalog => IsMaleBody ? maleEyebrowStyleCatalog : femaleEyebrowStyleCatalog;

    [Header("Facial Hair Style")]
    public FacialHairStyleCatalog facialHairStyleCatalog;
    public string currentFacialHairID = NoFacialHairID;
    public string CurrentFacialHairID => currentFacialHairID;

    [Header("Appearance Test")]
    public bool test;
    public string testHairID;

    void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    #region Body Parts Enable/Disable
    public void EnableHead()
    {
        maleHead.SetActive(true);
        femaleHead.SetActive(true);

        maleEyebrows.SetActive(true);
        femaleEyebrows.SetActive(true);
        EnableFacialHair();
    }

    public void DisableHead()
    {
        maleHead.SetActive(false);
        femaleHead.SetActive(false);

        maleEyebrows.SetActive(false);
        femaleEyebrows.SetActive(false);
        DisableFacialHair();
    }

    public void EnableHair()
    {
        if (hair != null)
            hair.SetActive(true);
    }

    public void DisableHair()
    {
        if (hair != null)
            hair.SetActive(false);
    }

    public void EnableFacialHair()
    {
        maleFacialHair.SetActive(true);
    }

    public void DisableFacialHair()
    {
        maleFacialHair.SetActive(false);
    }

    public void EnableBody()
    {
        for (int i = 0; i < maleBody.Length; i++)
        {
            maleBody[i].SetActive(true);
        }

        for (int i = 0; i < femaleBody.Length; i++)
        {
            femaleBody[i].SetActive(true);
        }
    }

    public void DisableBody()
    {
        for (int i = 0; i < maleBody.Length; i++)
        {
            maleBody[i].SetActive(false);
        }

        for (int i = 0; i < femaleBody.Length; i++)
        {
            femaleBody[i].SetActive(false);
        }
    }

    public void EnableArms()
    {
        for (int i = 0; i < maleArms.Length; i++)
        {
            maleArms[i].SetActive(true);
        }

        for (int i = 0; i < femaleArms.Length; i++)
        {
            femaleArms[i].SetActive(true);
        }
    }

    public void DisableArms()
    {
        for (int i = 0; i < maleArms.Length; i++)
        {
            maleArms[i].SetActive(false);
        }

        for (int i = 0; i < femaleArms.Length; i++)
        {
            femaleArms[i].SetActive(false);
        }
    }

    public void EnableLegs()
    {
        for (int i = 0; i < maleLegs.Length; i++)
        {
            maleLegs[i].SetActive(true);
        }

        for (int i = 0; i < femaleLegs.Length; i++)
        {
            femaleLegs[i].SetActive(true);
        }
    }

    public void DisableLegs()
    {
        for (int i = 0; i < maleLegs.Length; i++)
        {
            maleLegs[i].SetActive(false);
        }

        for (int i = 0; i < femaleLegs.Length; i++)
        {
            femaleLegs[i].SetActive(false);
        }
    }

    #endregion
    public void ToggleBodyType(bool isMale)
    {
        if (isMale)
        {
            maleObject.SetActive(true);
            femaleObject.SetActive(false);
        }
        else
        {
            maleObject.SetActive(false);
            femaleObject.SetActive(true);
        }

        if (player.IsSpawned && player.IsOwner)
            player.playerNetworkManager.isMale.Value = isMale;
        ToggleEquipmentPreview(isShowingEquipmentPreview);
    }

    #region Appearance
    private bool isShowingEquipmentPreview = false;
    public void ToggleEquipmentPreview(bool showEquipment)
    {
        isShowingEquipmentPreview = showEquipment;
        if (showEquipment)
        {
            DisableHead();
            DisableHair();
            DisableBody();
            DisableArms();
            DisableLegs();
            player.playerEquipmentManager.EquipArmor();
        }
        else
        {
            EnableHead();
            EnableHair();
            EnableBody();
            EnableArms();
            EnableLegs();
            player.playerEquipmentManager.UnloadHeadEquipment();
            player.playerEquipmentManager.UnloadBodyEquipment();
            player.playerEquipmentManager.UnloadHandEquipment();
            player.playerEquipmentManager.UnloadLegEquipment();
        }
    }
    // 新角色和缺少外观数据的旧存档使用同一个默认外观。
    public void ResetAppearance()
    {
        SetHairStyle(NoHairID);
        SetHeadStyle(DefaultHeadID);
        TrySetEyebrowStyle(DefaultEyebrowID, true);
        TrySetEyebrowStyle(DefaultEyebrowID, false);
        SetFacialHairStyle(NoFacialHairID);
    }

    public void LoadAppearance(AppearanceSaveData appearanceSaveData)
    {
        if (appearanceSaveData == null)
        {
            ResetAppearance();
            return;
        }

        // 选项已删除或配置损坏时，也不沿用上一个角色的外观。
        if (string.IsNullOrWhiteSpace(appearanceSaveData.hairID) || !TrySetHairStyle(appearanceSaveData.hairID))
            SetHairStyle(NoHairID);
        if (string.IsNullOrWhiteSpace(appearanceSaveData.headID) || !TrySetHeadStyle(appearanceSaveData.headID))
            SetHeadStyle(DefaultHeadID);
        if (string.IsNullOrWhiteSpace(appearanceSaveData.maleEyebrowID) || !TrySetEyebrowStyle(appearanceSaveData.maleEyebrowID, true))
            TrySetEyebrowStyle(DefaultEyebrowID, true);
        if (string.IsNullOrWhiteSpace(appearanceSaveData.femaleEyebrowID) || !TrySetEyebrowStyle(appearanceSaveData.femaleEyebrowID, false))
            TrySetEyebrowStyle(DefaultEyebrowID, false);
        if (string.IsNullOrWhiteSpace(appearanceSaveData.facialHairID) || !TrySetFacialHairStyle(appearanceSaveData.facialHairID))
            SetFacialHairStyle(NoFacialHairID);
    }

    public AppearanceSaveData GetAppearanceData()
    {
        return new AppearanceSaveData
        {
            hairID = currentHairID,
            headID = currentHeadID,
            maleEyebrowID = currentMaleEyebrowID,
            femaleEyebrowID = currentFemaleEyebrowID,
            facialHairID = currentFacialHairID
        };
    }

    public void SetHairStyle(string hairID)
    {
        TrySetHairStyle(hairID);
    }

    // 普通选择失败时，保留当前模型和 ID；读档入口自行决定是否回退。
    public bool TrySetHairStyle(string hairID)
    {
        if (string.IsNullOrWhiteSpace(hairID))
        {
            Debug.LogWarning("Hair ID is empty. The current hairstyle was preserved.", this);
            return false;
        }

        if (hair == null)
        {
            Debug.LogWarning("Hair parent is not assigned.", this);
            return false;
        }

        // 光头不需要模型，默认恢复也不依赖目录是否已配置。
        if (hairID == NoHairID)
        {
            ApplyHairModel(null, NoHairID);
            return true;
        }

        if (hairStyleCatalog == null)
        {
            Debug.LogWarning("HairStyleCatalog is not assigned.", this);
            return false;
        }

        if (!hairStyleCatalog.TryGetOption(hairID, out HairStyleOption option))
        {
            Debug.LogWarning($"Hair option '{hairID}' is missing or duplicated in '{hairStyleCatalog.name}'.", this);
            return false;
        }

        if (string.IsNullOrWhiteSpace(option.hairModelName))
        {
            Debug.LogWarning($"Hair option '{hairID}' has no model name.", this);
            return false;
        }

        Transform selectedModel = hair.transform.Find(option.hairModelName);
        if (selectedModel == null || selectedModel.parent != hair.transform)
        {
            Debug.LogWarning($"Hair model '{option.hairModelName}' for '{hairID}' is not a direct child of '{hair.name}'.", this);
            return false;
        }

        ApplyHairModel(selectedModel, option.hairID);
        return true;
    }

    private void ApplyHairModel(Transform selectedModel, string hairID)
    {
        // 仅改变子物体；父物体是否显示仍由头盔等装备逻辑控制。
        foreach (Transform child in hair.transform)
            child.gameObject.SetActive(child == selectedModel);

        currentHairID = hairID;
    }

    public void SetHeadStyle(string headID)
    {
        TrySetHeadStyle(headID);
    }

    public bool TrySetHeadStyle(string headID)
    {
        if (headStyleCatalog == null || !headStyleCatalog.TryGetOption(headID, out HeadStyleOption option))
            return false;

        // 男女模型全部确认存在后再切换，避免只修改一半。
        if (!TryFindAppearanceModel(maleHead, option.maleHeadModelName, out Transform maleModel) ||
            !TryFindAppearanceModel(femaleHead, option.femaleHeadModelName, out Transform femaleModel))
            return false;

        ApplyAppearanceModel(maleHead, maleModel);
        ApplyAppearanceModel(femaleHead, femaleModel);
        currentHeadID = option.headID;
        return true;
    }

    public void SetEyebrowStyle(string eyebrowID)
    {
        TrySetEyebrowStyle(eyebrowID);
    }

    public bool TrySetEyebrowStyle(string eyebrowID)
    {
        return TrySetEyebrowStyle(eyebrowID, IsMaleBody);
    }

    private bool TrySetEyebrowStyle(string eyebrowID, bool isMale)
    {
        if (string.IsNullOrWhiteSpace(eyebrowID))
            return false;

        GameObject parent = isMale ? maleEyebrows : femaleEyebrows;
        if (parent == null)
            return false;

        Transform selectedModel = null;
        if (eyebrowID != "none")
        {
            var catalog = isMale ? maleEyebrowStyleCatalog : femaleEyebrowStyleCatalog;
            if (catalog == null || !catalog.TryGetOption(eyebrowID, out EyebrowStyleOption option) ||
                !TryFindAppearanceModel(parent, option.eyebrowModelName, out selectedModel))
                return false;
        }

        ApplyAppearanceModel(parent, selectedModel);
        if (isMale)
            currentMaleEyebrowID = eyebrowID;
        else
            currentFemaleEyebrowID = eyebrowID;
        return true;
    }

    public void SetFacialHairStyle(string facialHairID)
    {
        TrySetFacialHairStyle(facialHairID);
    }

    public bool TrySetFacialHairStyle(string facialHairID)
    {
        if (string.IsNullOrWhiteSpace(facialHairID) || maleFacialHair == null)
            return false;

        Transform selectedModel = null;
        if (facialHairID != NoFacialHairID)
        {
            if (facialHairStyleCatalog == null || !facialHairStyleCatalog.TryGetOption(facialHairID, out FacialHairStyleOption option) ||
                !TryFindAppearanceModel(maleFacialHair, option.facialHairModelName, out selectedModel))
                return false;
        }

        ApplyAppearanceModel(maleFacialHair, selectedModel);
        currentFacialHairID = facialHairID;
        return true;
    }

    private bool TryFindAppearanceModel(GameObject parent, string modelName, out Transform model)
    {
        model = null;
        if (parent == null || string.IsNullOrWhiteSpace(modelName))
            return false;

        model = parent.transform.Find(modelName);
        if (model != null && model.parent == parent.transform)
            return true;

        Debug.LogWarning($"Appearance model '{modelName}' is not a direct child of '{parent.name}'.", this);
        return false;
    }

    private void ApplyAppearanceModel(GameObject parent, Transform selectedModel)
    {
        // 不开启父物体，头盔、面罩和性别仍然决定部位是否可见。
        foreach (Transform child in parent.transform)
            child.gameObject.SetActive(child == selectedModel);
    }

    #endregion

    private void Update()
    {
        if (test)
        {
            test = false;
            SetHairStyle(testHairID);
        }
    }

}
