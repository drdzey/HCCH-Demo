using System;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Lili.Protocol.General;
using Microsoft.Extensions.Logging;

namespace Lili.Protocol.Core;

public sealed class RemoteHotCallNavigationGate : IHotCallNavigationGate
{
    public RemoteHotCallNavigationGate(
        IHubFactory factory,
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfo,
        HotCallNavigationGate gate,
        ISharedLoggerFactory loggerFactory)
    {
        _factory = factory;
        _userIdProvider = userIdProvider;
        _appInfo = appInfo;
        _gate = gate;
        _logger = loggerFactory.GetLogger(GetType());
    }

    // for testing purposes
    internal RemoteHotCallNavigationGate(
        IHubFactory factory,
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfo,
        HotCallNavigationGate gate,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess)
        : this(factory, userIdProvider, appInfo, gate, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly IHubFactory _factory;
    private readonly IProvideUserId _userIdProvider;
    private readonly IAppInfoProvider _appInfo;
    private readonly HotCallNavigationGate _gate;
    private readonly ISharedLogger _logger;

    public event AsyncEventHandler<(Guid? UserId, string Owner)> GateIdle
    {
        add => _gate.GateIdle += value;
        remove => _gate.GateIdle -= value;
    }

    public void Go(HotCallSimpleKey key, Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;

        _gate.Go(key, userId, owner);
    }

    public void GoBack(Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;

        _gate.GoBack(userId, owner);
    }

    public async Task WaitForIdleAsync(HotCallSingleChain chain = null, Guid? userId = null, string owner = null, bool? localOnly = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;

        chain ??= _gate.GetChain(userId, owner);
        var nof = $"{nameof(WaitForIdleAsync)}({chain}, {_GetShort(userId)}, {owner})";

        localOnly ??= true;
        if (localOnly == false)
        {
            _logger.Log(nof, $"RemoteGate must cooperate with local NavigationStack", LogLevel.Warning);
        }

        try
        {
            var comm = await _factory.GetCommunication(userId.Value);
            await comm.GoViaSectionAsync(owner, chain);
            await _gate.WaitForIdleAsync(chain, userId, owner, localOnly: !localOnly.Value, cancellationToken: cancellationToken);
            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }
        catch (Exception ex)
        {
            _logger.Log(nof, "FAILED.", LogLevel.Error, ex);
            throw;
        }
    }

    private static string _GetShort(Guid? userId) => userId?.ToString().Substring(0, 4) ?? "NULL";
}