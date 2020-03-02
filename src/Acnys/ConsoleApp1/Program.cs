using System;
using System.Threading;
using System.Threading.Tasks;
using Acnys.Core.Scheduler;
using Serilog;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Log.Logger = new LoggerConfiguration().WriteTo.Console().MinimumLevel.Verbose().CreateLogger();

            var stoppingTokenSource = new CancellationTokenSource();

            var scheduler = new SchedulerService(Log.Logger, null, new SchedulerOptions());
            var shcedulerTask = scheduler.Process(stoppingTokenSource.Token);

            var task1 = scheduler.Schedule(() => Console.WriteLine("1 sec"), TimeSpan.FromSeconds(1), stoppingTokenSource.Token);
            var task2 = scheduler.Schedule(() => Console.WriteLine("2 sec"), TimeSpan.FromSeconds(2), stoppingTokenSource.Token);
            var task3 = scheduler.Schedule(() => Console.WriteLine("3 sec"), TimeSpan.FromSeconds(3), stoppingTokenSource.Token);

            Console.WriteLine("Waiting for tasks to finish");

            Task.WaitAll(task1, task2, task3);

            stoppingTokenSource.Cancel();

            Task.Delay(500);
        }
    }
}
