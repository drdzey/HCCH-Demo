using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lili.Protocol.General;

public interface IHotCallNavigationGate
{
    event AsyncEventHandler<(Guid? UserId, string Owner)> GateIdle;

    void Go(HotCallSimpleKey key, Guid? userId = null, string owner = null);

    void GoBack(Guid? userId = null, string owner = null);

    Task WaitForIdleAsync(HotCallSingleChain chain = null, Guid? userId = null, string owner = null, bool? localOnly = null, CancellationToken cancellationToken = default);
}