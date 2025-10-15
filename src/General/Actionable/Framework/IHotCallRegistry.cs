using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Lili.Protocol.General;

public interface IHotCallRegistry
{
    ConcurrentDictionary<string, HotCallInfo> GetSummary(Guid? userId = null);

    Task RegisterAsync(HotCallHandler handler, CancellationToken cancellationToken = default);

    Task UnregisterAsync(HotCallHandler handler, CancellationToken cancellationToken = default);

    Task RegisterDynAsync(Guid userId, HotCallHandler handler, CancellationToken cancellationToken = default);

    Task UnregisterDynAsync(Guid userId, HotCallHandler handler, CancellationToken cancellationToken = default);

    HotCallHandler FindHandler(string hotCall, Guid? userId = null);

    HotCallResult Invoke(string hotCall, Guid? userId, params object[] args);

    Task<HotCallResult> InvokeAsync(string hotCall, Guid? userId, params object[] args);
}