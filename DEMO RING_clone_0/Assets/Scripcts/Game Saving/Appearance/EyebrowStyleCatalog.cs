using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EyebrowStyleCatalog", menuName = "Appearance/EyebrowStyleCatalog")]
public class EyebrowStyleCatalog : ScriptableObject
{
    public List<EyebrowStyleOption> eyebrowStyleOptions = new List<EyebrowStyleOption>();

    public bool TryGetOption(string eyebrowID, out EyebrowStyleOption option)
    {
        option = null;
        if (string.IsNullOrWhiteSpace(eyebrowID) || eyebrowStyleOptions == null)
            return false;

        foreach (var candidate in eyebrowStyleOptions)
        {
            if (candidate == null || candidate.eyebrowID != eyebrowID)
                continue;

            // 重复 ID 不根据目录顺序选择，避免显示结果不确定。
            if (option != null)
            {
                option = null;
                return false;
            }

            option = candidate;
        }

        return option != null;
    }
}
