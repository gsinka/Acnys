using System;
using Serilog;
using Xunit.Abstractions;

namespace Acnys.Core.Tests.Helpers
{
    public static class TestHelper
    {
        public static LoggerConfiguration ConfigureLogForTesting(this LoggerConfiguration config, ITestOutputHelper testOutputHelper)
        {
            config.WriteTo.TestOutput(
                testOutputHelper,
                outputTemplate:
                "[{Timestamp:HH:mm:ss+fff}{EventType:x8} {Level:u3}][{Application}] {Message:lj} [{SourceContext}]{NewLine}{Exception}")
            .MinimumLevel.Verbose();

            return config;
        }
    }
}
