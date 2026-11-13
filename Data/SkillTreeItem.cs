namespace SkillManagementApp.Data;

public sealed record SkillTreeItem(int Id, string Name, int? ParentSkillId, bool ShowInInventory);
