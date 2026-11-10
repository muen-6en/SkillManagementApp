using Microsoft.EntityFrameworkCore;

namespace SkillManagementApp.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<Cycle> Cycles => Set<Cycle>();

    public DbSet<CycleAction> CycleActions => Set<CycleAction>();

    public DbSet<PracticeRecord> PracticeRecords => Set<PracticeRecord>();

    public DbSet<ReflectionEntry> ReflectionEntries => Set<ReflectionEntry>();

    public DbSet<SkillInventory> SkillInventories => Set<SkillInventory>();

    public DbSet<SkillInventoryEntry> SkillInventoryEntries => Set<SkillInventoryEntry>();

    public DbSet<SkillTag> SkillTags => Set<SkillTag>();

    public DbSet<SkillSuggestionDraft> SkillSuggestionDrafts => Set<SkillSuggestionDraft>();

    public DbSet<SkillSuggestionTask> SkillSuggestionTasks => Set<SkillSuggestionTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.Property(skill => skill.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(skill => skill.CreatedAt)
                .IsRequired();

            entity.HasIndex(skill => skill.CreatedAt);
        });

        modelBuilder.Entity<Cycle>(entity =>
        {
            entity.Property(cycle => cycle.Sequence)
                .IsRequired();

            entity.Property(cycle => cycle.CreatedAt)
                .IsRequired();

            entity.Property(cycle => cycle.CurrentLevel)
                .HasMaxLength(64);

            entity.Property(cycle => cycle.TargetLevel)
                .HasMaxLength(64);

            entity.HasIndex(cycle => new { cycle.SkillId, cycle.Sequence });

            entity.HasOne(cycle => cycle.Skill)
                .WithMany()
                .HasForeignKey(cycle => cycle.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CycleAction>(entity =>
        {
            entity.ToTable("Actions");

            entity.Property(action => action.Body)
                .IsRequired()
                .HasMaxLength(4000);

            entity.HasIndex(action => action.CycleId)
                .IsUnique();

            entity.HasOne(action => action.Cycle)
                .WithMany()
                .HasForeignKey(action => action.CycleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PracticeRecord>(entity =>
        {
            entity.Property(record => record.Body)
                .IsRequired()
                .HasMaxLength(4000);

            entity.Property(record => record.RecordedAt)
                .IsRequired();

            entity.HasIndex(record => new { record.CycleId, record.RecordedAt });

            entity.HasOne(record => record.Cycle)
                .WithMany()
                .HasForeignKey(record => record.CycleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReflectionEntry>(entity =>
        {
            entity.ToTable("Reflections");

            entity.Property(reflection => reflection.Body)
                .IsRequired()
                .HasMaxLength(4000);

            entity.HasIndex(reflection => reflection.CycleId)
                .IsUnique();

            entity.HasOne(reflection => reflection.Cycle)
                .WithMany()
                .HasForeignKey(reflection => reflection.CycleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SkillInventory>(entity =>
        {
            entity.Property(inventory => inventory.CompletedAt)
                .IsRequired();

            entity.HasIndex(inventory => inventory.CompletedAt);
        });

        modelBuilder.Entity<SkillInventoryEntry>(entity =>
        {
            entity.Property(entry => entry.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.HasIndex(entry => entry.InventoryId);
            entity.HasIndex(entry => entry.SkillId);

            entity.HasOne(entry => entry.Inventory)
                .WithMany()
                .HasForeignKey(entry => entry.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(entry => entry.Skill)
                .WithMany()
                .HasForeignKey(entry => entry.SkillId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SkillTag>(entity =>
        {
            entity.Property(tag => tag.Name)
                .IsRequired()
                .HasMaxLength(64);

            entity.HasIndex(tag => new { tag.SkillId, tag.Name })
                .IsUnique();

            entity.HasOne(tag => tag.Skill)
                .WithMany()
                .HasForeignKey(tag => tag.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SkillSuggestionDraft>(entity =>
        {
            entity.Property(draft => draft.EntryPoint)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(draft => draft.StartDate)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(draft => draft.EndDate)
                .HasMaxLength(10);

            entity.Property(draft => draft.PeriodLabel)
                .IsRequired()
                .HasMaxLength(200);
        });

        modelBuilder.Entity<SkillSuggestionTask>(entity =>
        {
            entity.Property(task => task.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(task => task.Time)
                .HasMaxLength(64);

            entity.Property(task => task.Difficulty)
                .HasMaxLength(64);

            entity.Property(task => task.Dependency)
                .HasMaxLength(64);

            entity.Property(task => task.Rework)
                .HasMaxLength(64);

            entity.Property(task => task.Improvement)
                .HasMaxLength(64);

            entity.HasIndex(task => task.DraftId);

            entity.HasOne(task => task.Draft)
                .WithMany(draft => draft.Tasks)
                .HasForeignKey(task => task.DraftId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
