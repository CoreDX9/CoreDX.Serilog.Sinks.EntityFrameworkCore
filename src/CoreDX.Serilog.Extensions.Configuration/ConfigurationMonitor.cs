using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Serilog.Events;

namespace CoreDX.Serilog.Extensions.Configuration;

/// <summary>
/// A monitor to watch changes of <see cref="IConfigurationSection"/> and re build log filter.
/// </summary>
public class MinimumLevelOverridableSerilogFilterConfigurationMonitor : IDisposable
{
    private readonly object _locker = new();
    private readonly IConfigurationSection? _section;

    private Func<LogEvent, bool>? _filter;
    private IDisposable? _configureChangeMonitor;

    /// <summary>
    /// Get log filter Function.
    /// </summary>
    public Func<LogEvent, bool> Filter => log => _filter?.Invoke(log) ?? true;

    /// <summary>
    /// Create an instance with specified configuration path.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <param name="configurationPath">The path of configuration section.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public MinimumLevelOverridableSerilogFilterConfigurationMonitor(IServiceProvider serviceProvider, string configurationPath)
    {
        if (serviceProvider is null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        if (string.IsNullOrEmpty(configurationPath))
        {
            throw new ArgumentException($"“{nameof(configurationPath)}”不能为 null 或空。", nameof(configurationPath));
        }

        _section = serviceProvider.GetRequiredService<IConfiguration>().GetSection(configurationPath);
        serviceProvider.GetRequiredService<MinimumLevelOverridableSerilogFilterConfigurationMonitorManager>().RegisterMonitor(this);

        Build();
    }

    private void Build()
    {
        if (_section is null) return;

        BuildFilter();

        _configureChangeMonitor = ChangeToken.OnChange(_section.GetReloadToken, BuildFilter);
    }

    private void BuildFilter()
    {
        lock (_locker)
        {
            _filter = null;

            if (!_section.Exists()) return;

            var configuration = _section.Get<MinimumLevelOverridableSerilogFilterConfiguration>();
            if (configuration is null) return;

            var defaultSuccess = Enum.TryParse<LogEventLevel>(configuration.Default, out var defaultLevel);
            if (!defaultSuccess) defaultLevel = LogEventLevel.Verbose;

            var builder = new MinimumLevelOverridableSerilogFilterBuilder();
            builder.SetDefaultMinimumLevel(defaultLevel);

            foreach (var item in configuration.Override ?? [])
            {
                if (string.IsNullOrWhiteSpace(item.Key)) continue;

                var success = Enum.TryParse<LogEventLevel>(item.Value, out var level);
                if (!success) continue;

                builder.Add(item.Key.Trim(), level);
            }

            _filter = builder.Build();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _configureChangeMonitor?.Dispose();
        GC.SuppressFinalize(this);
    }

    private class MinimumLevelOverridableSerilogFilterConfiguration
    {
        public string? Default { get; set; }

        public Dictionary<string, string> Override { get; set; } = [];
    }
}