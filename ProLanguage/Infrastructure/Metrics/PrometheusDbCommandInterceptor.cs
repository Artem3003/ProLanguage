using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Prometheus;

namespace Infrastructure.Metrics;

public class PrometheusDbCommandInterceptor : DbCommandInterceptor
{
    private static readonly Histogram QueryDuration = Prometheus.Metrics.CreateHistogram(
        "db_query_duration_seconds",
        "Duration of database queries",
        new HistogramConfiguration
        {
            LabelNames = ["operation_type"],
        });

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        QueryDuration.WithLabels(GetOperationType(command.CommandText)).Observe(eventData.Duration.TotalSeconds);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default)
    {
        QueryDuration.WithLabels(GetOperationType(command.CommandText)).Observe(eventData.Duration.TotalSeconds);
        return new ValueTask<DbDataReader>(result);
    }

    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result)
    {
        QueryDuration.WithLabels(GetOperationType(command.CommandText)).Observe(eventData.Duration.TotalSeconds);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        QueryDuration.WithLabels(GetOperationType(command.CommandText)).Observe(eventData.Duration.TotalSeconds);
        return new ValueTask<int>(result);
    }

    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        QueryDuration.WithLabels(GetOperationType(command.CommandText)).Observe(eventData.Duration.TotalSeconds);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default)
    {
        QueryDuration.WithLabels(GetOperationType(command.CommandText)).Observe(eventData.Duration.TotalSeconds);
        return new ValueTask<object?>(result);
    }

    private static string GetOperationType(string commandText)
    {
        if (string.IsNullOrWhiteSpace(commandText))
        {
            return "read";
        }

        var firstWord = commandText.TrimStart().Split(' ', 2)[0].ToUpperInvariant();
        return firstWord switch
        {
            "INSERT" => "write",
            "UPDATE" => "write",
            "DELETE" => "write",
            _ => "read",
        };
    }
}
