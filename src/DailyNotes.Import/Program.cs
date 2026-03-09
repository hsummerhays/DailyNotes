using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using DailyNotes.Core.Entities;
using DailyNotes.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

// ───────────────────────────────────────────────────────────────────
// DailyNotes CSV Import Tool
// Reads CSV exports from FileMaker and inserts into PostgreSQL.
// Usage: dotnet run --project src/DailyNotes.Import -- --csv-dir ./data
// ───────────────────────────────────────────────────────────────────

string csvDir = "./dataImport";

// Parse --csv-dir argument
for (int i = 0; i < args.Length - 1; i++)
{
    if (args[i] == "--csv-dir") csvDir = args[i + 1];
}

if (!Directory.Exists(csvDir))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Error: CSV directory not found: {Path.GetFullPath(csvDir)}");
    Console.ResetColor();
    Console.WriteLine("Usage: dotnet run --project src/DailyNotes.Import -- --csv-dir ./data");
    return 1;
}

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════╗");
Console.WriteLine("║     DailyNotes CSV Import Tool       ║");
Console.WriteLine("╚══════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine($"CSV directory: {Path.GetFullPath(csvDir)}");
Console.WriteLine();

// ── Build DbContext ──────────────────────────────────────────────
var connString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=DailyNotes;Username=postgres;Password=";

var services = new ServiceCollection();
services.AddDbContext<DailyNotesDbContext>(options =>
    options.UseNpgsql(connString));
services.AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<DailyNotesDbContext>();

var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<DailyNotesDbContext>();

// Ensure database + migrations
await db.Database.MigrateAsync();

// ── Clear existing data (optional, for idempotency in this dev tool) ─────────────
Console.WriteLine("Clearing existing data...");
db.WorkNotes.RemoveRange(db.WorkNotes);
db.WorkTasks.RemoveRange(db.WorkTasks);
db.WorkDays.RemoveRange(db.WorkDays);
db.Projects.RemoveRange(db.Projects);
db.PayPeriods.RemoveRange(db.PayPeriods);
await db.SaveChangesAsync();
Console.WriteLine("✓ Data cleared.");

// ── Create default tenant & user ─────────────────────────────────
var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Name == "Imported");
if (tenant == null)
{
    tenant = new Tenant { Name = "Imported", CreatedAt = DateTime.UtcNow };
    db.Tenants.Add(tenant);
    await db.SaveChangesAsync();
    Console.WriteLine($"✓ Created tenant: \"{tenant.Name}\" (ID: {tenant.Id})");
}
else
{
    Console.WriteLine($"✓ Using existing tenant: \"{tenant.Name}\" (ID: {tenant.Id})");
}

int tenantId = tenant.Id;
// ── Get or Create User ───────────────────────────────────────────
string userEmail = "hsummerhays1@gmail.com";
var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
var user = await userManager.FindByEmailAsync(userEmail);

if (user == null)
{
    user = new IdentityUser { UserName = userEmail, Email = userEmail, EmailConfirmed = true };
    var result = await userManager.CreateAsync(user, "REPLACED_PASSWORD");
    if (!result.Succeeded)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error creating user {userEmail}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        Console.ResetColor();
        return 1;
    }
    Console.WriteLine($"✓ Created user: \"{user.Email}\" (ID: {user.Id})");
}
else
{
    Console.WriteLine($"✓ Using existing user: \"{user.Email}\" (ID: {user.Id})");
}

// Ensure TenantUser link exists
var tenantUser = await db.TenantUsers.FirstOrDefaultAsync(tu => tu.TenantId == tenant.Id && tu.UserId == user.Id);
if (tenantUser == null)
{
    tenantUser = new TenantUser { TenantId = tenant.Id, UserId = user.Id, Role = "owner", CreatedAt = DateTime.UtcNow };
    db.TenantUsers.Add(tenantUser);
    await db.SaveChangesAsync();
    Console.WriteLine($"✓ Linked user to tenant \"{tenant.Name}\"");
}

