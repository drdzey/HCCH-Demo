using System;
using System.Threading.Tasks;
using Lili.Protocol.Core;
using Lili.Protocol.General;
using Lili.Protocol.Intercom;
// ReSharper disable ConvertToPrimaryConstructor

namespace Lili.Protocol.Tests.UnitTests;

internal sealed class FakedClientCommAsyncHub : ICommAsyncHub
{
    public FakedClientCommAsyncHub(Guid userId)
    {
        UserId = userId;
    }

    // this must be set
    public ICommAsyncHub Server { get; set; }

    private Guid UserId { get; }

    public Task<bool> IsAliveAsync()
    {
        return Task.FromResult(true);
    }

    public Task<CommunicationBlockResponseDto> SendPromptAsync(CommunicationBlockRequestDto request)
    {
        throw new NotSupportedException();
    }

    public Task RegisterHotCallAsync(HotCallInfo hci)
    {
        return Server?.RegisterHotCallAsync(hci) ?? Task.CompletedTask;
    }

    public Task UnregisterHotCallAsync(HotCallInfo hci)
    {
        return Server?.UnregisterHotCallAsync(hci) ?? Task.CompletedTask;
    }

    public Task RegisterHotCallDynStartAsync(string owner)
    {
        return Server?.RegisterHotCallDynStartAsync(owner) ?? Task.CompletedTask;
    }

    public Task RegisterHotCallDynAsync(string owner, HotCallComplexKey hcck)
    {
        return Server?.RegisterHotCallDynAsync(owner, hcck) ?? Task.CompletedTask;
    }

    public Task RegisterHotCallDynEndAsync(string owner)
    {
        return Server?.RegisterHotCallDynEndAsync(owner) ?? Task.CompletedTask;
    }

    public Task EnterHotCallDynSectionAsync(string owner, HotCallSimpleKey hcsk)
    {
        return Server?.EnterHotCallDynSectionAsync(owner, hcsk) ?? Task.CompletedTask;
    }

    public Task DropHotCallDynSectionAsync(string owner, HotCallSimpleKey hcsk)
    {
        return Server?.DropHotCallDynSectionAsync(owner, hcsk) ?? Task.CompletedTask;
    }

    public Task GoViaSectionAsync(string owner, HotCallSingleChain chain)
    {
        return Server?.GoViaSectionAsync(owner, chain) ?? Task.CompletedTask;
    }
}