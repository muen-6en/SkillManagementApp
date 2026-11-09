namespace SkillManagementApp.Data;

public static class AppRoutes
{
    public const string AppName = "スキル管理";

    public static string PageTitle(string pageName) => $"{pageName} - {AppName}";

    /// <summary>使い方（トップ）</summary>
    public const string Home = "/";

    /// <summary>スキル棚卸</summary>
    public const string Inventory = "/skills";

    /// <summary>スキル選定アシスタント</summary>
    public const string SkillSuggestion = "/skills/suggest";

}
