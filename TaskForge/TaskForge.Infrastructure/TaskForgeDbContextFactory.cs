using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TaskForge.Infrastructure.Data;

public class TaskForgeDbContextFactory : IDesignTimeDbContextFactory<TaskForgeDbContext>
{
    public TaskForgeDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Server=localhost,1433;Database=taskforgelocal;User Id=sa;Password=passworD1#;TrustServerCertificate=True;Max Pool Size=200;Min Pool Size=5;Command Timeout=30;Pooling=true;";

        var optionsBuilder = new DbContextOptionsBuilder<TaskForgeDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new TaskForgeDbContext(optionsBuilder.Options);
    }
}
