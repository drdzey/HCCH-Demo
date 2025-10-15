using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace Lili.Protocol.General;

public sealed class HotCallRegistry : IHotCallRegistry
{
    public HotCallRegistry(
        IHotCallContextProvider hotCallContextProvider,
        ISharedLoggerFactory loggerFactory)
    {
        _hotCallContextProvider = hotCallContextProvider;
        _logger = loggerFactory.GetLogger(GetType());
    }

    // for testing purposes
    internal HotCallRegistry(
        IHotCallContextProvider hotCallContextProvider,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess)
        : this(hotCallContextProvider, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly IHotCallContextProvider _hotCallContextProvider;
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, HotCallHandler>> _userHandlers = new();
    private readonly ConcurrentDictionary<string, HotCallHandler> _handlers = new();
    private readonly ISharedLogger _logger;

    public Task RegisterAsync(HotCallHandler handler, CancellationToken cancellationToken = default)
    {
        var nof = $"{nameof(RegisterAsync)}({handler})";
        _handlers[handler.ToString()] = handler;
        _logger.Log(nof, "DONE.", LogLevel.Trace);
        return Task.CompletedTask;
    }

    public Task UnregisterAsync(HotCallHandler handler, CancellationToken cancellationToken = default)
    {
        var nof = $"{nameof(UnregisterAsync)}({handler})";
        if (_handlers.TryRemove(handler.ToString(), out _))
        {
            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }

        return Task.CompletedTask;
    }

    public Task RegisterDynAsync(Guid userId, HotCallHandler handler, CancellationToken cancellationToken = default)
    {
        var nof = $"{nameof(RegisterDynAsync)}({_GetShort(userId)}, {handler})";
        var userMap = _userHandlers.GetOrAdd(userId, _ => new ConcurrentDictionary<string, HotCallHandler>());
        userMap[handler.ToString()] = handler;
        _logger.Log(nof, "DONE.", LogLevel.Trace);
        return Task.CompletedTask;
    }

    public Task UnregisterDynAsync(Guid userId, HotCallHandler handler, CancellationToken cancellationToken = default)
    {
        var nof = $"{nameof(UnregisterDynAsync)}({_GetShort(userId)}, {handler})";
        if (_userHandlers.TryGetValue(userId, out var userMap))
        {
            userMap.TryRemove(handler.ToString(), out _);

            if (userMap.IsEmpty)
            {
                _userHandlers.TryRemove(userId, out _);
            }

            _logger.Log(nof, "DONE.", LogLevel.Trace);
        }

        return Task.CompletedTask;
    }

    public HotCallHandler FindHandler(string hotCall, Guid? userId = null)
    {
        if (userId != null &&
            _userHandlers.TryGetValue(userId.Value, out var userMap) &&
            userMap.TryGetValue(hotCall, out var userHandler))
        {
            return userHandler;
        }

        if (_handlers.TryGetValue(hotCall, out var handler))
        {
            return handler;
        }

        throw new ArgumentException($"Handler '{hotCall}' not found (UserId: {userId?.ToString() ?? "GLOBAL"}).");
    }

    public ConcurrentDictionary<string, HotCallInfo> GetSummary(Guid? userId = null)
    {
        if (userId == null)
        {
            return new ConcurrentDictionary<string, HotCallInfo>(_handlers
                .ToDictionary(
                    kvp => kvp.Key, HotCallInfo (kvp) => kvp.Value));
        }
        else
        {
            var result = new Dictionary<string, HotCallInfo>();
            foreach (var kvp in _handlers)
            {
                result[kvp.Key] = kvp.Value;
            }

            if (_userHandlers.TryGetValue(userId.Value, out var userMap))
            {
                foreach (var kvp in userMap)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return new ConcurrentDictionary<string, HotCallInfo>(result);
        }
    }

    public HotCallResult Invoke(string hotCall, Guid? userId, params object[] args)
    {
        const string nof = $"{nameof(Invoke)}()";
        HotCallResult result = null;

        try
        {
            var handler = FindHandler(hotCall, userId);

            if (args.Length != handler.Parameters.Length)
            {
                throw new ArgumentException($"Handler '{hotCall}' expects {handler.Parameters.Length} parameters, but invoked with {args.Length}.");
            }

            var ctx = _hotCallContextProvider.Current;
            if (ctx != null)
            {
                ctx.HotCallInfo = handler;
            }

            if (handler.SyncHandler == null)
            {
                throw new NotSupportedException($"Handler '{hotCall}' does not support sync invocation.");
            }

            result = handler.SyncHandler(args);
        }
        catch (Exception ex)
        {
            _logger.Log(nof, $"Error invoking hot call '{hotCall}'", LogLevel.Error, ex);

            result ??= new HotCallResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };

            return result;
        }

        result.Success ??= true;
        return result;
    }

    public async Task<HotCallResult> InvokeAsync(string hotCall, Guid? userId, params object[] args)
    {
        const string nof = $"{nameof(InvokeAsync)}()";
        HotCallResult result = null;

        try
        {
            var handler = FindHandler(hotCall, userId);

            if (args.Length != handler.Parameters.Length)
            {
                throw new ArgumentException($"Handler '{hotCall}' expects {handler.Parameters.Length} parameters, but invoked with {args.Length}.");
            }

            var ctx = _hotCallContextProvider.Current;
            if (ctx != null)
            {
                ctx.HotCallInfo = handler;
            }

            if (handler.IsAsync == true)
            {
                result = await handler.AsyncHandler(args);
            }
            else if (handler.IsAsync == false)
            {
                result = handler.SyncHandler(args);
            }
        }
        catch (Exception ex)
        {
            _logger.Log(nof, $"Error invoking hot call '{hotCall}'", LogLevel.Error, ex);

            result ??= new HotCallResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };

            return result;
        }

        if (result != null)
        {
            result.Success ??= true;
        }

        return result;
    }

    private static string _GetShort(Guid? userId) => userId?.ToString().Substring(0, 4) ?? "NULL";
}
