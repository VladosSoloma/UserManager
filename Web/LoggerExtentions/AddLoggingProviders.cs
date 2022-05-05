using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

namespace Web.LoggerExtentions
{
    public static class LoggerExtentions
    {
        public static LoggerConfiguration AddLoggingProviders(this LoggerConfiguration logger, IConfiguration config, ref TelemetryClient? client)
        {
            logger.ReadFrom.Configuration(config);
            var appInsightsKey = config.GetSection("ApplicationInsights:Key").Value;
            if (string.IsNullOrEmpty(appInsightsKey))
                return logger;

            client = new TelemetryClient(new TelemetryConfiguration(appInsightsKey));
            return logger.WriteTo.ApplicationInsights(client, TelemetryConverter.Events);
        }
    }
}
