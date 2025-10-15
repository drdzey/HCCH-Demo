using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global

namespace Lili.Protocol.General;

public interface IHotCallHarvester
{
    Task HarvestStaticsAsync(Assembly assembly = null, Guid? userId = null, CancellationToken cancellationToken = default);

    Task HarvestDynamicsAsync(Assembly assembly = null, CancellationToken cancellationToken = default);
}