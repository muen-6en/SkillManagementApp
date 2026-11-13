using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public static class SkillInventoryExport
{
    public static async Task<string> BuildMarkdownAsync(AppDbContext db)
    {
        var skills = await LoadAsync(db);
        var builder = new StringBuilder();
        foreach (var skill in skills)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }

            builder.Append("# ").AppendLine(skill.Name);
            builder.Append("タグ: ").AppendLine(skill.Tags.Count == 0 ? "未設定" : string.Join("、", skill.Tags));
            builder.AppendLine("備考:");
            builder.AppendLine(string.IsNullOrWhiteSpace(skill.Note) ? "未設定" : skill.Note.Trim());

            foreach (var cycle in skill.Cycles)
            {
                builder.AppendLine();
                builder.Append("## サイクル ").Append(cycle.Sequence).Append("（").Append(cycle.Period).AppendLine("）");
                builder.Append("- 現在: ").AppendLine(cycle.CurrentLevel);
                builder.Append("- 目標: ").AppendLine(cycle.TargetLevel);
                builder.AppendLine("- やること:");
                builder.AppendLine(cycle.Action);
                builder.AppendLine("- 実践:");
                if (cycle.Practices.Count == 0)
                {
                    builder.AppendLine("  - 未設定");
                }
                else
                {
                    foreach (var practice in cycle.Practices)
                    {
                        builder.Append("  - ").Append(practice.RecordedAt.ToString("yyyy/MM/dd HH:mm")).Append(' ').AppendLine(FirstLine(practice.Body));
                        var rest = RestLines(practice.Body);
                        if (rest.Length > 0)
                        {
                            foreach (var line in rest.Split('\n'))
                            {
                                builder.Append("    ").AppendLine(line);
                            }
                        }
                    }
                }

                builder.Append("- 振り返り: ").AppendLine(cycle.Reflection);
            }
        }

        return builder.ToString();
    }

    public static async Task<string> BuildCsvAsync(AppDbContext db)
    {
        var skills = await LoadAsync(db);
        var builder = new StringBuilder();
        builder.Append('\uFEFF');
        builder.AppendLine("スキル名,タグ,備考,サイクル番号,期間開始,期間終了,現在のレベル,目標のレベル,やること記録,実践記録,振り返り");
        foreach (var skill in skills)
        {
            var tags = skill.Tags.Count == 0 ? "" : string.Join("、", skill.Tags);
            if (skill.Cycles.Count == 0)
            {
                AppendCsvRow(builder, skill.Name, tags, skill.Note, "", "", "", "", "", "", "", "");
                continue;
            }

            foreach (var cycle in skill.Cycles)
            {
                var practice = cycle.Practices.Count == 0
                    ? ""
                    : string.Join(
                        "\n\n",
                        cycle.Practices.Select(item => $"{item.RecordedAt:yyyy/MM/dd HH:mm}\n{item.Body.Trim()}"));
                AppendCsvRow(
                    builder,
                    skill.Name,
                    tags,
                    skill.Note,
                    cycle.Sequence.ToString(),
                    cycle.Start,
                    cycle.End,
                    CsvValue(cycle.CurrentLevel),
                    CsvValue(cycle.TargetLevel),
                    CsvValue(cycle.Action),
                    practice,
                    CsvValue(cycle.Reflection));
            }
        }

        return builder.ToString();
    }

    private static string CsvValue(string value) => value == "未設定" ? "" : value;

    private static async Task<IReadOnlyList<ExportSkill>> LoadAsync(AppDbContext db)
    {
        var skills = await db.Skills.AsNoTracking().Where(skill => skill.ShowInInventory).ToListAsync();
        var tags = await db.SkillTags.AsNoTracking().OrderBy(tag => tag.Name).ToListAsync();
        var cycles = await db.Cycles.AsNoTracking().OrderBy(cycle => cycle.Sequence).ToListAsync();
        var actions = await db.CycleActions.AsNoTracking().ToListAsync();
        var practices = await db.PracticeRecords.AsNoTracking()
            .OrderBy(record => record.RecordedAt)
            .ThenBy(record => record.Id)
            .ToListAsync();
        var reflections = await db.ReflectionEntries.AsNoTracking().ToListAsync();

        return skills
            .OrderBy(skill => skill.Name, StringComparer.Ordinal)
            .Select(skill => new ExportSkill(
                skill.Name,
                skill.Note,
                tags.Where(tag => tag.SkillId == skill.Id).Select(tag => tag.Name).ToList(),
                cycles.Where(cycle => cycle.SkillId == skill.Id).Select(cycle =>
                {
                    var action = actions.FirstOrDefault(item => item.CycleId == cycle.Id)?.Body;
                    var reflection = reflections.FirstOrDefault(item => item.CycleId == cycle.Id)?.Body;
                    return new ExportCycle(
                        cycle.Sequence,
                        FormatDate(cycle.StartDate),
                        FormatDate(cycle.EndDate),
                        FormatPeriod(cycle.StartDate, cycle.EndDate),
                        FormatLevel(cycle.CurrentLevel),
                        FormatLevel(cycle.TargetLevel),
                        string.IsNullOrWhiteSpace(action) ? "未設定" : action.Trim(),
                        practices.Where(item => item.CycleId == cycle.Id).ToList(),
                        string.IsNullOrWhiteSpace(reflection) ? "未実施" : reflection.Trim());
                }).ToList()))
            .ToList();
    }

    private static string FormatLevel(string? code)
    {
        var level = LearningLevels.Find(code);
        return level is null ? "未設定" : $"{level.Rank} {level.Name}";
    }

    private static string FormatDate(DateOnly? date) => date?.ToString("yyyy/MM/dd") ?? "";

    private static string FormatPeriod(DateOnly? start, DateOnly? end)
    {
        if (start is null || end is null)
        {
            return "未設定";
        }

        return $"{start:yyyy/MM/dd}〜{end:yyyy/MM/dd}";
    }

    private static string FirstLine(string body)
    {
        var trimmed = body.Replace("\r\n", "\n").Trim();
        var index = trimmed.IndexOf('\n');
        return index < 0 ? trimmed : trimmed[..index];
    }

    private static string RestLines(string body)
    {
        var trimmed = body.Replace("\r\n", "\n").Trim();
        var index = trimmed.IndexOf('\n');
        return index < 0 ? "" : trimmed[(index + 1)..];
    }

    private static void AppendCsvRow(StringBuilder builder, params string[] values)
    {
        builder.AppendLine(string.Join(",", values.Select(EscapeCsv)));
    }

    private static string EscapeCsv(string value)
    {
        var normalized = value.Replace("\r\n", "\n").Replace('\r', '\n');
        if (normalized.Contains('"') || normalized.Contains(',') || normalized.Contains('\n'))
        {
            return $"\"{normalized.Replace("\"", "\"\"")}\"";
        }

        return normalized;
    }

    private sealed record ExportSkill(string Name, string Note, IReadOnlyList<string> Tags, IReadOnlyList<ExportCycle> Cycles);

    private sealed record ExportCycle(
        int Sequence,
        string Start,
        string End,
        string Period,
        string CurrentLevel,
        string TargetLevel,
        string Action,
        IReadOnlyList<PracticeRecord> Practices,
        string Reflection);
}
