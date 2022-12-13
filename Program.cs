using System.Globalization;
using BlazorNullReferenceWithSerilog;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// If we comment this line the app runs fine
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.BrowserConsole(
        restrictedToMinimumLevel: LogEventLevel.Information,
        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
        CultureInfo.InvariantCulture)
    .CreateLogger();
/////////////////////////////////////////////////////////////////////////

try
{
    WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>(selector: "#app");
    builder.RootComponents.Add<HeadOutlet>(selector: "head::after");

    builder.Logging.AddSerilog();

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

    builder.Services
        .AddDbContextFactory<ADbContext>(opts =>
        {
            _ = opts
                .UseInMemoryDatabase(databaseName: "PrototypeDatabase")
                //.EnableSensitiveDataLogging()
                .LogTo(Log.Information, LogLevel.Information);
        });

    WebAssemblyHost app = builder.Build();

    // This block seems to be part of the problem, if we comment this block the code runs fine
    ADbContext dbContext = app.Services.GetRequiredService<ADbContext>();
    dbContext.Entities.Add(new EntityA());
    await dbContext.SaveChangesAsync(CancellationToken.None);
    //////////////////////////////////////////////////////////////////////////////////////////

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, messageTemplate: "An exception occurred while creating the WASM host");
    throw;
}

public class ADbContext : DbContext
{
    public ADbContext(DbContextOptions<ADbContext> options) : base(options)
    {
    }

    public DbSet<EntityA> Entities => Set<EntityA>();
}

public class EntityA
{
    public int Id { get; set; }
}
