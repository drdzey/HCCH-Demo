using System;

namespace Lili.Protocol.Tests.UnitTests;

internal static class ComponentNameExtensions
{
    public static string To(this ComponentName input)
    {
        return input switch
        {
            ComponentName.LCore => "Lili.Core",
            ComponentName.LDom => "Lili.Dom",
            ComponentName.LShell => "Lili.Shell",
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, null)
        };
    }
}