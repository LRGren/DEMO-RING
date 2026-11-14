using System;
using System.Collections.Generic;
using System.Linq;

// Editor-only ledger. Deleted assets remain here so their save-game IDs are never reused.
[Serializable]
internal sealed class ItemIdLedger
{
    public int version = 1;
    public List<ItemIdEntry> entries = new List<ItemIdEntry>();
}

[Serializable]
internal sealed class ItemIdEntry
{
    public string guid;
    public int itemID;
}

internal sealed class ItemIdCandidate
{
    public string guid;
    public string path;
    public int category;
    public int previousID;
    public int priority = int.MaxValue;
    public int assignedID;
}

internal static class ItemIdAllocation
{
    internal static void Allocate(ItemIdLedger ledger, IList<ItemIdCandidate> candidates)
    {
        if (ledger == null || ledger.version != 1 || ledger.entries == null)
            throw new InvalidOperationException("Unsupported or damaged item ID ledger.");

        var byGuid = new Dictionary<string, ItemIdEntry>(StringComparer.Ordinal);
        var used = new HashSet<int>();
        foreach (var entry in ledger.entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.guid) || entry.itemID <= 0 ||
                byGuid.ContainsKey(entry.guid) || !used.Add(entry.itemID))
                throw new InvalidOperationException("Duplicate or invalid entry in the item ID ledger. Restore it from version control.");
            byGuid.Add(entry.guid, entry);
        }

        var pending = new List<ItemIdCandidate>();
        var seenGuids = new HashSet<string>();
        // During first migration, the database's existing items take precedence over copies.
        foreach (var candidate in candidates.OrderBy(c => c.priority).ThenBy(c => c.path, StringComparer.Ordinal))
        {
            if (string.IsNullOrEmpty(candidate.guid) || !seenGuids.Add(candidate.guid) ||
                candidate.category < 10000 || candidate.category > 90000 || candidate.category % 10000 != 0)
                throw new InvalidOperationException("Invalid item candidate: " + candidate.path);

            if (byGuid.TryGetValue(candidate.guid, out var entry))
            {
                if (entry.itemID < candidate.category || entry.itemID >= candidate.category + 10000)
                    throw new InvalidOperationException("Item changed category: " + candidate.path + ". Create a new asset to preserve saved IDs.");
                candidate.assignedID = entry.itemID;
            }
            else if (candidate.previousID >= candidate.category && candidate.previousID < candidate.category + 10000 &&
                     used.Add(candidate.previousID))
            {
                candidate.assignedID = candidate.previousID;
                entry = new ItemIdEntry { guid = candidate.guid, itemID = candidate.assignedID };
                ledger.entries.Add(entry);
                byGuid.Add(entry.guid, entry);
            }
            else
            {
                pending.Add(candidate);
            }
        }

        // Preserve ALL valid existing IDs before allocating any new IDs.
        foreach (var candidate in pending)
        {
            int nextID = used.Where(id => id >= candidate.category && id < candidate.category + 10000)
                .DefaultIfEmpty(candidate.category - 1).Max() + 1;
            if (nextID >= candidate.category + 10000)
                throw new InvalidOperationException("Item ID category exhausted: " + candidate.category);
            used.Add(nextID);
            candidate.assignedID = nextID;
            ledger.entries.Add(new ItemIdEntry { guid = candidate.guid, itemID = nextID });
        }
    }
}
