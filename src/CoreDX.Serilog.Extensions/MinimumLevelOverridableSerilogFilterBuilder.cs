using Serilog.Events;
using Serilog.Filters;

namespace CoreDX.Serilog.Extensions;

/// <summary>
/// 
/// </summary>
public class MinimumLevelOverridableSerilogFilterBuilder
{
    private readonly Dictionary<string, LogEventLevel> _filters = [];

    /// <summary>
    /// 
    /// </summary>
    public LogEventLevel? DefaultMinimumLevel { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="level"></param>
    /// <exception cref="ArgumentException"></exception>
    public void AddSourceLevel(string source, LogEventLevel level) => _filters.Add(!string.IsNullOrWhiteSpace(source) ? source : throw new ArgumentException("Content is empty.", nameof(source)), level);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="level"></param>
    /// <exception cref="NullReferenceException"></exception>
    public void AddSourceLevel<TSource>(LogEventLevel level) => _filters.Add(typeof(TSource).FullName ?? throw new NullReferenceException(typeof(TSource).Name), level);

    /// <summary>
    /// 
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
/// 
/// </summary>
public static class MinimumLevelOverridableSerilogFilterBuilderExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="source"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static MinimumLevelOverridableSerilogFilterBuilder Add(
        this MinimumLevelOverridableSerilogFilterBuilder builder,
        string source,
        LogEventLevel level)
    {
        builder.AddSourceLevel(source, level);
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <param name="builder"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static MinimumLevelOverridableSerilogFilterBuilder Add<TSource>(
        this MinimumLevelOverridableSerilogFilterBuilder builder,
        LogEventLevel level)
    {
        builder.AddSourceLevel<TSource>(level);
        return builder;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static MinimumLevelOverridableSerilogFilterBuilder SetDefaultMinimumLevel(
        this MinimumLevelOverridableSerilogFilterBuilder builder,
        LogEventLevel? level)
    {
        builder.DefaultMinimumLevel = level;
        return builder;
    }
}