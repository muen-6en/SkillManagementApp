using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public static class InventoryOperations
{
    public static Task<bool> HasCompletedInventoryAsync(AppDbContext db, CancellationToken cancellationToken = default)
        => db.SkillInventories.AsNoTracking().AnyAsync(cancellationToken);

    public static async Task<int> CompleteInventoryAsync(
        AppDbContext db,
        IReadOnlyList<Skill> existingSkills,
        IReadOnlyList<string> newSkillNames,
        CancellationToken cancellationToken = default)
    {
        if (existingSkills.Count == 0 && newSkillNames.Count == 0)
        {
            throw new InvalidOperationException("棚卸に含めるスキルが1件もありません。");
        }

        var inventory = new SkillInventory
        {
            CompletedAt = DateTime.Now,
        };
        db.SkillInventories.Add(inventory);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var skill in existingSkills)
        {
            db.SkillInventoryEntries.Add(new SkillInventoryEntry
            {
                InventoryId = inventory.Id,
                SkillId = skill.Id,
                Name = skill.Name,
            });
        }

        foreach (var rawName in newSkillNames)
        {
            var name = rawName.Trim();
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            var skill = new Skill
            {
                Name = name,
                CreatedAt = DateTime.Now,
            };
            db.Skills.Add(skill);
            await db.SaveChangesAsync(cancellationToken);

            var cycle = new Cycle
            {
                SkillId = skill.Id,
                Sequence = 1,
                CreatedAt = DateTime.Now,
            };
            db.Cycles.Add(cycle);
            await db.SaveChangesAsync(cancellationToken);

            db.SkillInventoryEntries.Add(new SkillInventoryEntry
            {
                InventoryId = inventory.Id,
                SkillId = skill.Id,
                Name = name,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return inventory.Id;
    }
}
