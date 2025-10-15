using System;

namespace Lili.Protocol.Core;

public static class TokenSourceExtensions
{
    public static void SafeCancel(this System.Threading.CancellationTokenSource cts)
    {
        try
        {
            cts?.Cancel();
        }
        catch
        {
            // ignore
        }
    }

    public static void SafeDispose(this IDisposable disposal)
    {
        try
        {
            disposal?.Dispose();
        }
        catch
        {
            // ignore
        }
    }
}