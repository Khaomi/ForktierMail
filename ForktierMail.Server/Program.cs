using ForktierMail.Database;
using ForktierMail.Server.Manager;
using ForktierMail.Server.Middleware;
using Microsoft.EntityFrameworkCore;

namespace ForktierMail.Server;

public class Application
{
    public static string ROOT = AppDomain.CurrentDomain.BaseDirectory;

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var dbProvider = builder.Configuration.GetValue<string>("Database:Provider") ?? "Sqlite";
        var connectionString = builder.Configuration.GetValue<string>("Database:ConnectionString");

        builder.Services
            .AddPooledDbContextFactory<ServerDbContext>(options =>
            {
                if (dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseNpgsql(connectionString);
                }
                else
                {
                    var sqliteConn = connectionString ?? $"Data Source={Path.Combine(ROOT, "sqlite.db")}";
                    options.UseSqlite(sqliteConn);
                }
            });

        builder.Services
            .AddSingleton<ServerDataManager>();

        builder.Services
            .AddSignalR()
            .AddMessagePackProtocol();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ServerDbContext>>();

            using var context = dbFactory.CreateDbContext();
            await context.Database.EnsureCreatedAsync();

            var manager = scope.ServiceProvider.GetRequiredService<ServerDataManager>();
            try
            {
                // await manager.Seed();
                await manager.LoadFromDatabase();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Application>>();
                logger.LogError(ex, "Failed to load data from database on startup");
                throw;
            }
        }

        app.UseMiddleware<APIKeyMiddleware>();
        app.MapHub<MailHub>("/Mail");

        app.Run();
    }
}