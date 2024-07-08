using CoreDX.Serilog.Sinks.EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using System.Text;
#if NET6_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Nodes;
#else
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

namespace CoreDX.Serilog.Sinks.EntityFrameworkCore;

/// <summary>
/// An <see cref="IBatchedLogEventSink"/> using <see cref="DbContext"/> to write log.
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
/// <typeparam name="TLogRecord"></typeparam>
/// <param name="scopeFactory"></param>
/// <param name="contextFactory"></param>
/// <param name="serializerOptions"></param>
/// <param name="formatProvider"></param>
public class EntityFrameworkCoreSink<TDbContext, TLogRecord>(
    IServiceScopeFactory scopeFactory,
    Func<IServiceProvider, TDbContext>? contextFactory,
#if NET6_0_OR_GREATER
    JsonSerializerOptions? serializerOptions,
#elif NET5_0
    JsonSerializerSettings? serializerOptions,
#elif NETSTANDARD2_0_OR_GREATER
    JsonSerializerSettings? serializerOptions,
#endif
    IFormatProvider? formatProvider = null) : IBatchedLogEventSink
    where TDbContext : DbContext
    where TLogRecord : LogRecord, new()
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly Func<IServiceProvider, TDbContext> _contextFactory = contextFactory ?? (static sp => sp.GetRequiredService<TDbContext>());
#if NET7_0_OR_GREATER
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions ?? JsonSerializerOptions.Default;
#elif NET6_0_OR_GREATER
    private readonly JsonSerializerOptions _serializerOptions = serializerOptions ?? new();
#elif NET5_0
    private readonly JsonSerializerSettings _serializerOptions = serializerOptions ?? new();
#elif NETSTANDARD2_0_OR_GREATER
    private readonly JsonSerializerSettings _serializerOptions = serializerOptions ?? new();
#endif
    private readonly JsonFormatter _jsonFormatter = new(formatProvider: formatProvider);

    /// <inheritdoc />
    public async Task EmitBatchAsync(IReadOnlyCollection<LogEvent> batch)
    {
#if NET6_0_OR_GREATER
        await using var scope = _scopeFactory.CreateAsyncScope();
#else
        using var scope = _scopeFactory.CreateScope();
#endif
        var dbContext = _contextFactory(scope.ServiceProvider);
        var set = dbContext.Set<TLogRecord>();

#if NET5_0_OR_GREATER
        await set.AddRangeAsync(batch.Select(ConvertLogEventToLogRecord));
#else
        set.AddRange(batch.Select(ConvertLogEventToLogRecord));
#endif

        await dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    private TLogRecord ConvertLogEventToLogRecord(LogEvent logEvent)
    {
        var logEventValue = ConvertLogEventToJson(logEvent);
        int? eventId = null;
        string? eventName = null;

        string? propertiesValue = null;
        try
        {
#if NET6_0_OR_GREATER
            var json = JsonNode.Parse(logEventValue);
#else
            var json = JObject.FromObject(logEvent);
#endif
            var properties = json?["Properties"];

#if NET6_0_OR_GREATER
            propertiesValue = properties?.ToJsonString(_serializerOptions);
#else
            propertiesValue = JsonConvert.SerializeObject(properties, _serializerOptions);
#endif

            var eventProperty = properties?["EventId"];
            eventId = (int?)(eventProperty?["Id"] ?? null);
            eventName = (string?)(eventProperty?["Name"] ?? null);
        }
#if DEBUG
#pragma warning disable CS0168 // 声明了变量，但从未使用过
        catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量，但从未使用过
#else
        catch
#endif
        {
            // ignore
        }

        return new TLogRecord
        {
            Exception = logEvent.Exception?.ToString(),
            Level = logEvent.Level.ToString(),
            LogEvent = logEventValue,
            EventId = eventId,
            EventName = eventName,
            Message = logEvent.RenderMessage(formatProvider),
            MessageTemplate = logEvent.MessageTemplate?.ToString(),
            TimeStamp = logEvent.Timestamp,
            SpanId = logEvent.SpanId?.ToHexString(),
            TraceId = logEvent.TraceId?.ToHexString(),
            Properties = propertiesValue
        };

        string ConvertLogEventToJson(LogEvent logEvent)
        {
            StringBuilder sb = new();
            using (StringWriter writer = new(sb))
            {
                _jsonFormatter.Format(logEvent, writer);
            }

            return sb.ToString();
        }
    }
}