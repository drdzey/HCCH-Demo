using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Lili.Protocol.General;
using Microsoft.Extensions.Logging;
// ReSharper disable UnusedType.Global

namespace Lili.Protocol.Core;

public sealed class RemoteHotCallRegistry : IHotCallRegistry
{
    public RemoteHotCallRegistry(
        HotCallRegistry registry,
        IHubFactory factory,
        ISharedLoggerFactory loggerFactory)
    {
        _registry = registry;
        _factory = factory;
        _logger = loggerFactory.GetLogger(GetType());
    }

    // for testing purposes
    internal RemoteHotCallRegistry(
        HotCallRegistry registry,
        IHubFactory factory,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess)
        : this(registry, factory, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly HotCallRegistry _registry;
    private readonly IHubFactory _factory;
    private readonly ISharedLogger _logger;

    public ConcurrentDictionary<string, HotCallInfo> GetSummary(Guid? userId = null)
    {
        return _registry.GetSummary(userId);
    }

    public Task RegisterAsync(HotCallHandler handler, CancellationToken cancellationToken = default)
    {
        return _registry.RegisterAsync(handler, cancellationToken);
    }

    public Task UnregisterAsync(HotCallHandler handler, CancellationToken cancellationToken = default)
    {
        return _registry.UnregisterAsync(handler, cancellationToken);
    }

    public async Task RegisterDynAsync(Guid userId, HotCallHandler handler, CancellationToken cancellationToken = default)
    {
        var nof = $"{nameof(RegisterDynAsync)}({handler})";

        try
        {
            var comm = await _factory.GetCommunication(userId);
            await comm.RegisterHotCallAsync(handler);
            await _registry.RegisterDynAsync(userId, handler, cancellationToken);

            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }
        catch (Exception ex)
        {
            _logger.Log(nof, "FAILED.", LogLevel.Error, ex);
            throw;
        }
    }

    public async Task UnregisterDynAsync(Guid userId, HotCallHandler handler, CancellationToken cancellationToken = default)
    {
        var nof = $"{nameof(UnregisterDynAsync)}({handler})";

        try
        {
            var comm = await _factory.GetCommunication(userId);
            await comm.UnregisterHotCallAsync(handler);
            await _registry.UnregisterDynAsync(userId, handler, cancellationToken);

            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }
        catch (Exception ex)
        {
            _logger.Log(nof, "FAILED.", LogLevel.Error, ex);
            throw;
        }
    }

    public HotCallHandler FindHandler(string hotCall, Guid? userId = null)
    {
        return _registry.FindHandler(hotCall, userId);
    }

    public HotCallResult Invoke(string hotCall, Guid? userId, params object[] args)
    {
        return _registry.Invoke(hotCall, userId, args);
    }

    public Task<HotCallResult> InvokeAsync(string hotCall, Guid? userId, params object[] args)
    {
        return _registry.InvokeAsync(hotCall, userId, args);
    }
}