using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public static class InventoryHistoryLoader
{
    public static async Task<IReadOnlyList<InventoryHistorySummary>> LoadSummariesAsync(
        AppDbContext db,
        CancellationToken cancellationToken = default)
    {
        var inventories = await db.SkillInventories
            .AsNoTracking()
            .OrderByDescending(inventory => inventory.CompletedAt)
            .ThenByDescending(inventory => inventory.Id)
            .ToListAsync(cancellationToken);

        if (inventories.Count == 0)
        {
            return Array.Empty<InventoryHistorySummary>();
        }

        var inventoryIds = inventories.Select(inventory => inventory.Id).ToList();
        var entryCounts = await db.SkillInventoryEntries
            .AsNoTracking()
            .Where(entry => inventoryIds.Contains(entry.InventoryId))
            .GroupBy(entry => entry.InventoryId)
            .Select(group => new { InventoryId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.InventoryId, item => item.Count, cancellationToken);

        return inventories
            .Select(inventory => new InventoryHistorySummary(
                inventory.Id,
                inventory.CompletedAt,
                entryCounts.GetValueOrDefault(inventory.Id, 0)))
            .ToList();
    }

    public static async Task<InventoryHistoryDetail?> TryLoadDetailAsync(
        AppDbContext db,
        int inventoryId,
        CancellationToken cancellationToken = default)
    {
        var inventory = await db.SkillInventories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == inventoryId, cancellationToken);

        if (inventory is null)
        {
            return null;
        }

        var entries = await db.SkillInventoryEntries
            .AsNoTracking()
            .Where(entry => entry.InventoryId == inventoryId)
            .OrderBy(entry => entry.Name)
            .ThenBy(entry => entry.Id)
            .ToListAsync(cancellationToken);

        var skillIds = entries.Select(entry => entry.SkillId).Distinct().ToList();
        var cycles = await db.Cycles
            .AsNoTracking()
            .Where(cycle => skillIds.Contains(cycle.SkillId))
            .ToListAsync(cancellationToken);

        var latestCycleBySkill = cycles
            .GroupBy(cycle => cycle.SkillId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(cycle => cycle.Sequence)
                    .ThenByDescending(cycle => cycle.Id)
                    .First()
                    .Id);

        var entryRows = entries
            .Select(entry => new InventoryHistoryEntryRow(
                entry.Name,
                entry.SkillId,
                latestCycleBySkill.GetValueOrDefault(entry.SkillId)))
            .ToList();

        return new InventoryHistoryDetail(inventory.Id, inventory.CompletedAt, entryRows);
    }
}

public sealed record InventoryHistorySummary(int InventoryId, DateTime CompletedAt, int EntryCount);

public sealed record InventoryHistoryDetail(
    int InventoryId,
    DateTime CompletedAt,
    IReadOnlyList<InventoryHistoryEntryRow> Entries);

public sealed record InventoryHistoryEntryRow(string Name, int SkillId, int? LatestCycleId);
