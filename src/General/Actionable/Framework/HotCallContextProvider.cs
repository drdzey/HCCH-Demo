using System.Threading;

// ReSharper disable UnusedType.Global

namespace Lili.Protocol.General;

public sealed class HotCallContextProvider : IHotCallContextProvider
{
    private readonly AsyncLocal<HotCallContext> _ctx = new();

    public HotCallContext Current
    {
        get => _ctx.Value ?? new HotCallContext();
        set => _ctx.Value = value;
    }
}
