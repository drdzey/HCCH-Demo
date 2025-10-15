using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lili.Protocol.General;

public interface IHotCallDynRegistry
{
    Task RegisterStartAsync(Guid? userId = null, string owner = null, CancellationToken cancellationToken = default);

    Task RegisterAsync(HotCallComplexKey key, Guid? userId = null, string owner = null, CancellationToken cancellationToken = default);

    Task RegisterEndAsync(Guid? userId = null, string owner = null, CancellationToken cancellationToken = default);

    Task EnterSectionAsync(HotCallSimpleKey key = null, Guid? userId = null, string owner = null, CancellationToken cancellationToken = default);

    Task DropSectionAsync(HotCallSimpleKey key = null, Guid? userId = null, string owner = null, CancellationToken cancellationToken = default);
}