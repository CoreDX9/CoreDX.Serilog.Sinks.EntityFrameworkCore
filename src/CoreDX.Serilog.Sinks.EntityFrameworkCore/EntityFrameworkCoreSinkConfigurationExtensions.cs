using CoreDX.Serilog.Sinks.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
#if NET6_0_OR_GREATER
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif

namespace CoreDX.Serilog.Sinks.EntityFrameworkCore;

/// <summary>
/// Extensions for configure EntityFrameworkCore sink.
/// </summary>
public static class EntityFrameworkCoreSinkConfigurationExtensions
{
    /// <summary>
    /// Configure EntityFrameworkCore sink.
    /// </summary>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <param name="sinkConfiguration">The configuration.</param>
    /// <param name="serviceScopeFactory">The application service scope factory.</param>
    /// <param name="contextFactory">A <see cref="Func{T, TResult}"/> using to get <see cref="DbContext"/>.</param>
    /// <param name="serializerOptions">The JSON serializer options.</param>
    /// <param name="configureOptions">An <see cref="Action{T}"/> using to configure <see cref="BatchingOptions"/>.</param>
    /// <param name="formatProvider">The string format provider.</param>
    /// <param name="restrictedToMinimumLevel">The restricted to minimum level.</param>
    /// <param name="levelSwitch">The logging level switch.</param>
    /// <returns>The <see cref="LoggerConfiguration"/> using to chaind call.</returns>
    public static LoggerConfiguration EntityFrameworkCore<TDbContext>(
        this LoggerSinkConfiguration sinkConfiguration,
        IServiceScopeFactory serviceScopeFactory,
        Func<IServiceProvider, TDbContext>? contextFactory,
#if NET6_0_OR_GREATER
        JsonSerializerOptions? serializerOptions = null,
#else
        JsonSerializerSettings? serializerOptions = null,
#endif
        Action<BatchingOptions>? configureOptions = null,
        IFormatProvider? formatProvider = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        LoggingLevelSwitch? levelSwitch = null)
        where TDbContext : DbContext
    {
        return EntityFrameworkCore<TDbContext, LogRecord>(
            sinkConfiguration,
            serviceScopeFactory,
            contextFactory,
            serializerOptions,
            configureOptions,
            formatProvider,
            restrictedToMinimumLevel,
            levelSwitch);
    }

    /// <summary>
    /// Configure EntityFrameworkCore sink.
    /// </summary>
    /// <typeparam name="TDbContext">The type of <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TLogRecord">The type of <see cref="LogRecord"/>.</typeparam>
    /// <param name="sinkConfiguration">The configuration.</param>
    /// <param name="serviceScopeFactory">The application service scope factory.</param>
    /// <param name="contextFactory">A <see cref="Func{T, TResult}"/> using to get <see cref="DbContext"/>.</param>
    /// <param name="serializerOptions">The JSON serializer options.</param>
    /// <param name="configureOptions">An <see cref="Action{T}"/> using to configure <see cref="BatchingOptions"/>.</param>
    /// <param name="formatProvider">The string format provider.</param>
    /// <param name="restrictedToMinimumLevel">The restricted to minimum level.</param>
    /// <param name="levelSwitch">The logging level switch.</param>
    /// <returns>The <see cref="LoggerConfiguration"/> using to chaind call.</returns>
    public static LoggerConfiguration EntityFrameworkCore<TDbContext, TLogRecord>(
        this LoggerSinkConfiguration sinkConfiguration,
        IServiceScopeFactory serviceScopeFactory,
        Func<IServiceProvider, TDbContext>? contextFactory,
#if NET6_0_OR_GREATER
        JsonSerializerOptions? serializerOptions = null,
#else
        JsonSerializerSettings? serializerOptions = null,
#endif
        Action<BatchingOptions>? configureOptions = null,
        IFormatProvider? formatProvider = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
        LoggingLevelSwitch? levelSwitch = null)
        where TDbContext : DbContext
        where TLogRecord : LogRecord, new()
    {
        var efCoreSink = new EntityFrameworkCoreSink<TDbContext, TLogRecord>(serviceScopeFactory, contextFactory, serializerOptions, formatProvider);
        var batchingOptions = new BatchingOptions();
        configureOptions?.Invoke(batchingOptions);

        return sinkConfiguration.Sink(efCoreSink, batchingOptions, restrictedToMinimumLevel, levelSwitch);
    }
}
