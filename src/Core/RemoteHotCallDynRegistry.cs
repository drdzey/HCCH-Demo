using System;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Lili.Protocol.General;
using Microsoft.Extensions.Logging;

namespace Lili.Protocol.Core;

public sealed class RemoteHotCallDynRegistry : IHotCallDynRegistry
{
    public RemoteHotCallDynRegistry(
        HotCallDynRegistry dynRegistry,
        IHubFactory factory,
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfoProvider,
        ISharedLoggerFactory loggerFactory)
    {
        _dynRegistry = dynRegistry;
        _factory = factory;
        _userIdProvider = userIdProvider;
        _appInfoProvider = appInfoProvider;
        _logger = loggerFactory.GetLogger(GetType());

        dynRegistry.IsRemote = true;
    }

    // for testing purposes
    internal RemoteHotCallDynRegistry(
        HotCallDynRegistry dynRegistry,
        IHubFactory factory,
        IProvideUserId userIdProvider,
        IAppInfoProvider appInfoProvider,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess)
        : this(dynRegistry, factory, userIdProvider, appInfoProvider, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly HotCallDynRegistry _dynRegistry;
    private readonly IHubFactory _factory;
    private readonly IProvideUserId _userIdProvider;
    private readonly IAppInfoProvider _appInfoProvider;
    private readonly ISharedLogger _logger;

    public async Task RegisterStartAsync(Guid? userId = null, string owner = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;
        var nof = $"{nameof(RegisterStartAsync)}({_GetShort(userId)}, {owner})";

        try
        {
            var comm = await _factory.GetCommunication(userId.Value);
            await comm.RegisterHotCallDynStartAsync(owner);
            await _dynRegistry.RegisterStartAsync(userId, owner, cancellationToken);
            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }
        catch (Exception ex)
        {
            _logger.Log(nof, "FAILED.", LogLevel.Error, ex);
            throw;
        }
    }

    public async Task RegisterAsync(HotCallComplexKey key, Guid? userId = null, string owner = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;
        var nof = $"{nameof(RegisterAsync)}({key}, {_GetShort(userId)}, {owner})";

        try
        {
            var comm = await _factory.GetCommunication(userId.Value);
            await comm.RegisterHotCallDynAsync(owner, key);
            await _dynRegistry.RegisterAsync(key, userId, owner, cancellationToken);

            _logger.Log(nof, "DONE.", LogLevel.Trace);
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

        try
        {
            var comm = await _factory.GetCommunication(userId.Value);
            await comm.RegisterHotCallDynEndAsync(owner);
            await _dynRegistry.RegisterEndAsync(userId, owner, cancellationToken);

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
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;

        key ??= HotCallSimpleKey.FromKey();
        var nof = $"{nameof(EnterSectionAsync)}({key}, {_GetShort(userId)}, {owner})";

        try
        {
            var comm = await _factory.GetCommunication(userId.Value);
            await comm.EnterHotCallDynSectionAsync(owner, key);
            await _dynRegistry.EnterSectionAsync(key, userId, owner, cancellationToken);

            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }
        catch (Exception ex)
        {
            _logger.Log(nof, "FAILED.", LogLevel.Error, ex);
            throw;
        }
    }

    public async Task DropSectionAsync(HotCallSimpleKey key = null, Guid? userId = null, string owner = null, CancellationToken cancellationToken = default)
    {
        userId ??= _userIdProvider.GetUserId();
        owner ??= _appInfoProvider.ApplicationName;

        key ??= HotCallSimpleKey.FromKey();
        var nof = $"{nameof(DropSectionAsync)}({key}, {_GetShort(userId)}, {owner})";

        try
        {
            var comm = await _factory.GetCommunication(userId.Value);
            await comm.DropHotCallDynSectionAsync(owner, key);
            await _dynRegistry.DropSectionAsync(key, userId, owner, cancellationToken);

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