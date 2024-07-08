using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreDX.Serilog.Extensions.Configuration;

public class MinimumLevelOverridableSerilogFilterConfigurationMonitorManager : IDisposable
{
    private readonly List<MinimumLevelOverridableSerilogFilterConfigurationMonitor> _monitors = [];

    public void RegisterMonitor(MinimumLevelOverridableSerilogFilterConfigurationMonitor monitor)
    {
        if (monitor is null)
        {
            throw new ArgumentNullException(nameof(monitor));
        }

        _monitors.Add(monitor);
    }

    public void Dispose()
    {
        foreach (var monitor in _monitors)
        {
            monitor.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

public static class MinimumLevelOverridableSerilogFilterConfigurationMonitorManagerExtensions
{
    public static IServiceCollection AddMinimumLevelOverridableSerilogFilterConfigurationMonitorManager(this IServiceCollection services)
    {
        services.TryAddSingleton<MinimumLevelOverridableSerilogFilterConfigurationMonitorManager>();
        return services;
    }
}