using Auth0.OidcClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;
using System.Windows;

namespace TaskForge.WPF
{
    public partial class App : Application
    {
        public static Auth0Client Auth0Client { get; private set; }

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

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  // Шлях до папки запуску (bin/Debug/netX.0)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // optional: false - файл обов'язковий
                .Build();

            string domain = configuration["Auth0:Domain"];
            string clientId = configuration["Auth0:ClientId"];
            Auth0Client = new Auth0Client(new Auth0ClientOptions
            {
                Domain = domain,
                ClientId = clientId,
                RedirectUri = "http://localhost/callback",
                PostLogoutRedirectUri = "http://localhost"
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Додаток завершує роботу");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}