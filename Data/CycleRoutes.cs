namespace SkillManagementApp.Data;

public static class CycleRoutes
{
    public static string Current(int skillId, int cycleId) =>
        $"/skills/{skillId}/cycles/{cycleId}/current";

    public static string Target(int skillId, int cycleId) =>
        $"/skills/{skillId}/cycles/{cycleId}/target";

    public static string Practice(int skillId, int cycleId) =>
        $"/skills/{skillId}/cycles/{cycleId}/practice";

    public static string Reflection(int skillId, int cycleId) =>
        $"/skills/{skillId}/cycles/{cycleId}/reflection";

    public static string Diff(int skillId, int cycleId) =>
        $"/skills/{skillId}/cycles/{cycleId}/diff";

    public static string Results(int skillId) =>
        $"/skills/{skillId}/results";
}
