using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public static class SkillTagOperations
{
    public static async Task<bool> TryAddTagAsync(
        AppDbContext db,
        int skillId,
        string rawName,
        CancellationToken cancellationToken = default)
    {
        var name = rawName.Trim();
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        var skillExists = await db.Skills.AnyAsync(skill => skill.Id == skillId, cancellationToken);
        if (!skillExists)
        {
            return false;
        }

        var duplicate = await db.SkillTags.AnyAsync(
            tag => tag.SkillId == skillId && tag.Name == name,
            cancellationToken);
        if (duplicate)
        {
            return false;
        }

        db.SkillTags.Add(new SkillTag
        {
            SkillId = skillId,
            Name = name,
        });
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public static async Task AddTagsAsync(
        AppDbContext db,
        int skillId,
        IEnumerable<string> rawNames,
        CancellationToken cancellationToken = default)
    {
        foreach (var rawName in rawNames)
        {
            var name = rawName.Trim();
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            var duplicate = await db.SkillTags.AnyAsync(
                tag => tag.SkillId == skillId && tag.Name == name,
                cancellationToken);
            if (duplicate)
            {
                continue;
            }

            db.SkillTags.Add(new SkillTag
            {
                SkillId = skillId,
                Name = name,
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public static async Task<bool> TryRemoveTagAsync(
        AppDbContext db,
        int skillId,
        int tagId,
        CancellationToken cancellationToken = default)
    {
        var tag = await db.SkillTags
            .FirstOrDefaultAsync(item => item.Id == tagId && item.SkillId == skillId, cancellationToken);
        if (tag is null)
        {
            return false;
        }

        db.SkillTags.Remove(tag);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
