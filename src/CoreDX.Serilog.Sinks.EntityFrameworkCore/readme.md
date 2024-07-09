## About

Serilog sink based on EntityFrameworkCore to persist logs.

## How to Use

### YourDbContext
``` csharp
public class YourLogRecord : LogRecord
{
    public int YourProperty { get; set; }
}

public class YourApplicationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add entity type to DbContext with default entity type.
        modelBuilder.UseLogRecord(b =>
        {
            b.ToTable($"{nameof(LogRecord)}s");
        });

        // Add entity type to DbContext with custom entity type based on LogRecord.
        modelBuilder.UseLogRecord<YourLogRecord>(b =>
        {
            b.ToTable($"{nameof(YourLogRecord)}s");
        });
    }
}

public class YourLogDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add entity type to DbContext with default entity type.
        modelBuilder.UseLogRecord(b =>
        {
            b.ToTable($"{nameof(LogRecord)}s", tb => tb.ExcludeFromMigrations());
        });

        // Add entity type to DbContext with custom entity type based on LogRecord.
        modelBuilder.UseLogRecord<YourLogRecord>(b =>
        {
            b.ToTable($"{nameof(YourLogRecord)}s", tb => tb.ExcludeFromMigrations());
        });
    }
}

```

### ServiceCollection
``` csharp
// Add DbContext to ServiceCollection to access database.
services.AddDbContext<YourApplicationDbContext>(options =>
{
    options.UseSqlite("app.db")
});

services.AddDbContext<YourLogDbContext>(options =>
{
    // important!
    // Suppress SQL command execution logs for EF Core so that this context does not write logs representing SQL commands that insert logs, eliminating infinite loop.
    options.ConfigureWarnings(b => b.Ignore(RelationalEventId.CommandExecuted, RelationalEventId.CommandError));

    options.UseSqlite("app.db")
});

// Add services.
services.AddMinimumLevelOverridableSerilogFilterConfigurationMonitorManager();
```

### Program
``` csharp
public static IHostBuilder CreateHostBuilder(string[] args) => CreateHostBuilderAdvance(args, null);
    Host.CreateDefaultBuilder(args)
        .UseSerilog((hostBuilder, serviceProvider, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(hostBuilder.Configuration)
                .ReadFrom.Services(serviceProvider)
                .WriteTo.Logger(internalConfiguration =>
                {
                    internalConfiguration
                        .Filter.ByIncludingOnly(
                            new MinimumLevelOverridableSerilogFilterConfigurationMonitor(
                                serviceProvider,
                                "SerilogFilterExtensions:EntityFrameworkCore"
                            ).Filter)
                        // using default entity type
                        .WriteTo.EntityFrameworkCore(
                            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                            static sp => sp.GetRequiredService<YourLogDbContext>(),
                            new()
                            {
                                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                            });
                        // using custom entity type
                        .WriteTo.EntityFrameworkCore<YourLogDbContext, YourLogRecord>(
                            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
                            static sp => sp.GetRequiredService<YourLogDbContext>(),
                            new()
                            {
                                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                            });
                });

        }, writeToProviders: true);
```

### appsttings.json sample
``` json
{
  "SerilogFilterExtensions": {
    "EntityFrameworkCore": {
      "Default": "Warning",
      "Override": {
        "Microsoft.AspNetCore.DataProtection.KeyManagement": "Error",
        "Microsoft.AspNetCore.DataProtection.Repositories": "Error",
        "Microsoft.EntityFrameworkCore.Database.Command": "Error",
        "Microsoft.EntityFrameworkCore.Model.Validation": "Error"
      }
    }
  }
}
```