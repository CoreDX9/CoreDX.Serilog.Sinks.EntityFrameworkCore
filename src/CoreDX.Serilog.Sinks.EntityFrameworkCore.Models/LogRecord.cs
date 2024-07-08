using System.ComponentModel.DataAnnotations;

namespace CoreDX.Serilog.Sinks.EntityFrameworkCore.Models;

#pragma warning disable CS1591 // 缺少对公共可见类型或成员的 XML 注释
public class LogRecord

{
    public virtual long Id { get; set; }

    public virtual int? EventId { get; set; }

    public virtual string? EventName { get; set; }

    public virtual string? Message { get; set; }

    public virtual string? MessageTemplate { get; set; }

    [StringLength(128)]
    public virtual string? Level { get; set; }

    public virtual DateTimeOffset TimeStamp { get; set; }

    public virtual string? Exception { get; set; }

    public virtual string? LogEvent { get; set; }

    [StringLength(16)]
    public virtual string? SpanId { get; set; }

    [StringLength(32)]
    public virtual string? TraceId { get; set; }

    public virtual string? Properties { get; set; }
}
#pragma warning restore CS1591 // 缺少对公共可见类型或成员的 XML 注释
