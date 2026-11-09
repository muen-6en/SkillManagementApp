namespace SkillManagementApp.Data;

public sealed class SkillSuggestionDraft
{
    public int Id { get; set; }

    public string EntryPoint { get; set; } = SkillSuggestionCatalog.WorkProblems;

    public string StartDate { get; set; } = string.Empty;

    public string? EndDate { get; set; }

    public ICollection<SkillSuggestionTask> Tasks { get; set; } = [];
}
