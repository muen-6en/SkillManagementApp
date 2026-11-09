namespace SkillManagementApp.Data;

public sealed class PracticeRecord
{
    public int Id { get; set; }

    public int CycleId { get; set; }

    public Cycle Cycle { get; set; } = null!;

    public string Body { get; set; } = string.Empty;

    public DateTime RecordedAt { get; set; }
}
