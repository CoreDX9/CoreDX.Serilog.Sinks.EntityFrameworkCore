using Serilog.Events;
using Serilog.Filters;

namespace CoreDX.Serilog.Extensions;

/// <summary>
/// A minimum level switch for Serilog filter.
/// </summary>
public class MinimumLevelOverridableSerilogFilterBuilder
{
    private readonly Dictionary<string, LogEventLevel> _filters = [];

    /// <summary>
    /// Get or set default minimum level of log.
    /// </summary>
    public LogEventLevel? DefaultMinimumLevel { get; set; }

    /// <summary>
    /// Add a log event level switch predicate to builder.
    /// </summary>
    /// <param name="source">The log source.</param>
    /// <param name="level">The log level.</param>
    /// <exception cref="ArgumentException"></exception>
    public void AddSourceLevel(string source, LogEventLevel level) => _filters.Add(!string.IsNullOrWhiteSpace(source) ? source : throw new ArgumentException("Content is empty.", nameof(source)), level);

    /// <summary>
    /// Add a log event level switch predicate to builder using type name.
    /// </summary>
    /// <typeparam name="TSource">The log type of source.</typeparam>
    /// <param name="level">The log level.</param>
    /// <exception cref="NullReferenceException"></exception>
    public void AddSourceLevel<TSource>(LogEventLevel level) => _filters.Add(typeof(TSource).FullName ?? throw new NullReferenceException(typeof(TSource).Name), level);

    /// <summary>
    /// Build a new filter Function.
    /// </summary>
    /// <returns></returns>
    public Func<LogEvent, bool> Build() =>
        (log) =>
        {
            var match = false;
            var keyLength = 0;
            foreach (var filter in _filters)
            {
                if (Matching.FromSource(filter.Key)(log))
                {
                    if (filter.Key.Length > keyLength)
                    {
                        keyLength = filter.Key.Length;
                        match = log.Level >= filter.Value;
                    }
                }
            }

            return keyLength is 0 && DefaultMinimumLevel is not null
                ? log.Level >= DefaultMinimumLevel
                : match;
        };
}

/// <summary>
/// Extensions for Configure <see cref="MinimumLevelOverridableSerilogFilterBuilder"/>.
/// </summary>
public static class MinimumLevelOverridableSerilogFilterBuilderExtensions
{
    /// <summary>
    /// Add a log event level switch predicate to builder.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="source">The log source.</param>
    /// <param name="level">The log level.</param>
    /// <returns>The <see cref="MinimumLevelOverridableSerilogFilterBuilder"/> so that additional calls can be chained.</returns>
    public static MinimumLevelOverridableSerilogFilterBuilder Add(
        this MinimumLevelOverridableSerilogFilterBuilder builder,
        string source,
        LogEventLevel level)
    {
        builder.AddSourceLevel(source, level);
        return builder;
    }

    /// <summary>
    /// Add a log event level switch predicate to builder using type name.
    /// </summary>
    /// <typeparam name="TSource">The log type of source.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="level">The log level.</param>
    /// <returns>The <see cref="MinimumLevelOverridableSerilogFilterBuilder"/> so that additional calls can be chained.</returns>
    public static MinimumLevelOverridableSerilogFilterBuilder Add<TSource>(
        this MinimumLevelOverridableSerilogFilterBuilder builder,
        LogEventLevel level)
    {
        builder.AddSourceLevel<TSource>(level);
        return builder;
    }

    /// <summary>
    /// Set default minimum level of log.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="level">The log level.</param>
    /// <returns>The <see cref="MinimumLevelOverridableSerilogFilterBuilder"/> so that additional calls can be chained.</returns>
    public static MinimumLevelOverridableSerilogFilterBuilder SetDefaultMinimumLevel(
        this MinimumLevelOverridableSerilogFilterBuilder builder,
        LogEventLevel? level)
    {
        builder.DefaultMinimumLevel = level;
        return builder;
    }
}