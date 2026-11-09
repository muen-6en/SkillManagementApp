using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public sealed record CycleSettingView(
    string CurrentLevelText,
    string TargetLevelText,
    string ActionText,
    string ReflectionText);

public static class CycleSettingLoader
{
    public static async Task<Cycle?> GetPreviousCycleAsync(
        AppDbContext db,
        int skillId,
        int cycleId,
        CancellationToken cancellationToken = default)
    {
        var current = await db.Cycles
            .AsNoTracking()
            .FirstOrDefaultAsync(cycle => cycle.Id == cycleId && cycle.SkillId == skillId, cancellationToken);

        if (current is null)
        {
            return null;
        }

        return await db.Cycles
            .AsNoTracking()
            .Where(cycle => cycle.SkillId == skillId && cycle.Sequence < current.Sequence)
            .OrderByDescending(cycle => cycle.Sequence)
            .ThenByDescending(cycle => cycle.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<CycleSettingView> LoadAsync(
        AppDbContext db,
        int cycleId,
        CancellationToken cancellationToken = default)
    {
        var cycle = await db.Cycles
            .AsNoTracking()
            .FirstAsync(cycle => cycle.Id == cycleId, cancellationToken);

        var action = await db.CycleActions
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.CycleId == cycleId, cancellationToken);

        var reflection = await db.ReflectionEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.CycleId == cycleId, cancellationToken);

        return new CycleSettingView(
            FormatLevel(cycle.CurrentLevel),
            FormatLevel(cycle.TargetLevel),
            FormatOptionalText(action?.Body),
            FormatOptionalText(reflection?.Body));
    }

    private static string FormatLevel(string? code)
    {
        if (!LearningLevels.IsValid(code))
        {
            return "未設定";
        }

        var level = LearningLevels.Find(code);
        return level is null ? "未設定" : $"{level.Name}（{level.Meaning}）";
    }

    private static string FormatOptionalText(string? text) =>
        string.IsNullOrWhiteSpace(text) ? "未設定" : text.Trim();
}
