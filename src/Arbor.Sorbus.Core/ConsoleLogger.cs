using System;

namespace Arbor.Sorbus.Core
{
    public class ConsoleLogger : ILogger
    {
        public void WriteError(string message, string prefix = null)
        {
            if (LogLevel.Error.Level <= LogLevel.Level)
            {
                Console.Error.WriteLine(message);
            }
        }

        public void Write(string message, string prefix = null)
        {
            if (LogLevel.Information.Level <= LogLevel.Level)
            {
                Console.WriteLine(message);
            }
        }

        public void WriteWarning(string message, string prefix = null)
        {
            if (LogLevel.Warning.Level <= LogLevel.Level)
            {
                Console.WriteLine(message);
            }
        }

        public void WriteVerbose(string message, string prefix = null)
        {
            if (LogLevel.Verbose.Level <= LogLevel.Level)
            {
                Console.WriteLine(message);
            }
        }

        public LogLevel LogLevel { get; set; }

        public void WriteDebug(string message, string prefix = null)
        {
            if (LogLevel.Debug.Level <= LogLevel.Level)
            {
                Console.WriteLine(message);
            }
        }
    }
}