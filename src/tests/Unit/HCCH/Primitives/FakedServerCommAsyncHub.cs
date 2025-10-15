using System;
using System.Threading;
using System.Threading.Tasks;
using Lili.Protocol.Core;
using Lili.Protocol.General;
using Lili.Protocol.Intercom;

namespace Lili.Protocol.Tests.UnitTests;

internal sealed class FakedServerCommAsyncHub : ICommAsyncHub
{
    public FakedServerCommAsyncHub(Guid userId, ComponentBox source)
    {
        UserId = userId;

        _source = source;
    }

    private readonly ComponentBox _source;

    private Guid UserId { get; }

    private IHotCallRegistry StaticRegistry => _source.SttRegistry;

    private IHotCallDynRegistry DynamicRegistry => _source.DynRegistry;

    private IHotCallNavigationGate NavigationGate => _source.NavigationGate;

    public Task<bool> IsAliveAsync()
    {
        return Task.FromResult(true);
    }

    public Task<CommunicationBlockResponseDto> SendPromptAsync(CommunicationBlockRequestDto request)
    {
        throw new NotSupportedException();
    }

    public async Task RegisterHotCallAsync(HotCallInfo hci)
    {
        await StaticRegistry.RegisterAsync(hci.AsHandler(), CancellationToken.None);
    }

    public async Task UnregisterHotCallAsync(HotCallInfo hci)
    {
        await StaticRegistry.UnregisterAsync(hci.AsHandler(), CancellationToken.None);
    }

    public async Task RegisterHotCallDynStartAsync(string owner)
    {
        await DynamicRegistry.RegisterStartAsync(UserId, owner, CancellationToken.None);
    }

    public async Task RegisterHotCallDynAsync(string owner, HotCallComplexKey hcck)
    {
        await DynamicRegistry.RegisterAsync(hcck, UserId, owner);
    }

    public async Task RegisterHotCallDynEndAsync(string owner)
    {
        await DynamicRegistry.RegisterEndAsync(UserId, owner, CancellationToken.None);
    }

    public async Task EnterHotCallDynSectionAsync(string owner, HotCallSimpleKey hcsk)
    {
        await DynamicRegistry.EnterSectionAsync(hcsk, UserId, owner, CancellationToken.None);
    }

    public async Task DropHotCallDynSectionAsync(string owner, HotCallSimpleKey hcsk)
    {
        await DynamicRegistry.DropSectionAsync(hcsk, UserId, owner, CancellationToken.None);
    }

    public async Task GoViaSectionAsync(string owner, HotCallSingleChain chain)
    {
        await NavigationGate.WaitForIdleAsync(chain, UserId, owner, cancellationToken: CancellationToken.None);
    }
}