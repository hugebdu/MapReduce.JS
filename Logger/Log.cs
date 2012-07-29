using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logger
{
    public class Log
    {
        private enum LogLevel
        {
            Error = 0,
            Warning = 1,
            Info = 2,
            Debug = 3
        }

        public static Log Instance {get; private set;}
        private LogLevel _level;

        static Log()
        {
            Instance = new Log();
        }

        private Log()
	    {
            _level = LogLevel.Info;
        }

        public void Info(string message)
        {
            WriteLog(message, LogLevel.Info);
        }
        
        public void Warning(string message)
        {
            WriteLog(message, LogLevel.Warning);
        }
        
        public void Error(string message)
        {
            WriteLog(message, LogLevel.Error);
        }

        public void Debug(string message)
        {
            WriteLog(message, LogLevel.Debug);
        }

        private void WriteLog(string message, LogLevel level)
        {
            if (level > _level)
                return;

            //TODO: Write log
        }
    }
}
