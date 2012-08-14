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
        
        private object _syncObject = new Object();
        private LogLevel _level;

        public static Log Instance {get; private set;}

        static Log()
        {
            // Temp log solution
            try
            {
                System.IO.File.Delete(@"c:\temp\mr.log");
            }
            catch { }
            
            Instance = new Log() { Active = true };
        }

        public bool Active { get; set; }

        private Log()
	    {
            _level = LogLevel.Debug;
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

        public void Exception(Exception e, string message = null)
        {
            StringBuilder detailsBuilder = new StringBuilder();
            detailsBuilder.AppendLine(string.Format("Exception occured. {0}", message));
            Exception ex = e;
            int i = 0;
            while (ex != null)
            {
                detailsBuilder.AppendLine(string.Format("({0}) Exception of type {1}. Message: {2}",
                    i++,
                    ex.GetType(),
                    ex.Message));
                detailsBuilder.AppendLine("  === Stack trace ===");
                detailsBuilder.AppendLine(ex.StackTrace);
                detailsBuilder.AppendLine("  ===================");

                ex = ex.InnerException;
            }

            Error(detailsBuilder.ToString());
        }

        public void Debug(string message)
        {
            WriteLog(message, LogLevel.Debug);
        }

        private void WriteLog(string message, LogLevel level)
        {
            if (!Active)
                return;
            if (level > _level)
                return;

            System.Threading.Tasks.TaskFactory factory = new System.Threading.Tasks.TaskFactory();
            factory.StartNew(() =>
                {
                    lock (_syncObject)
                    {
                        // TEMP Log solution
                        using (var sw = new System.IO.StreamWriter(@"c:\temp\mr.log", true))
                        {
                            sw.WriteLine(string.Format("{0} [{1}]  {2}", DateTime.Now, level, message));
                            sw.Close();
                        }
                    }
                });
        }
    }
}
