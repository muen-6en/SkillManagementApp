namespace SkillManagementApp.Data;

public static class SqlitePaths
{
    public const string FileName = "skillmanagement.db";

    public static string DataDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SkillManagementApp");

    public static string DatabasePath { get; } = Path.Combine(DataDirectory, FileName);
}

public sealed class DatabaseLocation
{
    public DatabaseLocation(string path)
    {
        Path = path;
    }

    public string Path { get; }
}
