namespace SkillManagementApp.Data;

public sealed record LearningLevelDefinition(string Code, string Name, string Meaning);

public static class LearningLevels
{
    public const string UnconsciousIncompetence = "unconscious_incompetence";
    public const string ConsciousIncompetence = "conscious_incompetence";
    public const string ConsciousCompetence = "conscious_competence";
    public const string UnconsciousCompetence = "unconscious_competence";
    public const string UnconsciousCompetenceTeaching = "unconscious_competence_teaching";

    public static IReadOnlyList<LearningLevelDefinition> All { get; } =
    [
        new(UnconsciousIncompetence, "無意識的無能", "知らないし、できない"),
        new(ConsciousIncompetence, "意識的無能", "知っていても、できない"),
        new(ConsciousCompetence, "意識的有能", "考えると、できる"),
        new(UnconsciousCompetence, "無意識的有能", "考えなくても、できる"),
        new(UnconsciousCompetenceTeaching, "無意識的有能に意識的有能", "どこからでも、教えられる"),
    ];

    public static bool IsValid(string? code) =>
        !string.IsNullOrEmpty(code) && All.Any(level => level.Code == code);

    public static LearningLevelDefinition? Find(string? code) =>
        All.FirstOrDefault(level => level.Code == code);
}
