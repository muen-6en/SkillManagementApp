using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public static class DeletionOperations
{
    public static async Task<bool> TryDeleteCycleAsync(
        AppDbContext db,
        int skillId,
        int cycleId,
        CancellationToken cancellationToken = default)
    {
        var cycle = await db.Cycles
            .FirstOrDefaultAsync(item => item.Id == cycleId && item.SkillId == skillId, cancellationToken);

        if (cycle is null)
        {
            return false;
        }

        db.Cycles.Remove(cycle);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static async Task<bool> TryDeleteSkillAsync(
        AppDbContext db,
        int skillId,
        CancellationToken cancellationToken = default)
    {
        var skill = await db.Skills.FirstOrDefaultAsync(item => item.Id == skillId, cancellationToken);
        if (skill is null)
        {
            return false;
        }

        var inventoryEntries = await db.SkillInventoryEntries
            .Where(entry => entry.SkillId == skillId)
            .ToListAsync(cancellationToken);
        db.SkillInventoryEntries.RemoveRange(inventoryEntries);

        db.Skills.Remove(skill);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
