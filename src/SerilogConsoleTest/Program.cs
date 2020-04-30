//using Anotar.Serilog;
using Anotar.Serilog;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SerilogConsoleTest
{
    public class Person
    {
        public string Name { get; set; }
        public double Height { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Hello World!");
            SetupLoggerConfig();
            LogTo.Information("Hello World!");

            LogStructual();

            CallAllLogLevel();

            LogParallel();

            CauseOutOfMemoryEx();

            LogTo.Information("Finish App");
        }

        private static void LogStructual()
        {
            var person = new Person { Name = "Tiger", Height = 143.6 };
            LogTo.Information("Show {@tiger}", person);
        }

        private static void LogParallel() =>
            Parallel.For(0, 3, i =>
                LogTo.Information("Run {i}", i));

        private static void CauseOutOfMemoryEx()
        {
            var bigArray = new int[0];
            try
            {
                for (int bit = 0; bit < 64; bit++)
                {
                    bigArray = Enumerable.Range(0, (int)Math.Pow(2, bit)).ToArray();
                    LogTo.Debug("Created Array {Size}", bigArray.Length);
                }

            }
            catch (Exception ex)
            {
                LogTo.Error(ex, "Fail to Create big Array");
            }
        }

        private static void CallAllLogLevel()
        {
            LogTo.Verbose("Log Level 1");
            LogTo.Debug("Log Level 2");
            LogTo.Information("Log Level 3");
            LogTo.Warning("Log Level 4");
            LogTo.Error("Log Level 5");
            LogTo.Fatal("Log Level 6");
        }

        private static void SetupLoggerConfig()
        {
            string template = "| {Timestamp:HH:mm:ss.fff} | {Level:u4} | {ThreadId:00}:{ThreadName} | {ProcessId:00}:{ProcessName} | {Message:j} | {SourceContext} | {MethodName} | {LineNumber} L | {AssemblyName} | {AssemblyVersion} | {MachineName} | {EnvironmentUserName} | {MemoryUsage} B|{NewLine}{Exception}";

            string logFilePathHead = $"logs\\{nameof(SerilogConsoleTest)}";

            Log.Logger = new LoggerConfiguration()
                            .Enrich.WithThreadId()
                            .Enrich.WithThreadName().Enrich.WithProperty("ThreadName", "__")
                            .Enrich.WithProcessId().Enrich.WithProcessName()
                            .Enrich.WithMachineName()
                            .Enrich.WithEnvironmentUserName()
                            .Enrich.WithAssemblyName()
                            .Enrich.WithAssemblyVersion()
                            .Enrich.WithMemoryUsage()
                            .Enrich.WithExceptionDetails()
                            .MinimumLevel.Verbose()
                            .WriteTo.Console(outputTemplate: template)
                            .WriteTo.Debug(outputTemplate: template)
                            .WriteTo.File($"{logFilePathHead}.txt", LogEventLevel.Information, outputTemplate: template, rollingInterval: RollingInterval.Day)
                            .WriteTo.File(new CompactJsonFormatter(), $"{logFilePathHead}_comapct.json", LogEventLevel.Information, rollingInterval: RollingInterval.Day)
                            .CreateLogger();

            Thread.CurrentThread.Name = "MN";
        }
    }
}
