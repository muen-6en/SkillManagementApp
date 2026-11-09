namespace SkillManagementApp.Data;

public sealed class SkillSuggestionTask
{
    public int Id { get; set; }

    public int DraftId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Time { get; set; } = string.Empty;

    public string Difficulty { get; set; } = string.Empty;

    public string Dependency { get; set; } = string.Empty;

    public string Rework { get; set; } = string.Empty;

    public string Improvement { get; set; } = string.Empty;

    public SkillSuggestionDraft Draft { get; set; } = null!;
}
