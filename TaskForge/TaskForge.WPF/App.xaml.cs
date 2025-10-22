using Microsoft.Extensions.Configuration;
using Serilog;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TaskForge.Infrastructure.Repositories;
using TaskForge.Infrastructure.Data;
using TaskForge.Application.Services;
using TaskForge.Domain.Interfaces;
using TaskForge.Application.Interfaces;

namespace TaskForge.WPF
{
    public partial class App : System.Windows.Application 
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<Auth0Service>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IProjectRepository, ProjectRepository>();
            services.AddSingleton<ITaskRepository, TaskRepository>();
            services.AddSingleton<IProjectService, ProjectService>(); 
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ITaskService, TaskService>();

            // Register DbContext
            services.AddDbContext<TaskForgeDbContext>(options =>
                options.UseSqlServer("Server=localhost,1433;Database=taskforgelocal;User Id=sa;Password=passworD1#;TrustServerCertificate=True;Max Pool Size=200;Min Pool Size=5;Command Timeout=30;Pooling=true;"));

            // Register MainWindow
            services.AddTransient<MainWindow>();

            _serviceProvider = services.BuildServiceProvider();
        }

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

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Додаток завершує роботу");
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}