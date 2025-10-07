using Serilog;
using System.Configuration;
using System.Data;
using System.Windows;
using Serilog.Enrichers;

namespace TaskForge.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Налаштування глобального логера
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()  // Рівень логування (Debug, Information, Warning, Error, Fatal)
                .WriteTo.Console()     // Виведення в консоль для розробки
                .WriteTo.Seq("http://localhost:5341")  // Відправка в Seq (замініть URL, якщо інший)
                .Enrich.WithMachineName()  // Додає назву машини до логів
                .Enrich.WithThreadId()    // Додає ID потоку
                .CreateLogger();

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Information("Додаток завершує роботу");
            Log.CloseAndFlush();  // Закриваємо логер при виході
            base.OnExit(e);
        }
    }

}
