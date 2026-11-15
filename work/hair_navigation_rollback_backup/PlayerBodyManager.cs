using UnityEngine;

public class PlayerBodyManager : MonoBehaviour
{
    public const string NoHairID = "none";

    [Header("Body Objects")]
    public GameObject maleObject;
    public GameObject femaleObject;

    [Header("Hair")]
    public GameObject hair;
    public GameObject maleFacialHair;

    [Header("Male")]
    public GameObject maleHead;
    public GameObject[] maleBody;
    public GameObject[] maleArms;
    public GameObject[] maleLegs;
    public GameObject maleEyebrows;

    [Header("Female")]
    public GameObject femaleHead;
    public GameObject[] femaleBody;
    public GameObject[] femaleArms;
    public GameObject[] femaleLegs;
    public GameObject femaleEyebrows;

    [Header("Appearance")]
    public HairStyleCatalog hairStyleCatalog;
    [SerializeField] private string currentHairID;
    public string CurrentHairID => currentHairID;

    [Header("Appearance Test")]
    public bool test;
    public string testHairID;

    #region Body Parts Enable/Disable
    public void EnableHead()
    {
        maleHead.SetActive(true);
        femaleHead.SetActive(true);

        maleEyebrows.SetActive(true);
        femaleEyebrows.SetActive(true);
    }

    public void DisableHead()
    {
        maleHead.SetActive(false);
        femaleHead.SetActive(false);

        maleEyebrows.SetActive(false);
        femaleEyebrows.SetActive(false);
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
    }

    #region Appearance

    // 新角色和缺少外观数据的旧存档使用同一个默认外观。
    public void ResetAppearance()
    {
        SetHairStyle(NoHairID);
    }

    public void LoadAppearance(AppearanceSaveData appearanceSaveData)
    {
        if (appearanceSaveData == null || string.IsNullOrWhiteSpace(appearanceSaveData.hairID))
        {
            ResetAppearance();
            return;
        }

        // 选项已删除或配置损坏时，也不沿用上一个角色的外观。
        if (!TrySetHairStyle(appearanceSaveData.hairID))
            ResetAppearance();
    }

    public AppearanceSaveData GetAppearanceData()
    {
        return new AppearanceSaveData { hairID = currentHairID };
    }

    public void SetHairStyle(string hairID)
    {
        TrySetHairStyle(hairID);
    }

    // 目录顺序就是切换顺序；到达两端后循环，并跳过无效选项。
    public bool TryCycleHairStyle(int direction)
    {
        var options = hairStyleCatalog != null ? hairStyleCatalog.hairStyleOptions : null;
        if (direction == 0 || options == null || options.Count == 0)
            return false;

        int step = direction > 0 ? 1 : -1;
        int index = options.FindIndex(option => option != null && option.hairID == currentHairID);
        if (index < 0)
            index = step > 0 ? -1 : 0;

        for (int attempt = 0; attempt < options.Count; attempt++)
        {
            index = (index + step + options.Count) % options.Count;
            HairStyleOption option = options[index];
            if (option == null || string.IsNullOrWhiteSpace(option.hairID) || option.hairID == currentHairID)
                continue;

            if (TrySetHairStyle(option.hairID))
                return true;
        }

        return false;
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
