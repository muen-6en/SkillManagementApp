namespace SkillManagementApp.Data;

public static class AppRoutes
{
    public const string AppName = "スキル管理";

    public static string PageTitle(string pageName) => $"{pageName} - {AppName}";

    /// <summary>使い方（トップ）</summary>
    public const string Home = "/";

    /// <summary>スキル棚卸</summary>
    public const string Inventory = "/skills";

    /// <summary>スキル整理</summary>
    public const string Organize = "/skills/organize";

    /// <summary>スキル選定アシスタント</summary>
    public const string SkillSuggestion = "/skills/suggest";

    /// <summary>ヘルプ（OSS情報）</summary>
    public const string Help = "/help";

}
