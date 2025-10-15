using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lili.Protocol.General;

public sealed class HotCallDynRegistry : IHotCallDynRegistry
{
    public HotCallDynRegistry(
        IServiceProvider serviceProvider,
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfoProvider,
        IHotCallNavigationStack navigationStack,
        ISharedLoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _userIdProvider = userIdProvider;
        _appInfoProvider = appInfoProvider;
        _navigationStack = navigationStack;
        _registry = new Lazy<IHotCallRegistry>(_GetRegistry);

        _logger = loggerFactory.GetLogger(GetType());
    }

    // for testing purposes
    internal HotCallDynRegistry(
        IServiceProvider serviceProvider,
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfoProvider,
        IHotCallNavigationStack navigationStack,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess)
        : this(serviceProvider, userIdProvider, appInfoProvider, navigationStack, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly ISharedLogger _logger;
    private readonly Lazy<IHotCallRegistry> _registry;
    private readonly IServiceProvider _serviceProvider;
    private readonly IProvideUserId _userIdProvider;
    private readonly IAppInfoProvider _appInfoProvider;
    private readonly IHotCallNavigationStack _navigationStack;
    private readonly ConcurrentDictionary<(Guid UserId, string Owner), ConcurrentDictionary<HotCallSimpleKey, HotCallComplexKey>> _keys = new();
    private readonly ConcurrentDictionary<(Guid UserId, string Owner), bool> _isFinalized = new();

    internal bool IsRemote { get; set; }

    private IHotCallRegistry Registry => _registry.Value;

    public async Task RegisterStartAsync(Guid? userId = null, string owner = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;
        var nof = $"{nameof(RegisterStartAsync)}({_GetShort(userId)}, {owner})";

        var compoundKey = (userId.Value, owner);

        await _navigationStack.ClearAsync(userId: userId, owner: owner, cancellationToken: cancellationToken);
        _keys.TryRemove(compoundKey, out _);

        _isFinalized[compoundKey] = false;
        _logger.Log(nof, "DONE.", LogLevel.Trace);
    }

    public Task RegisterAsync(HotCallComplexKey key, Guid? userId = null, string owner = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;
        var nof = $"{nameof(RegisterAsync)}({key}, {_GetShort(userId)}, {owner})";

        var compoundKey = (userId.Value, owner);

        try
        {
            if (!_isFinalized.TryGetValue(compoundKey, out var isFinalized))
            {
                throw new InvalidOperationException("Register must call Start() first.");
            }

            if (isFinalized)
            {
                throw new InvalidOperationException("Chain already finalized, cannot register new section.");
            }

            var dict = _keys.GetOrAdd(compoundKey, _ => new ConcurrentDictionary<HotCallSimpleKey, HotCallComplexKey>());
            dict[key] = key;

            _logger.Log(nof, "DONE.", LogLevel.Trace);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.Log(nof, "FAILED.", LogLevel.Error, ex);
            throw;
        }
    }

    public async Task RegisterEndAsync(Guid? userId = null, string owner = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;
        var nof = $"{nameof(RegisterEndAsync)}({_GetShort(userId)}, {owner})";

        var compoundKey = (userId.Value, owner);

        try
        {
            if (!_isFinalized.TryGetValue(compoundKey, out var finalized) && finalized)
            {
                throw new InvalidOperationException("Chain already finalized, cannot register new section.");
            }

            _isFinalized[compoundKey] = true;

            await _navigationStack.InitAsync(userId: userId, owner: owner, cancellationToken: cancellationToken);
            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }
        catch (Exception ex)
        {
            _logger.Log(nof, "FAILED.", LogLevel.Error, ex);
            throw;
        }
    }

    public async Task EnterSectionAsync(HotCallSimpleKey key = null, Guid? userId = null, string owner = null, CancellationToken cancellationToken = default)
    {
        key ??= HotCallSimpleKey.FromKey();
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;
        var nof = $"{nameof(EnterSectionAsync)}({key}, {_GetShort(userId)}, {owner})";

        var compoundKey = (userId.Value, owner);

        if (_keys.TryGetValue(compoundKey, out var dict))
        {
            if (!dict.TryGetValue(key, out var complexKey))
            {
                _logger.Log(nof, $"Key ({key}) not found.", LogLevel.Warning);
                return;
            }

            foreach (var handler in complexKey.Handlers)
            {
                await Registry.RegisterDynAsync(userId.Value, handler, cancellationToken);
            }

            _logger.Log(nof, $"ComplexKey HotCall registered for section ({key}) and user ({userId}).", LogLevel.Trace);
        }
    }

    public async Task DropSectionAsync(HotCallSimpleKey key = null, Guid? userId = null, string owner = null, CancellationToken cancellationToken = default)
    {
        key ??= HotCallSimpleKey.FromKey();
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;
        var nof = $"{nameof(DropSectionAsync)}({key}, {_GetShort(userId)}, {owner})";

        var compoundKey = (userId.Value, owner);

        if (_keys.TryGetValue(compoundKey, out var dict))
        {
            if (!dict.TryGetValue(key, out var value))
            {
                _logger.Log(nof, $"Key ({key}) not found.", LogLevel.Warning);
                return;
            }

            var handlers = value.Handlers;
            foreach (var handler in handlers)
            {
                await Registry.UnregisterDynAsync(userId.Value, handler, cancellationToken);
            }

            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }
    }

    private IHotCallRegistry _GetRegistry()
    {
        const string nof = $"{nameof(_GetRegistry)}()";
        IHotCallRegistry registry = null;
        if (IsRemote)
        {
            registry = _GetLocalRegistry();
        }
        else
        {
            registry = _GetRemoteRegistry();
        }

        if (registry != null)
        {
            _logger.Log(nof, $"Provided ({registry.GetType().Name}) with IsRemote ({IsRemote})", LogLevel.Trace);
        }
        else
        {
            _logger.Log(nof, $"Cannot resolve registry with IsRemote ({IsRemote})", LogLevel.Warning);
        }

        return registry;
    }

    private IHotCallRegistry _GetLocalRegistry()
    {
        return _serviceProvider.GetService<HotCallRegistry>() ??
               _serviceProvider.GetService<IHotCallRegistry>();
    }

    private IHotCallRegistry _GetRemoteRegistry()
    {
        return _serviceProvider.GetService<IHotCallRegistry>();
    }

    private static string _GetShort(Guid? userId) => userId?.ToString().Substring(0, 4) ?? "NULL";
}