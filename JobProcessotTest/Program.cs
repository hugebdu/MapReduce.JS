using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JobProcessor;

namespace JobProcessotTest
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                JobProcessor.Model.JobChunk chunk = new JobProcessor.Model.JobChunk()
                {
                    Data = new Uri("http://www.idc.ac.il"),
                    Handler = new Uri("http://www.idc.ac.il/handler"),
                    Mode = JobProcessor.Model.ProcessingMode.Map
                };

                chunk.ChunkUid.SplitId = Guid.NewGuid().ToString();
                chunk.ChunkUid.JobId = "J1";

                Console.WriteLine(chunk.ToJson());
                return;
                var workerRole = new WorkerRole();
                workerRole.OnStart();
                workerRole.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
