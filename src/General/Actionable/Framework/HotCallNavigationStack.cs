using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToPrimaryConstructor

namespace Lili.Protocol.General;

public sealed class HotCallNavigationStack : IHotCallNavigationStack
{
    public HotCallNavigationStack(
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfo,
        ISharedLoggerFactory loggerFactory)
    {
        _userIdProvider = userIdProvider;
        _appInfo = appInfo;
        _logger = loggerFactory.GetLogger(GetType());
    }

    // testing purposes
    internal HotCallNavigationStack(
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfo,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess) : this(userIdProvider, appInfo, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly object _sync = new();
    private readonly ConcurrentDictionary<(Guid UserId, string Owner), Stack<HotCallSimpleKey>> _sectionStacks = new();

    private readonly IProvideUserId _userIdProvider;
    private readonly IAppInfoProvider _appInfo;
    private readonly ISharedLogger _logger;

    public event AsyncEventHandler<(HotCallSimpleKey Key, Guid? UserId, string Owner, bool LocalOnly)> RequiredEnter;

    public event AsyncEventHandler<(HotCallSimpleKey Key, Guid? UserId, string Owner, bool LocalOnly)> RequiredDrop;

    public HotCallSimpleKey GetCurrentSection(Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var compoundKey = (userId.Value, owner);
        var stack = _sectionStacks.GetOrAdd(compoundKey, new Stack<HotCallSimpleKey>());

        lock (_sync)
        {
            return stack.Count > 0 ? stack.Peek() : null;
        }
    }

    public HotCallSimpleKey[] GetSections(Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var compoundKey = (userId.Value, owner);
        var stack = _sectionStacks.GetOrAdd(compoundKey, new Stack<HotCallSimpleKey>());

        lock (_sync)
        {
            return stack.ToArray();
        }
    }

    public HotCallSingleChain GetCurrentChain(Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;

        var chain = (HotCallSingleChain)GetSections(userId, owner);
        return chain;
    }

    public string GetSectionPath(Guid? userId = null, string owner = null)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;

        return GetCurrentChain(userId, owner).ToString();
    }

    public async Task GoAsync(HotCallSimpleKey key, Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var compoundKey = (userId.Value, owner);
        var nof = $"{nameof(GoAsync)}({key}, {_GetShort(userId)}, {owner})";

        var timestamp = new Stopwatch();
        timestamp.Start();

        if (GetCurrentSection(userId, owner)?.Equals(key) == true)
        {
            return;
        }

        if (GetCurrentSection(userId, owner) == null && key.Key.Equals(HotCallSimpleKey.RootKey))
        {
            // already at root
            return;
        }

        var stack = _sectionStacks.GetOrAdd(compoundKey, new Stack<HotCallSimpleKey>());

        HotCallSimpleKey prev;
        lock (_sync)
        {
            if (stack.Count > 0)
            {
                prev = stack.Pop();
            }
            else
            {
                prev = HotCallSimpleKey.FromKey();
            }
        }

        await _OnRequiredDropAsync(nof, prev, userId, owner, localOnly, cancellationToken);

        lock (_sync)
        {
            stack.Push(key);
        }

        await _OnRequiredEnterAsync(nof, key, userId, owner, localOnly, cancellationToken);

        timestamp.Stop();
        _logger.Log(nof, $"DONE in {timestamp.ElapsedMilliseconds} ms.", LogLevel.Trace);
    }

    public async Task GoBackAsync(Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var compoundKey = (userId.Value, owner);
        var nof = $"{nameof(GoBackAsync)}({_GetShort(userId)}, {owner})";

        var timestamp = new Stopwatch();
        timestamp.Start();

        if (GetSections(userId, owner).Length == 0)
        {
            return;
        }

        var stack = _sectionStacks.GetOrAdd(compoundKey, new Stack<HotCallSimpleKey>());

        HotCallSimpleKey dropped;
        lock (_sync)
        {
            dropped = stack.Pop();
        }

        await _OnRequiredDropAsync(nof, dropped, userId, owner, localOnly, cancellationToken);

        HotCallSimpleKey next;
        lock (_sync)
        {
            next = stack.Count > 0 ? stack.Peek() : HotCallSimpleKey.FromKey(HotCallSimpleKey.RootKey);
        }

        await _OnRequiredEnterAsync(nof, next, userId, owner, localOnly, cancellationToken);

        timestamp.Stop();
        _logger.Log(nof, $"DONE in {timestamp.ElapsedMilliseconds} ms.", LogLevel.Trace);
    }

    public async Task InitAsync(Guid? userId, string owner = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var nof = $"{nameof(InitAsync)}({_GetShort(userId)}, {owner ?? "NULL"})";

        var key = HotCallSimpleKey.FromKey();
        await _OnRequiredEnterAsync(nof, key, userId: userId, owner: owner, localOnly: true, cancellationToken);
    }

    public async Task ClearAsync(Guid? userId, string owner = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfo.ApplicationName;
        var nof = $"{nameof(ClearAsync)}({_GetShort(userId)}, {owner ?? "NULL"})";

        var compoundKey = (userId.Value, owner);

        while (GetSections(userId, owner).Length > 0)
        {
            var stack = _sectionStacks.GetOrAdd(compoundKey, new Stack<HotCallSimpleKey>());

            HotCallSimpleKey key;
            lock (_sync)
            {
                key = stack.Pop();
            }

            await _OnRequiredDropAsync(nof, key, userId: userId, owner: owner, localOnly: true, cancellationToken);
        }

        // drop default key
        await _OnRequiredDropAsync(nof, HotCallSimpleKey.FromKey(), userId: userId, owner: owner, localOnly: true, cancellationToken);
    }

    private async Task _OnRequiredEnterAsync(string nof, HotCallSimpleKey key, Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default)
    {
        if (RequiredEnter != null)
        {
            _logger.Log(nof, $"Required ENTER for ({key})", LogLevel.Trace);
            await RequiredEnter.Invoke(this, (key, userId, owner, localOnly), cancellationToken);
        }
    }

    private async Task _OnRequiredDropAsync(string nof, HotCallSimpleKey key, Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default)
    {
        if (RequiredDrop != null)
        {
            _logger.Log(nof, $"Required DROP for ({key})", LogLevel.Trace);
            await RequiredDrop.Invoke(this, (key, userId, owner, localOnly), cancellationToken);
        }
    }

    private static string _GetShort(Guid? userId) => userId?.ToString().Substring(0, 4) ?? "NULL";
}
