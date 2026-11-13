namespace SkillManagementApp.Data;

public sealed class Skill
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;

    public int? ParentSkillId { get; set; }

    public bool ShowInInventory { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}
