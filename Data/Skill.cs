namespace SkillManagementApp.Data;

public sealed class Skill
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}
