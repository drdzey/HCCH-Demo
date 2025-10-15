using System;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.General;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ConvertToPrimaryConstructor

namespace Lili.Protocol.General;

public sealed class HotCallNavigationService : PublicDisposableUnit, IHotCallNavigationService
{
    public HotCallNavigationService(
        IServiceProvider serviceProvider,
        IHotCallNavigationStack stack,
        ISharedLoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _stack = stack;
        _logger = loggerFactory.GetLogger(GetType());

        _stack.RequiredEnter += Stack_RequiredEnter;
        _stack.RequiredDrop += Stack_RequiredDrop;
    }

    internal HotCallNavigationService(
        IServiceProvider serviceProvider,
        IHotCallNavigationStack stack,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess)
        : this(serviceProvider, stack, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly IHotCallNavigationStack _stack;
    private readonly ISharedLogger _logger;

    public async Task GoAsync(HotCallSimpleKey key, Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default)
    {
        var nof = $"{nameof(GoAsync)}({key}, {_GetShort(userId)}, {owner})";
        await _stack.GoAsync(key, userId, owner, localOnly, cancellationToken);
        _logger.Log(nof, $"DONE. Current Path ({_stack.GetSectionPath(userId, owner)})", LogLevel.Trace);
    }

    public async Task GoBackAsync(Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default)
    {
        var nof = $"{nameof(GoBackAsync)}({_GetShort(userId)}, {owner})";
        await _stack.GoBackAsync(userId, owner, localOnly, cancellationToken);
        _logger.Log(nof, $"DONE. Current Path ({_stack.GetSectionPath(userId, owner)})", LogLevel.Trace);
    }

    private async Task Stack_RequiredEnter(object sender, (HotCallSimpleKey Key, Guid? UserId, string Owner, bool LocalOnly) e, CancellationToken cancellationToken = default)
    {
        var registry = _GetRegistry(e.LocalOnly);
        if (registry != null)
        {
            await registry.EnterSectionAsync(e.Key, e.UserId, e.Owner, cancellationToken: cancellationToken);
        }
    }

    private async Task Stack_RequiredDrop(object sender, (HotCallSimpleKey Key, Guid? UserId, string Owner, bool LocalOnly) e, CancellationToken cancellationToken = default)
    {
        var registry = _GetRegistry(e.LocalOnly);
        if (registry != null)
        {
            await registry.DropSectionAsync(e.Key, e.UserId, e.Owner, cancellationToken: cancellationToken);
        }

        // log
    }

    protected override void DoDispose()
    {
        _stack.RequiredEnter -= Stack_RequiredEnter;
        _stack.RequiredDrop -= Stack_RequiredDrop;
    }

    private IHotCallDynRegistry _GetRegistry(bool localOnly)
    {
        if (localOnly)
        {
            return _GetLocalRegistry();
        }
        else
        {
            return _GetRemoteRegistry();
        }
    }

    private IHotCallDynRegistry _GetLocalRegistry()
    {
        return _serviceProvider.GetService<HotCallDynRegistry>() ??
               _serviceProvider.GetService<IHotCallDynRegistry>();
    }

    private IHotCallDynRegistry _GetRemoteRegistry()
    {
        return _serviceProvider.GetService<IHotCallDynRegistry>();
    }

    private static string _GetShort(Guid? userId) => userId?.ToString().Substring(0, 4) ?? "NULL";
}
