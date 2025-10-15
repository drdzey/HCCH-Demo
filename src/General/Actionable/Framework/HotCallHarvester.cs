using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable ConvertToLocalFunction
// ReSharper disable UnusedType.Global

namespace Lili.Protocol.General;

public class HotCallHarvester : IHotCallHarvester
{
    public HotCallHarvester(
        IServiceProvider serviceProvider,
        IAppInfoProvider appInfo,
        IHotCallRegistry registry,
        IHotCallDynRegistry dynRegistry,
        ISharedLoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _appInfo = appInfo;
        _registry = registry;
        _dynRegistry = dynRegistry;
        _logger = loggerFactory.GetLogger(GetType());
    }

    // testing purposes
    internal HotCallHarvester(
        IServiceProvider serviceProvider,
        IAppInfoProvider appInfo,
        IHotCallRegistry registry,
        IHotCallDynRegistry dynRegistry,
        ISharedLoggerFactory loggerFactory,
        string ownerProcess)
        : this(serviceProvider, appInfo, registry, dynRegistry, loggerFactory)
    {
        _logger = loggerFactory.GetLogger($"{ownerProcess}::{GetType().Name}");
    }

    private readonly IServiceProvider _serviceProvider;
    private readonly IAppInfoProvider _appInfo;
    private readonly IHotCallRegistry _registry;
    private readonly IHotCallDynRegistry _dynRegistry;
    private readonly ISharedLogger _logger;

    public async Task HarvestStaticsAsync(
        Assembly assembly = null,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        const string nof = $"{nameof(HarvestStaticsAsync)}()";

        await _ExtractMethods<HotCallingAttribute>(
            assembly,
            afterCheck: async (_, localHandlers) =>
            {
                foreach (var handler in localHandlers)
                {
                    _logger.Log(nof, $"HotCall ({handler}) registered.");

                    if (userId.HasValue)
                    {
                        await _registry.RegisterDynAsync(userId.Value, handler, cancellationToken);
                    }
                    else
                    {
                        await _registry.RegisterAsync(handler, cancellationToken);
                    }
                }
            },
            cancellationToken: cancellationToken);
    }

    public async Task HarvestDynamicsAsync(
        Assembly assembly = null,
        CancellationToken cancellationToken = default)
    {
        const string nof = $"{nameof(HarvestDynamicsAsync)}()";

        var keys = new Dictionary<Type, string>();
        var result = new List<HotCallComplexKey>();

        await _ExtractMethods<HotCallingDynAttribute>(
            assembly,
            preCheck: @interface =>
            {
                var dynAttr = @interface.GetCustomAttribute<HotCallingDynAttribute>();
                var key = dynAttr?.Key ?? "root";
                keys[@interface] = key;
            },
            afterCheck: (@interface, localHandlers) =>
            {
                if (keys.TryGetValue(@interface, out var key))
                {
                    var item = new HotCallComplexKey(key, localHandlers);
                    _logger.Log(nof, $"HotCall ComplexKey ({item}) registered.");
                    result.Add(item);
                }

                return Task.CompletedTask;
            },
            cancellationToken: cancellationToken);

        await _dynRegistry.RegisterStartAsync(cancellationToken: cancellationToken);
        foreach (var complexKey in result)
        {
            await _dynRegistry.RegisterAsync(complexKey, cancellationToken: cancellationToken);
        }

        await _dynRegistry.RegisterEndAsync(cancellationToken: cancellationToken);
    }

    private async Task _ExtractMethods<TAttribute>(
        Assembly assembly = null,
        Action<Type> preCheck = null,
        Func<Type, List<HotCallHandler>, Task> afterCheck = null,
        CancellationToken cancellationToken = default)
        where TAttribute : Attribute
    {
        assembly ??= Assembly.GetExecutingAssembly();

        var interfaces = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<TAttribute>() != null)
            .ToList();

        foreach (var @interface in interfaces)
        {
            var h = _ExtractMethods(@interface);
            if (h.Count == 0)
            {
                continue;
            }

            var localHandlers = new List<HotCallHandler>();
            var impl = _serviceProvider.GetService(@interface); // if there is no implementation
            if (impl == null)
            {
                continue;
            }

            preCheck?.Invoke(@interface);

            foreach (var method in @interface.GetMethods())
            {
                var hotAttr = method.GetCustomAttribute<HotCallAttribute>();
                if (hotAttr == null)
                {
                    continue;
                }

                var parameters = method.GetParameters();
                var hotParams = _GetParams(parameters);

                hotAttr.Params = hotParams;
                var info = (HotCallInfo)hotAttr;
                info.IsLocal = true;
                info.Owner = _appInfo.ApplicationName;

                var handler = _ExtractHandler(@interface, method, info);
                if (handler != null)
                {
                    localHandlers.Add(handler);
                }
            }

            if (afterCheck != null)
            {
                await afterCheck.Invoke(@interface, localHandlers);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    private List<HotCallHandler> _ExtractMethods(Type @interface)
    {
        var handlers = new List<HotCallHandler>();

        var impl = _serviceProvider.GetService(@interface); // if there is no implementation
        if (impl == null)
        {
            return handlers;
        }

        foreach (var method in @interface.GetMethods())
        {
            var hotAttr = method.GetCustomAttribute<HotCallAttribute>();
            if (hotAttr == null)
            {
                continue;
            }

            var parameters = method.GetParameters();
            var hotParams = _GetParams(parameters);

            hotAttr.Params = hotParams;
            var info = (HotCallInfo)hotAttr;
            info.IsLocal = true;
            info.Owner = _appInfo.ApplicationName;

            var handler = _ExtractHandler(@interface, method, info);
            if (handler != null)
            {
                handlers.Add(handler);
            }
        }

        return handlers;
    }

    private static List<HotParamAttribute> _GetParams(ParameterInfo[] parameters)
    {
        var hotParams = parameters
            .Select(p =>
            {
                var hotParamAttr = p.GetCustomAttribute<HotParamAttribute>();
                return (Type: p, Attribute: hotParamAttr);
            })
            .Where(i => i.Attribute != null)
            .Select(i =>
            {
                i.Attribute.Parameter = i.Type;
                return i.Attribute;
            })
            .ToList();

        return hotParams;
    }

    private HotCallHandler _ExtractHandler(Type @interface, MethodInfo method, HotCallInfo info)
    {
        HotCallHandler handler = null;
        if (method.ReturnType == typeof(HotCallResult))
        {
            Func<object[], HotCallResult> syncHandler = args =>
            {
                var impl = _serviceProvider.GetService(@interface);
                var response = (HotCallResult)method.Invoke(impl, args);
                return response;
            };

            handler = info.CreateHandler(syncHandler);
        }
        else if (method.ReturnType == typeof(Task<HotCallResult>))
        {
            Func<object[], Task<HotCallResult>> asyncHandler = async args =>
            {
                var impl = _serviceProvider.GetService(@interface);
                var taskObj = method.Invoke(impl, args);
                if (taskObj is Task<HotCallResult> hotResultTask)
                {
                    return await hotResultTask;
                }

                throw new NotSupportedException($"Method ({method}) doesn't support return type.");
            };

            handler = info.CreateHandler(asyncHandler);
        }

        return handler;
    }
}