string userId = user.Id;


// ── Helper: Safe date parsing ────────────────────────────────────
// ── Helper: Safe date parsing ────────────────────────────────────
DateTime? ParseDate(string? value, string fieldName = "Date")
{
    if (string.IsNullOrWhiteSpace(value)) return null;

    // Try standard formats first
    if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    // fallback to US format explicitly if needed
    if (DateTime.TryParseExact(value, ["M/d/yyyy", "M/d/yy", "MM/dd/yyyy", "MM/dd/yy", "yyyy-MM-dd", "d/M/yyyy", "dd/MM/yyyy"],
        CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
        return DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine($"  [!] Failed to parse date '{value}' for field '{fieldName}'");
    Console.ResetColor();
    return null;
}

int? ParseInt(string? value)
{
    if (string.IsNullOrWhiteSpace(value)) return null;
    return int.TryParse(value, out var i) ? i : null;
}

var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HasHeaderRecord = true,
    MissingFieldFound = null,
    HeaderValidated = null,
    BadDataFound = null,
    TrimOptions = TrimOptions.Trim,
};

int totalImported = 0;

// ── 1. Import Projects ──────────────────────────────────────────
var projectsFile = Path.Combine(csvDir, "Projects.csv");
var projectIdMap = new Dictionary<string, int>(); // FM ID → DB ID

if (File.Exists(projectsFile))
{
    Console.Write("Importing Projects...");
    using var reader = new StreamReader(projectsFile);
    using var csv = new CsvReader(reader, csvConfig);
    var records = csv.GetRecords<dynamic>().ToList();
    int count = 0;

    foreach (var row in records)
    {
        var dict = (IDictionary<string, object>)row;
        var createdDateParsed = ParseDate(dict.ContainsKey("CreatedDate") ? dict["CreatedDate"]?.ToString() : null, "CreatedDate");
        var completedDateParsed = ParseDate(dict.ContainsKey("CompletedDate") ? dict["CompletedDate"]?.ToString() : null, "CompletedDate");
        var project = new Project
        {
            TenantId = tenantId,
            UserId = userId,
            Name = dict.ContainsKey("Name") ? dict["Name"]?.ToString() ?? "" : "",
            Category = dict.ContainsKey("Category") ? dict["Category"]?.ToString() : null,
            CreatedDate = createdDateParsed.HasValue ? DateOnly.FromDateTime(createdDateParsed.Value) : null,
            CompletedDate = completedDateParsed.HasValue ? DateOnly.FromDateTime(completedDateParsed.Value) : null,
            CreatedAt = DateTime.UtcNow,
        };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        // Map FileMaker Project ID to new DB ID
        string fmId = dict.ContainsKey("Project ID") ? dict["Project ID"]?.ToString() ?? "" :
                       dict.ContainsKey("ProjectID") ? dict["ProjectID"]?.ToString() ?? "" :
                       dict.ContainsKey("Id") ? dict["Id"]?.ToString() ?? "" : project.Id.ToString();
        projectIdMap[fmId] = project.Id;
        count++;
    }
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($" {count} records");
    Console.ResetColor();
    totalImported += count;
}

// ── 2. Import Tasks ─────────────────────────────────────────────
var tasksFile = Path.Combine(csvDir, "Tasks.csv");
var taskIdMap = new Dictionary<string, int>(); // FM ID → DB ID

if (File.Exists(tasksFile))
{
    Console.Write("Importing Tasks...");
    using var reader = new StreamReader(tasksFile);
    using var csv = new CsvReader(reader, csvConfig);
    var records = csv.GetRecords<dynamic>().ToList();
    int count = 0;

    foreach (var row in records)
    {
        var dict = (IDictionary<string, object>)row;
        string? fmProjectId = dict.ContainsKey("Project ID") ? dict["Project ID"]?.ToString() :
                               dict.ContainsKey("ProjectID") ? dict["ProjectID"]?.ToString() : null;

        int? projectId = null;
        if (!string.IsNullOrEmpty(fmProjectId))
        {
            if (projectIdMap.TryGetValue(fmProjectId, out var mappedId))
                projectId = mappedId;
            else
                Console.WriteLine($"  [!] Project ID '{fmProjectId}' not found for task '{dict["Name"]?.ToString()}'");
        }

        var startDateVal = ParseDate(dict.ContainsKey("StartDate") ? dict["StartDate"]?.ToString() :
                                    dict.ContainsKey("Start Date") ? dict["Start Date"]?.ToString() : null, "StartDate");
        var dueDateVal = ParseDate(dict.ContainsKey("DueDate") ? dict["DueDate"]?.ToString() :
                                dict.ContainsKey("Due Date") ? dict["Due Date"]?.ToString() : null, "DueDate");

        var task = new WorkTask
        {
            TenantId = tenantId,
            UserId = userId,
            ProjectId = projectId,
            Name = dict.ContainsKey("Name") ? dict["Name"]?.ToString() ?? "" :
                   dict.ContainsKey("Task Name") ? dict["Task Name"]?.ToString() ?? "" : "",
            Status = dict.ContainsKey("Status") ? dict["Status"]?.ToString() ?? "pending" : "pending",
            Comments = dict.ContainsKey("Comments") ? dict["Comments"]?.ToString() : null,
            StartDate = startDateVal.HasValue ? DateOnly.FromDateTime(startDateVal.Value) : null,
            DueDate = dueDateVal.HasValue ? DateOnly.FromDateTime(dueDateVal.Value) : null,
            CreatedAt = DateTime.UtcNow,
        };
        db.WorkTasks.Add(task);
        await db.SaveChangesAsync();

        string fmId = dict.ContainsKey("Task ID") ? dict["Task ID"]?.ToString() ?? "" :
                       dict.ContainsKey("TaskID") ? dict["TaskID"]?.ToString() ?? "" :
                       dict.ContainsKey("Id") ? dict["Id"]?.ToString() ?? "" : task.Id.ToString();
        taskIdMap[fmId] = task.Id;
        count++;
    }
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($" {count} records");
    Console.ResetColor();
    totalImported += count;
}

// ── 3. Import Work Days ─────────────────────────────────────────
var workDaysFile = Path.Combine(csvDir, "Work Days.csv");
if (!File.Exists(workDaysFile))
    workDaysFile = Path.Combine(csvDir, "WorkDays.csv");

if (File.Exists(workDaysFile))
{
    Console.Write("Importing Work Days...");
    using var reader = new StreamReader(workDaysFile);
    using var csv = new CsvReader(reader, csvConfig);
    var records = csv.GetRecords<dynamic>().ToList();
    int count = 0;


    // ── Helper: Safe time parsing ────────────────────────────────────
    TimeOnly? ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        // Check if it's "16" meaning 16 hours? No, likely bad data.
        // Standard formats: H:mm:ss, h:mm tt
        if (TimeOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
            return t;

        // Handle "8 AM", "4:30 pm"
        string[] formats = ["h:mm tt", "h tt", "H:mm", "H:mm:ss", "h:mm:ss tt"];
        if (TimeOnly.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out t))
            return t;

        // Fallback: Try parsing as DateTime then extract Time
        var dt = ParseDate(value, "TimeCheck");
        if (dt.HasValue) return TimeOnly.FromDateTime(dt.Value);

        return null;
    }

    // ... existing code ...

    foreach (var row in records)
    {
        var dict = (IDictionary<string, object>)row;

        var workDateVal = ParseDate(dict.ContainsKey("Date") ? dict["Date"]?.ToString() :
                                  dict.ContainsKey("WorkDate") ? dict["WorkDate"]?.ToString() : null, "WorkDate");

        var wd = new WorkDay
        {
            TenantId = tenantId,
            UserId = userId,
            WorkDate = workDateVal.HasValue ? DateOnly.FromDateTime(workDateVal.Value) : DateOnly.FromDateTime(DateTime.UtcNow),
            TimeIn1 = ParseTime(dict.ContainsKey("Time In") ? dict["Time In"]?.ToString() :
                               dict.ContainsKey("TimeIn1") ? dict["TimeIn1"]?.ToString() : null),
            TimeOut1 = ParseTime(dict.ContainsKey("Time Out") ? dict["Time Out"]?.ToString() :
                                dict.ContainsKey("TimeOut1") ? dict["TimeOut1"]?.ToString() : null),
            TimeIn2 = ParseTime(dict.ContainsKey("TimeIn2") ? dict["TimeIn2"]?.ToString() : null),
            TimeOut2 = ParseTime(dict.ContainsKey("TimeOut2") ? dict["TimeOut2"]?.ToString() : null),
            TimeIn3 = ParseTime(dict.ContainsKey("TimeIn3") ? dict["TimeIn3"]?.ToString() : null),
            TimeOut3 = ParseTime(dict.ContainsKey("TimeOut3") ? dict["TimeOut3"]?.ToString() : null),
            BreakMinutes = ParseInt(dict.ContainsKey("Break Minutes") ? dict["Break Minutes"]?.ToString() :
                                     dict.ContainsKey("BreakMinutes") ? dict["BreakMinutes"]?.ToString() : null) ?? 0,
            Comments = dict.ContainsKey("Comments") ? dict["Comments"]?.ToString() : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WorkDays.Add(wd);
        count++;

        // Batch save every 500 records for large datasets
        if (count % 500 == 0) await db.SaveChangesAsync();
    }
    await db.SaveChangesAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($" {count} records");
    Console.ResetColor();
    totalImported += count;
}

// ── 4. Import Notes ─────────────────────────────────────────────
var notesFile = Path.Combine(csvDir, "Notes.csv");
if (File.Exists(notesFile))
{
    Console.Write("Importing Notes...");

    // Cache existing WorkDates to avoid FK violations
    var existingDates = new HashSet<DateOnly>(await db.WorkDays.Select(w => w.WorkDate).ToListAsync());

    using var reader = new StreamReader(notesFile);
    using var csv = new CsvReader(reader, csvConfig);
    var records = csv.GetRecords<dynamic>().ToList();
    int count = 0;

    foreach (var row in records)
    {
        var dict = (IDictionary<string, object>)row;
        string? fmTaskId = dict.ContainsKey("Task ID") ? dict["Task ID"]?.ToString() :
                            dict.ContainsKey("TaskID") ? dict["TaskID"]?.ToString() : null;

        int? taskId = null;
        if (!string.IsNullOrEmpty(fmTaskId))
        {
            if (taskIdMap.TryGetValue(fmTaskId, out var mappedId))
                taskId = mappedId;
        }

        string? noteContent = dict.ContainsKey("Note") ? dict["Note"]?.ToString() :
                              dict.ContainsKey("Content") ? dict["Content"]?.ToString() : null;

        var noteDateVal = ParseDate(dict.ContainsKey("Date") ? dict["Date"]?.ToString() :
                                  dict.ContainsKey("NoteDate") ? dict["NoteDate"]?.ToString() : null, "NoteDate");

        var noteDate = noteDateVal.HasValue ? DateOnly.FromDateTime(noteDateVal.Value) : DateOnly.FromDateTime(DateTime.UtcNow);

        // Ensure WorkDay exists
        if (!existingDates.Contains(noteDate))
        {
            var newWorkDay = new WorkDay
            {
                TenantId = tenantId,
                UserId = userId,
                WorkDate = noteDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.WorkDays.Add(newWorkDay);
            existingDates.Add(noteDate);
        }

        var note = new WorkNote
        {
            TenantId = tenantId,
            UserId = userId,
            WorkTaskId = taskId,
            NoteDate = noteDate,
            Content = JsonDocument.Parse(JsonSerializer.Serialize(new { text = noteContent ?? "" })),
            TimeMinutes = ParseInt(dict.ContainsKey("Time") ? dict["Time"]?.ToString() :
                                    dict.ContainsKey("TimeMinutes") ? dict["TimeMinutes"]?.ToString() :
                                    dict.ContainsKey("Time Minutes") ? dict["Time Minutes"]?.ToString() : null) ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WorkNotes.Add(note);
        count++;

        // Batch save every 1000 records for large datasets
        if (count % 1000 == 0)
        {
            await db.SaveChangesAsync();
            Console.Write($"\rImporting Notes... {count:N0}");
        }
    }
    await db.SaveChangesAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"\rImporting Notes... {count:N0} records");
    Console.ResetColor();
    totalImported += count;
}

// ── 5. Import Pay Periods ───────────────────────────────────────
var payPeriodsFile = Path.Combine(csvDir, "Pay Period.csv");
if (!File.Exists(payPeriodsFile))
    payPeriodsFile = Path.Combine(csvDir, "PayPeriods.csv");

if (File.Exists(payPeriodsFile))
{
    Console.Write("Importing Pay Periods...");
    using var reader = new StreamReader(payPeriodsFile);
    using var csv = new CsvReader(reader, csvConfig);
    var records = csv.GetRecords<dynamic>().ToList();
    int count = 0;

    foreach (var row in records)
    {
        var dict = (IDictionary<string, object>)row;
        string? startStr = dict.ContainsKey("Period Start Date") ? dict["Period Start Date"]?.ToString() :
                           dict.ContainsKey("StartDate") ? dict["StartDate"]?.ToString() :
                           dict.ContainsKey("Period Start") ? dict["Period Start"]?.ToString() :
                           dict.ContainsKey("PeriodStartDate") ? dict["PeriodStartDate"]?.ToString() : null;
        string? endStr = dict.ContainsKey("Period End Date") ? dict["Period End Date"]?.ToString() :
                         dict.ContainsKey("EndDate") ? dict["EndDate"]?.ToString() :
                         dict.ContainsKey("Period End") ? dict["Period End"]?.ToString() :
                         dict.ContainsKey("PeriodEndDate") ? dict["PeriodEndDate"]?.ToString() : null;

        var startDate = ParseDate(startStr, "PeriodStartDate");
        var endDate = ParseDate(endStr, "PeriodEndDate");

        var pp = new PayPeriod
        {
            TenantId = tenantId,
            UserId = userId,
            PeriodStartDate = startDate.HasValue ? DateOnly.FromDateTime(startDate.Value) : DateOnly.FromDateTime(DateTime.UtcNow),
            PeriodEndDate = endDate.HasValue ? DateOnly.FromDateTime(endDate.Value) : DateOnly.FromDateTime(DateTime.UtcNow),
            Holidays = ParseInt(dict.ContainsKey("Holidays") ? dict["Holidays"]?.ToString() : null) ?? 0,
            PtoReported = ParseInt(dict.ContainsKey("PtoReported") ? dict["PtoReported"]?.ToString() : null) ?? 0,
            PtoDaysOfMonth = dict.ContainsKey("PtoDaysOfMonth") ? dict["PtoDaysOfMonth"]?.ToString() : null,
            CreatedAt = DateTime.UtcNow,
        };
        db.PayPeriods.Add(pp);
        count++;

        if (count % 200 == 0) await db.SaveChangesAsync();
    }
    await db.SaveChangesAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($" {count} records");
    Console.ResetColor();
    totalImported += count;
}

// ── Summary ─────────────────────────────────────────────────────
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"═══ Import complete: {totalImported:N0} total records ═══");
Console.ResetColor();

return 0;
