using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public static class CycleOperations
{
    public static async Task<Cycle?> GetLatestCycleAsync(AppDbContext db, int skillId, CancellationToken cancellationToken = default) =>
        await db.Cycles
            .AsNoTracking()
            .Where(cycle => cycle.SkillId == skillId)
            .OrderByDescending(cycle => cycle.Sequence)
            .ThenByDescending(cycle => cycle.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public static async Task<Cycle> EnsureFirstCycleAsync(AppDbContext db, int skillId, CancellationToken cancellationToken = default)
    {
        var latest = await db.Cycles
            .Where(cycle => cycle.SkillId == skillId)
            .OrderByDescending(cycle => cycle.Sequence)
            .ThenByDescending(cycle => cycle.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latest is not null)
        {
            return latest;
        }

        var cycle = new Cycle
        {
            SkillId = skillId,
            Sequence = 1,
            CreatedAt = DateTime.Now,
        };
        db.Cycles.Add(cycle);
        await db.SaveChangesAsync(cancellationToken);
        return cycle;
    }

    public static async Task<(Skill Skill, Cycle Cycle)?> TryGetCycleContextAsync(
        AppDbContext db,
        int skillId,
        int cycleId,
        CancellationToken cancellationToken = default)
    {
        var skill = await db.Skills.AsNoTracking().FirstOrDefaultAsync(item => item.Id == skillId, cancellationToken);
        if (skill is null)
        {
            return null;
        }

        var cycle = await db.Cycles.AsNoTracking().FirstOrDefaultAsync(item => item.Id == cycleId && item.SkillId == skillId, cancellationToken);
        if (cycle is null)
        {
            return null;
        }

        return (skill, cycle);
    }

    public static async Task<Cycle?> CreateNextCycleAsync(
        AppDbContext db,
        int skillId,
        int cycleId,
        string reflectionBody,
        CancellationToken cancellationToken = default)
    {
        var body = reflectionBody.Trim();
        if (string.IsNullOrEmpty(body))
        {
            return null;
        }

        var cycle = await db.Cycles.FirstOrDefaultAsync(item => item.Id == cycleId && item.SkillId == skillId, cancellationToken);
        if (cycle is null)
        {
            return null;
        }

        var reflection = await db.ReflectionEntries.FirstOrDefaultAsync(item => item.CycleId == cycleId, cancellationToken);
        if (reflection?.NextCycleId is not null)
        {
            return null;
        }

        var nextSequence = await db.Cycles
            .Where(item => item.SkillId == skillId)
            .MaxAsync(item => (int?)item.Sequence, cancellationToken) ?? 0;
        nextSequence += 1;

        var nextCycle = new Cycle
        {
            SkillId = skillId,
            Sequence = nextSequence,
            CreatedAt = DateTime.Now,
        };
        db.Cycles.Add(nextCycle);
        await db.SaveChangesAsync(cancellationToken);

        if (reflection is null)
        {
            db.ReflectionEntries.Add(new ReflectionEntry
            {
                CycleId = cycleId,
                Body = body,
                NextCycleId = nextCycle.Id,
            });
        }
        else
        {
            reflection.Body = body;
            reflection.NextCycleId = nextCycle.Id;
        }

        await db.SaveChangesAsync(cancellationToken);
        return nextCycle;
    }
}
