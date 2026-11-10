using Microsoft.EntityFrameworkCore;
using SkillManagementApp.Components;
using SkillManagementApp.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var provider = builder.Configuration["Database:Provider"] ?? "Sqlite";
if (!string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException("初期構成のデータベースは SQLite だけです。");
}

Directory.CreateDirectory(SqlitePaths.DataDirectory);
var databasePath = SqlitePaths.DatabasePath;
builder.Services.AddSingleton(new DatabaseLocation(databasePath));
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={databasePath}"));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    db.Database.EnsureCreated();
    await db.Database.ExecuteSqlRawAsync("""
        CREATE TABLE IF NOT EXISTS Skills (
            Id INTEGER NOT NULL CONSTRAINT PK_Skills PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_Skills_CreatedAt ON Skills (CreatedAt);
        CREATE TABLE IF NOT EXISTS Cycles (
            Id INTEGER NOT NULL CONSTRAINT PK_Cycles PRIMARY KEY AUTOINCREMENT,
            SkillId INTEGER NOT NULL,
            Sequence INTEGER NOT NULL,
            CurrentLevel TEXT NULL,
            TargetLevel TEXT NULL,
            StartDate TEXT NULL,
            EndDate TEXT NULL,
            CreatedAt TEXT NOT NULL,
            CONSTRAINT FK_Cycles_Skills_SkillId
                FOREIGN KEY (SkillId) REFERENCES Skills (Id) ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS IX_Cycles_SkillId_Sequence ON Cycles (SkillId, Sequence);
        CREATE TABLE IF NOT EXISTS Actions (
            Id INTEGER NOT NULL CONSTRAINT PK_Actions PRIMARY KEY AUTOINCREMENT,
            CycleId INTEGER NOT NULL,
            Body TEXT NOT NULL,
            CONSTRAINT FK_Actions_Cycles_CycleId
                FOREIGN KEY (CycleId) REFERENCES Cycles (Id) ON DELETE CASCADE
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Actions_CycleId ON Actions (CycleId);
        CREATE TABLE IF NOT EXISTS PracticeRecords (
            Id INTEGER NOT NULL CONSTRAINT PK_PracticeRecords PRIMARY KEY AUTOINCREMENT,
            CycleId INTEGER NOT NULL,
            Body TEXT NOT NULL,
            RecordedAt TEXT NOT NULL,
            CONSTRAINT FK_PracticeRecords_Cycles_CycleId
                FOREIGN KEY (CycleId) REFERENCES Cycles (Id) ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS IX_PracticeRecords_CycleId_RecordedAt ON PracticeRecords (CycleId, RecordedAt);
        CREATE TABLE IF NOT EXISTS Reflections (
            Id INTEGER NOT NULL CONSTRAINT PK_Reflections PRIMARY KEY AUTOINCREMENT,
            CycleId INTEGER NOT NULL,
            Body TEXT NOT NULL,
            NextCycleId INTEGER NULL,
            CONSTRAINT FK_Reflections_Cycles_CycleId
                FOREIGN KEY (CycleId) REFERENCES Cycles (Id) ON DELETE CASCADE
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_Reflections_CycleId ON Reflections (CycleId);
        CREATE TABLE IF NOT EXISTS SkillInventories (
            Id INTEGER NOT NULL CONSTRAINT PK_SkillInventories PRIMARY KEY AUTOINCREMENT,
            CompletedAt TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS IX_SkillInventories_CompletedAt ON SkillInventories (CompletedAt);
        CREATE TABLE IF NOT EXISTS SkillInventoryEntries (
            Id INTEGER NOT NULL CONSTRAINT PK_SkillInventoryEntries PRIMARY KEY AUTOINCREMENT,
            InventoryId INTEGER NOT NULL,
            SkillId INTEGER NOT NULL,
            Name TEXT NOT NULL,
            CONSTRAINT FK_SkillInventoryEntries_SkillInventories_InventoryId
                FOREIGN KEY (InventoryId) REFERENCES SkillInventories (Id) ON DELETE CASCADE,
            CONSTRAINT FK_SkillInventoryEntries_Skills_SkillId
                FOREIGN KEY (SkillId) REFERENCES Skills (Id) ON DELETE RESTRICT
        );
        CREATE INDEX IF NOT EXISTS IX_SkillInventoryEntries_InventoryId ON SkillInventoryEntries (InventoryId);
        CREATE INDEX IF NOT EXISTS IX_SkillInventoryEntries_SkillId ON SkillInventoryEntries (SkillId);
        CREATE TABLE IF NOT EXISTS SkillTags (
            Id INTEGER NOT NULL CONSTRAINT PK_SkillTags PRIMARY KEY AUTOINCREMENT,
            SkillId INTEGER NOT NULL,
            Name TEXT NOT NULL,
            CONSTRAINT FK_SkillTags_Skills_SkillId
                FOREIGN KEY (SkillId) REFERENCES Skills (Id) ON DELETE CASCADE
        );
        CREATE UNIQUE INDEX IF NOT EXISTS IX_SkillTags_SkillId_Name ON SkillTags (SkillId, Name);
        CREATE TABLE IF NOT EXISTS SkillSuggestionDrafts (
            Id INTEGER NOT NULL CONSTRAINT PK_SkillSuggestionDrafts PRIMARY KEY,
            EntryPoint TEXT NOT NULL,
            StartDate TEXT NOT NULL,
            EndDate TEXT NULL,
            PeriodLabel TEXT NOT NULL DEFAULT ''
        );
        CREATE TABLE IF NOT EXISTS SkillSuggestionTasks (
            Id INTEGER NOT NULL CONSTRAINT PK_SkillSuggestionTasks PRIMARY KEY AUTOINCREMENT,
            DraftId INTEGER NOT NULL,
            Name TEXT NOT NULL,
            Time TEXT NOT NULL,
            Difficulty TEXT NOT NULL,
            Dependency TEXT NOT NULL,
            Rework TEXT NOT NULL,
            Improvement TEXT NOT NULL,
            CONSTRAINT FK_SkillSuggestionTasks_SkillSuggestionDrafts_DraftId
                FOREIGN KEY (DraftId) REFERENCES SkillSuggestionDrafts (Id) ON DELETE CASCADE
        );
        CREATE INDEX IF NOT EXISTS IX_SkillSuggestionTasks_DraftId ON SkillSuggestionTasks (DraftId);
        """);
    try
    {
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE SkillSuggestionDrafts ADD COLUMN PeriodLabel TEXT NOT NULL DEFAULT '';");
    }
    catch (Microsoft.Data.Sqlite.SqliteException exception) when (exception.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
    {
    }

    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cycles ADD COLUMN StartDate TEXT NULL;");
    }
    catch (Microsoft.Data.Sqlite.SqliteException exception) when (exception.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
    {
    }

    try
    {
        await db.Database.ExecuteSqlRawAsync("ALTER TABLE Cycles ADD COLUMN EndDate TEXT NULL;");
    }
    catch (Microsoft.Data.Sqlite.SqliteException exception) when (exception.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
    {
    }
}

app.UseExceptionHandler("/Error", createScopeForErrors: true);
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
