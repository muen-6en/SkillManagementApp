namespace SkillManagementApp.Data;

public sealed class SkillTag
{
    public int Id { get; set; }

    public int SkillId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Skill Skill { get; set; } = null!;
}
