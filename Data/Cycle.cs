namespace SkillManagementApp.Data;

public sealed class Cycle
{
    public int Id { get; set; }

    public int SkillId { get; set; }

    public Skill Skill { get; set; } = null!;

    public int Sequence { get; set; }

    public string? CurrentLevel { get; set; }

    public string? TargetLevel { get; set; }

    public DateTime CreatedAt { get; set; }
}
