using System.Globalization;
using BlazorNullReferenceWithSerilog;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.BrowserConsole(
        restrictedToMinimumLevel: LogEventLevel.Information,
        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
        CultureInfo.InvariantCulture)
    .CreateLogger();

try
{
    WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>(selector: "#app");
    builder.RootComponents.Add<HeadOutlet>(selector: "head::after");

    builder.Logging.AddSerilog();

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

    await builder.Build().RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, messageTemplate: "An exception occurred while creating the WASM host");
    throw;
}
