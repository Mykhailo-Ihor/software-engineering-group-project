namespace TaskForge.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;

public class TaskForgeDbContext : DbContext
{
    public TaskForgeDbContext(DbContextOptions<TaskForgeDbContext> options)
        : base(options)
    {
    }

    // Тут будуть DbSet-и, поки пусто
}