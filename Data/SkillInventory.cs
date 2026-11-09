namespace SkillManagementApp.Data;

/// <summary>1回分の棚卸結果。エントリ経由でスキルと紐づく。</summary>
public sealed class SkillInventory
{
    public int Id { get; set; }

    public DateTime CompletedAt { get; set; }
}
