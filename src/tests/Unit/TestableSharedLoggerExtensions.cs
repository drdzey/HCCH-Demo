using Calamara.Ng.Common.Pooler.Logging;
using Lili.Protocol.General;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Lili.Protocol.Tests.UnitTests;

public static class TestableSharedLoggerExtensions
{
    public static ISharedLoggerFactory AsLogger(this ITestOutputHelper output)
    {
        return new TestableSharedLoggerFactory(output);
    }
}

internal sealed class TestableSharedLoggerFactory : ISharedLoggerFactory
{
    public TestableSharedLoggerFactory(ITestOutputHelper output)
    {
        _output = output;
    }

    private readonly ITestOutputHelper _output;

    public ISharedLogger GetLogger(Type type)
    {
        return new TestableSharedLogger(_output, type.Name);
    }

    public ISharedLogger GetLogger(string loggerName)
    {
        return new TestableSharedLogger(_output, loggerName);
    }

    public LogLevel? MinConsoleLevel { get; set; }

    private sealed class TestableSharedLogger : ISharedLogger
    {
        public TestableSharedLogger(ITestOutputHelper output, string name)
        {
            _output = output;
            Name = name;
        }

        private readonly ITestOutputHelper _output;

        public string Name { get; }

        public void Log(string nof, string message, LogLevel level = LogLevel.Information, Exception ex = null)
        {
            var lvl = level switch
            {
                LogLevel.Trace => "TRC",
                LogLevel.Debug => "DBG",
                LogLevel.Information => "INF",
                LogLevel.Warning => "WRN",
                LogLevel.Error => "ERR",
                LogLevel.Critical => "FTL",
                _ => "UNK"
            };

            var msg = $"[{lvl}] {Name}.{nof}: {message}";
            _output.WriteLine(msg);
            Debug.WriteLine(msg);
            if (ex != null)
            {
                _output.WriteLine($"Exception: {ex}");
            }
        }

        public ILoggingAdapter LogAdapter { get; }
    }
}