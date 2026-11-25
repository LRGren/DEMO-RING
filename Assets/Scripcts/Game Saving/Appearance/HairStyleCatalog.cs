using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HairStyleCatalog", menuName = "Appearance/HairStyleCatalog")]
public class HairStyleCatalog : ScriptableObject
{
    public List<HairStyleOption> hairStyleOptions = new List<HairStyleOption>();

    public bool TryGetOption(string hairID, out HairStyleOption option)
    {
        option = null;
        if (string.IsNullOrWhiteSpace(hairID) || hairStyleOptions == null)
            return false;

        foreach (var candidate in hairStyleOptions)
        {
            if (candidate == null || candidate.hairID != hairID)
                continue;

            // 同一 ID 出现两次时，拒绝选择，避免目录顺序决定角色外观。
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
