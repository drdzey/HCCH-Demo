using System;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.General;

namespace Lili.Protocol.General;

public interface IHotCallNavigationService : IDisposableEx
{
    Task GoAsync(HotCallSimpleKey key, Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default);

    Task GoBackAsync(Guid? userId = null, string owner = null, bool localOnly = false, CancellationToken cancellationToken = default);
}