using System;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

namespace BlazorServer;

public static class Program
{
    private const string hostUrl = "http://0.0.0.0:5000";

    public static void Main(string[] args)
    {
        while (true)
        {
            Log.Information($"[{nameof(Program),-15}] Starting blazor server");
            try
            {
                IHost host = CreateHostBuilder(args).Build();
                var logger = host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger>();

                AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs args) =>
                {
                    Exception e = (Exception)args.ExceptionObject;
                    logger.LogError(e, e.Message);
                };

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Information($"[{nameof(Program),-15}] {ex.Message}");
                Log.Information("");
                System.Threading.Thread.Sleep(3000);
            }
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls(hostUrl);
                webBuilder.ConfigureLogging(logging =>
                    logging.ClearProviders().AddSerilog());
                webBuilder.UseStartup<Startup>();
            });
}