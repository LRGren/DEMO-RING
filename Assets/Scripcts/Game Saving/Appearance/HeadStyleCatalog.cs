using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HeadStyleCatalog", menuName = "Appearance/HeadStyleCatalog")]
public class HeadStyleCatalog : ScriptableObject
{
    public List<HeadStyleOption> headStyleOptions = new List<HeadStyleOption>();

    public bool TryGetOption(string headID, out HeadStyleOption option)
    {
        option = null;
        if (string.IsNullOrWhiteSpace(headID) || headStyleOptions == null)
            return false;

        foreach (var candidate in headStyleOptions)
        {
            if (candidate == null || candidate.headID != headID)
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
