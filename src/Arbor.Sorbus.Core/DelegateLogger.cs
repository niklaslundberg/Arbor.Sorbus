using System;

namespace Arbor.Sorbus.Core
{
    internal class DelegateLogger : ILogger
    {
        readonly Action<string, string> _debug;
        readonly Action<string, string> _error;
        readonly Action<string, string> _info;
        readonly Action<string, string> _verbose;
        readonly Action<string, string> _warning;

        public DelegateLogger(Action<string, string> error = null,
            Action<string, string> warning = null,
            Action<string, string> info = null,
            Action<string, string> verbose = null,
            Action<string, string> debug = null)
        {
            _error = error ?? ((message, prefix) => { });
            _warning = warning ?? ((message, prefix) => { });
            _info = info ?? ((message, prefix) => { });
            _verbose = verbose ?? ((message, prefix) => { });
            _debug = debug ?? ((message, prefix) => { });
        }

        public void WriteError(string message, string prefix = null)
        {
            if (LogLevel.Error.Level <= LogLevel.Level)
            {
                _error(message, prefix);
            }
        }

        public void Write(string message, string prefix = null)
        {
            if (LogLevel.Information.Level <= LogLevel.Level)
            {
                _info(message, prefix);
            }
        }

        public void WriteWarning(string message, string prefix = null)
        {
            if (LogLevel.Warning.Level <= LogLevel.Level)
            {
                _warning(message, prefix);
            }
        }

        public void WriteVerbose(string message, string prefix = null)
        {
            if (LogLevel.Verbose.Level <= LogLevel.Level)
            {
                _verbose(message, prefix);
            }
        }

        public LogLevel LogLevel { get; set; }

        public void WriteDebug(string message, string prefix = null)
        {
            if (LogLevel.Debug.Level <= LogLevel.Level)
            {
                _debug(message, prefix);
            }
        }
    }
}