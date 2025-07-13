using System.Diagnostics;
using OpenTelemetry;

namespace CleanArch.Infrastructure.OpenTelemetry;

/// <summary>
/// OpenTelemetry processor that adds custom tags to Hangfire database operations for easier filtering.
/// </summary>
public class HangfireTaggerProcessor : BaseProcessor<Activity>
{
    public override void OnEnd(Activity data)
    {
        // Check if this is a database operation
        var dbStatement = data.GetTagItem("db.statement")?.ToString();

        if (!string.IsNullOrEmpty(dbStatement))
        {
            var statement = dbStatement.ToLowerInvariant();
            if (statement.Contains("hangfire"))
            {
                // Add custom tag instead of filtering
                data.SetTag("om.source", "hangfire");
                data.SetTag("om.category", "background-job");
            }
        }

        base.OnEnd(data);
    }
}
