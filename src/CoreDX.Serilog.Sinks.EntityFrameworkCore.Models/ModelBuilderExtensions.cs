using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreDX.Serilog.Sinks.EntityFrameworkCore.Models;

/// <summary>
/// Extensions for configure entity model.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Configure entity model using custom type.
    /// </summary>
    /// <typeparam name="TLogRecord">The type of <see cref="LogRecord"/>.</typeparam>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="configureEntity">An <see cref="Action{T}"/> using to configure model.</param>
    /// <returns>The <see cref="ModelBuilder"/> using to chaind call.</returns>
    public static ModelBuilder UseLogRecord<TLogRecord>(
        this ModelBuilder modelBuilder,
        Action<EntityTypeBuilder<TLogRecord>>? configureEntity = null)
        where TLogRecord : LogRecord, new()
    {
        var entityBuilder = modelBuilder.Entity<TLogRecord>();
        configureEntity?.Invoke(entityBuilder);

        return modelBuilder;
    }

    /// <summary>
    /// Configure entity model using type of <see cref="LogRecord"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    /// <param name="configureEntity">An <see cref="Action{T}"/> using to configure model.</param>
    /// <returns>The <see cref="ModelBuilder"/> using to chaind call.</returns>
    public static ModelBuilder UseLogRecord(
        this ModelBuilder modelBuilder,
        Action<EntityTypeBuilder<LogRecord>>? configureEntity = null) =>
            modelBuilder.UseLogRecord<LogRecord>(configureEntity);
}