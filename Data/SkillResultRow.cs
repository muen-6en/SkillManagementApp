using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public sealed record SkillResultRow(
    int CycleId,
    int Sequence,
    string? CurrentLevelName,
    string? CurrentLevelMeaning,
    string? TargetLevelName,
    string? TargetLevelMeaning,
    int RecordCount);

public static class SkillResultsLoader
{
    public static async Task<IReadOnlyList<SkillResultRow>> LoadAsync(
        AppDbContext db,
        int skillId,
        CancellationToken cancellationToken = default)
    {
        var cycles = await db.Cycles
            .AsNoTracking()
            .Where(cycle => cycle.SkillId == skillId)
            .OrderBy(cycle => cycle.Sequence)
            .ThenBy(cycle => cycle.Id)
            .ToListAsync(cancellationToken);

        if (cycles.Count == 0)
        {
            return Array.Empty<SkillResultRow>();
        }

        var cycleIds = cycles.Select(cycle => cycle.Id).ToList();
        var recordCounts = await db.PracticeRecords
            .AsNoTracking()
            .Where(record => cycleIds.Contains(record.CycleId))
            .GroupBy(record => record.CycleId)
            .Select(group => new { CycleId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.CycleId, item => item.Count, cancellationToken);

        return cycles
            .Select(cycle => new SkillResultRow(
                cycle.Id,
                cycle.Sequence,
                FormatLevelName(cycle.CurrentLevel),
                FormatLevelMeaning(cycle.CurrentLevel),
                FormatLevelName(cycle.TargetLevel),
                FormatLevelMeaning(cycle.TargetLevel),
                recordCounts.GetValueOrDefault(cycle.Id, 0)))
            .ToList();
    }

    private static string? FormatLevelName(string? code)
    {
        if (!LearningLevels.IsValid(code))
        {
            return null;
        }

        return LearningLevels.Find(code)?.Name;
    }

    private static string? FormatLevelMeaning(string? code)
    {
        if (!LearningLevels.IsValid(code))
        {
            return null;
        }

        return LearningLevels.Find(code)?.Meaning;
    }
}
