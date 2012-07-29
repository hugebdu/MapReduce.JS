using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using JobProcessor.Manage;
using JobProcessor.Implementation;

namespace JobProcessor
{
    public class WorkerRole : RoleEntryPoint
    {
        public WorkerRole()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder detailsBuilder = new StringBuilder();
            Exception ex = e.ExceptionObject as Exception;
            int i = 0;
            while (ex != null)
            {
                detailsBuilder.AppendLine(string.Format("({0}) Exception of type {1}. Message: {2}",
                    i++,
                    ex.GetType(),
                    ex.Message));
                detailsBuilder.AppendLine(string.Format("     Stack trace: {0}", ex.StackTrace));

                ex = ex.InnerException;
            }

            Logger.Log.Instance.Error(string.Format("WorkerRole. Caught unhandled exception: {0}", detailsBuilder.ToString()));
        }

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("$projectname$ entry point called", "Information");
            Logger.Log.Instance.Info("WorkerRole. Start worker role");
            var monitor = new JobsMonitor(new DefaultFactory());

            while (true)
            {
                Logger.Log.Instance.Info("WorkerRole. Check for new jobs");
                monitor.CheckForNewJobs();
                Thread.Sleep(10000);
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
