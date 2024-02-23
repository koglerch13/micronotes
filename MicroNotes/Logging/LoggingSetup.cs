using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.Targets;

namespace MicroNotes.Logging;

public static class LoggingSetup
{
    public static void SetupLogger()
    {
        var config = new LoggingConfiguration();
        var logfile = new FileTarget("./log.txt");
        config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);
        
        LogManager.Configuration = config;
    }

    public static Microsoft.Extensions.Logging.ILogger GetLogger()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddNLog());
        return loggerFactory.CreateLogger("MicroNotes");
    }
}