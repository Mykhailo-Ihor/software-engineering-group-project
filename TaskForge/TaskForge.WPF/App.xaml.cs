using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;

namespace TaskForge.WPF
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .CreateLogger();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Додаток завершує роботу");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}