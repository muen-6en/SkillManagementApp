namespace SkillManagementApp.Data;

public sealed class SkillInventoryEntry
{
    public int Id { get; set; }

    public int InventoryId { get; set; }

    public int SkillId { get; set; }

    public string Name { get; set; } = string.Empty;

    public SkillInventory Inventory { get; set; } = null!;

    public Skill Skill { get; set; } = null!;
}
