using Data;
using Jobs;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.AspNetCore;
using Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<WattDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Local"))
);

builder.Services.AddHttpClient(
    "Wattpad",
    client =>
    {
        client.BaseAddress = new Uri("https://www.wattpad.com/api/v3/");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3"
        );
    }
);

builder.Services.AddQuartz(q =>
{
    var jobkey = new JobKey("WattpadStoryJob");

    q.AddJob<ScrapStoryJob>(opts => opts.WithIdentity(jobkey));

    q.AddTrigger(opts =>
        opts.ForJob(jobkey)
            .WithIdentity("WattpadStoryJob-trigger")
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(30).RepeatForever())
    );
});
builder.Services.AddQuartzServer(options => options.WaitForJobsToComplete = true);

builder.Services.AddScoped<StoriesService>();

builder.Services.AddSingleton<ScrapperService>();

// Add Razor Pages support
builder.Services.AddRazorPages();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<WattDbContext>();
    if (context.Database.GetPendingMigrations().Any())
    {
        context.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Serve static files (wwwroot) and enable Razor Pages
app.UseStaticFiles();

app.UseHttpsRedirection();

// Map Razor Pages
app.MapRazorPages();

app.MapControllers();

// Open browser after app starts
app.Lifetime.ApplicationStarted.Register(() =>
{
    var url = app.Urls.FirstOrDefault() ?? "http://localhost:5000";
    OpenBrowser(url);
});

app.Run();

static void OpenBrowser(string url)
{
    try
    {
        if (OperatingSystem.IsWindows())
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo { FileName = url, UseShellExecute = true }
            );
        }
        else if (OperatingSystem.IsLinux())
        {
            System.Diagnostics.Process.Start("xdg-open", url);
        }
        else if (OperatingSystem.IsMacOS())
        {
            System.Diagnostics.Process.Start("open", url);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unable to open browser: {ex.Message}");
    }
}
