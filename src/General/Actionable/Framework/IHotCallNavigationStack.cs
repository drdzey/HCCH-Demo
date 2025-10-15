using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lili.Protocol.General;

public interface IHotCallNavigationStack
{
    event AsyncEventHandler<(HotCallSimpleKey Key, Guid? UserId, string Owner, bool LocalOnly)> RequiredEnter;

    event AsyncEventHandler<(HotCallSimpleKey Key, Guid? UserId, string Owner, bool LocalOnly)> RequiredDrop;

    HotCallSimpleKey GetCurrentSection(Guid? userId = null, string owner = null);

    HotCallSimpleKey[] GetSections(Guid? userId = null, string owner = null);

    HotCallSingleChain GetCurrentChain(Guid? userId = null, string owner = null);

    string GetSectionPath(Guid? userId = null, string owner = null);

    Task GoAsync(HotCallSimpleKey key, Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default);

    Task GoBackAsync(Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default);

    // only local
    Task InitAsync(Guid? userId, string owner = null, CancellationToken cancellationToken = default);

    // only local
    Task ClearAsync(Guid? userId, string owner = null, CancellationToken cancellationToken = default);
}