using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FacialHairStyleCatalog", menuName = "Appearance/FacialHairStyleCatalog")]
public class FacialHairStyleCatalog : ScriptableObject
{
    public List<FacialHairStyleOption> facialHairStyleOptions = new List<FacialHairStyleOption>();

    public bool TryGetOption(string facialHairID, out FacialHairStyleOption option)
    {
        option = null;
        if (string.IsNullOrWhiteSpace(facialHairID) || facialHairStyleOptions == null)
            return false;

        foreach (var candidate in facialHairStyleOptions)
        {
            if (candidate == null || candidate.facialHairID != facialHairID)
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
