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
            //mrgen.test();
            //return;
            
            if (args.Length > 0)
            {
                var mrgen = new MapResultGenerator();
                if (args[0] == "mr")
                {
                    mrgen.Generate(args[1], args[2], args[3]);
                }
                else if (args[0] == "pop")
                {
                    mrgen.Pop();
                }
                return;
            }

            var workerRole = new WorkerRole();
            workerRole.OnStart();
            workerRole.Run();

            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
