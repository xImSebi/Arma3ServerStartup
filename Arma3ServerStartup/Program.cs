using Serilog;
using System.Diagnostics;

namespace Arma3ServerStartup
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            while (true)
            {
                while (Process.GetProcessesByName("arma3server_x64").Length > 0 || Process.GetProcessesByName("arma3server").Length > 0)
                {
                    Log.Information("Server is running. Checking again in 5000ms");
                    Thread.Sleep(5000);
                }

                Log.Warning("Server not running! Starting Server in 15000ms");
                Thread.Sleep(15000);

                var task = Task.Run(() => new ServerStartup().Start());
                while (!task.IsCompletedSuccessfully)
                { Thread.Sleep(100); }

                Thread.Sleep(5000);
            }

            Log.CloseAndFlush();
        }
    }
}