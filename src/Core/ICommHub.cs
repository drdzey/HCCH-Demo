using System.Threading.Tasks;
using Lili.Protocol.General;
using Lili.Protocol.Intercom;
// ReSharper disable UnusedParameter.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Lili.Protocol.Core;

public interface ICommHub : IHub
{
    bool IsAlive();

    CommunicationBlockResponseDto SendPrompt(CommunicationBlockRequestDto request);

    // static
    void RegisterHotCall(HotCallInfo hci);

    void UnregisterHotCall(HotCallInfo hci);

    // dynamic
    void RegisterHotCallDynStart(string owner);

    void RegisterHotCallDyn(string owner, HotCallComplexKey hcck);

    void RegisterHotCallDynEnd(string owner);

    void EnterHotCallDynSection(string owner, HotCallSimpleKey hcsk);

    void DropHotCallDynSection(string owner, HotCallSimpleKey hcsk);

    // navigation
    void GoViaSection(string owner, HotCallSingleChain chain);
}

public interface ICommAsyncHub : IHub
{
    Task<bool> IsAliveAsync();

    Task<CommunicationBlockResponseDto> SendPromptAsync(CommunicationBlockRequestDto request);

    // static
    Task RegisterHotCallAsync(HotCallInfo hci);

    Task UnregisterHotCallAsync(HotCallInfo hci);

    // dynamic
    Task RegisterHotCallDynStartAsync(string owner);

    Task RegisterHotCallDynAsync(string owner, HotCallComplexKey hcck);

    Task RegisterHotCallDynEndAsync(string owner);

    Task EnterHotCallDynSectionAsync(string owner, HotCallSimpleKey hcsk);

    Task DropHotCallDynSectionAsync(string owner, HotCallSimpleKey hcsk);

    // navigation
    Task GoViaSectionAsync(string owner, HotCallSingleChain chain);
}