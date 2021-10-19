using Serilog;
using System;
using System.Threading.Tasks;

namespace Prebatching_Automation
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("execution_logs.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();

                QIF_Execution qif = new QIF_Execution(Log.Logger);
                var qifResult = await qif.Execute();

                if (qifResult)
                {
                    Cache_Execution cacheExecution = new Cache_Execution(Log.Logger);
                    var cacheExecutionresult = await cacheExecution.Execute();

                    if (cacheExecutionresult)
                    {
                        Log.Information("Execution completed successfully...!!");
                    }
                    else
                    {
                        Log.Error("Execution completed with some errors in Cache execution...!!");
                    }
                }
                else
                {
                    Log.Error("Execution completed with some errors in QIF execution...!!");
                }
            }
            catch (Exception exception)
            {
                Log.Error($"Exception occurred in execution {exception.GetType().ToString()} {exception.Message}\n{exception.StackTrace}");
            }

            //Console.WriteLine("Press Any Key To Exit...!!");
            //Console.ReadKey();
        }
    }
}
