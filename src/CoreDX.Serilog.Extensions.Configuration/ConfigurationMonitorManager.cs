using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreDX.Serilog.Extensions.Configuration;

/// <summary>
/// A manager to support dispose <see cref="MinimumLevelOverridableSerilogFilterConfigurationMonitor"/> automatically when application is shutting down.
/// </summary>
public class MinimumLevelOverridableSerilogFilterConfigurationMonitorManager : IDisposable
{
    private readonly List<MinimumLevelOverridableSerilogFilterConfigurationMonitor> _monitors = [];

    /// <summary>
    /// Register a <see cref="MinimumLevelOverridableSerilogFilterConfigurationMonitor"/> to <see cref="MinimumLevelOverridableSerilogFilterConfigurationMonitorManager"/>.
    /// </summary>
    /// <param name="monitor">The monitor.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void RegisterMonitor(MinimumLevelOverridableSerilogFilterConfigurationMonitor monitor)
    {
        if (monitor is null)
        {
            throw new ArgumentNullException(nameof(monitor));
        }

        _monitors.Add(monitor);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var monitor in _monitors)
        {
            monitor.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Extensions for configure services.
/// </summary>
public static class MinimumLevelOverridableSerilogFilterConfigurationMonitorManagerExtensions
{
    /// <summary>
    /// Try add <see cref="MinimumLevelOverridableSerilogFilterConfigurationMonitorManager"/> to <see cref="IServiceCollection"/> as singleton service.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddMinimumLevelOverridableSerilogFilterConfigurationMonitorManager(this IServiceCollection services)
    {
        services.TryAddSingleton<MinimumLevelOverridableSerilogFilterConfigurationMonitorManager>();
        return services;
    }
}