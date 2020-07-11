using DominosLocationMap.Core.CrossCutting.Logging;
using NLog;

namespace DominosLocationMap.Core.CrossCutting.Logging.NLog
{
    public class NLogManager : ILogManager
    {
        private readonly ILogger _logger;

        public NLogManager()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Information(string message)
        {
            _logger.Info(message);
        }

        public void Warning(string message)
        {
            _logger.Warn(message);
        }
    }
}