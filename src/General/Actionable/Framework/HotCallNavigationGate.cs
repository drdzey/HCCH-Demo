using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToPrimaryConstructor

namespace Lili.Protocol.General;

public sealed class HotCallNavigationGate : IHotCallNavigationGate
{
    public HotCallNavigationGate(
        IHotCallNavigationService navigationService,
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfo,
        ISharedLoggerFactory loggerFactory)
    {
        _navigationService = navigationService;
        _userIdProvider = userIdProvider;
        _appInfo = appInfo;
        _logger = loggerFactory.GetLogger(GetType());
    }

    // for testing purposes
    internal HotCallNavigationGate(
        IHotCallNavigationService navigationService,
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfo,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess)
        : this(navigationService, userIdProvider, appInfo, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly IHotCallNavigationService _navigationService;
    private readonly IProvideUserId _userIdProvider;
    private readonly IAppInfoProvider _appInfo;
    private readonly ISharedLogger _logger;
    private readonly ConcurrentDictionary<(Guid UserId, string Owner), ConcurrentQueue<HotCallSimpleKey>> _queues = new();

    public HotCallSingleChain GetChain(Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var compoundKey = (userId.Value, owner);
        var queue = _queues.GetOrAdd(compoundKey, new ConcurrentQueue<HotCallSimpleKey>());
        return queue.ToArray();
    }

    public event AsyncEventHandler<(Guid? UserId, string Owner)> GateIdle;

    public void Go(HotCallSimpleKey key, Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var compoundKey = (userId.Value, owner);
        var queue = _queues.GetOrAdd(compoundKey, new ConcurrentQueue<HotCallSimpleKey>());
        queue.Enqueue(key);
    }

    public void GoBack(Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var compoundKey = (userId.Value, owner);
        var queue = _queues.GetOrAdd(compoundKey, new ConcurrentQueue<HotCallSimpleKey>());
        queue.Enqueue(HotCallSimpleKey.Back);
    }

    public async Task WaitForIdleAsync(HotCallSingleChain chain = null, Guid? userId = null, string owner = null, bool? localOnly = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;

        if (chain == null)
        {
            chain = GetChain(userId, owner);
        }
        else
        {
            var compoundKey = (userId.Value, owner);
            _queues.TryRemove(compoundKey, out _);
        }

        var nof = $"{nameof(WaitForIdleAsync)}({chain}, {_GetShort(userId)}, {owner})";

        localOnly ??= false;
        if (localOnly == true)
        {
            _logger.Log(nof, $"LocalGate must cooperate with remote NavigationStack", LogLevel.Warning);
        }

        foreach (var key in chain.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (key.Equals(HotCallSimpleKey.Back))
            {
                await _navigationService.GoBackAsync(userId, owner, localOnly.Value, cancellationToken);
            }
            else
            {
                await _navigationService.GoAsync(key, userId, owner, localOnly.Value, cancellationToken);
            }
        }

        if (GateIdle != null)
        {
            await GateIdle.Invoke(this, (userId, owner), cancellationToken);
        }

        _logger.Log(nof, "DONE.", LogLevel.Trace);
    }

    private static string _GetShort(Guid? userId) => userId?.ToString().Substring(0, 4) ?? "NULL";
}