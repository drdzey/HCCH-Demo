using System;
using System.Threading.Tasks;

namespace Lili.Protocol.Core;

public interface IHubFactory
{
    event EventHandler Reconnected;

    Task<IWardenAsyncHub> GetWarden(Guid userId);

    Task<IFlicAsyncHub> GetFlic(Guid userId);

    Task<IUserAsyncHub> GetUser(Guid userId);

    Task<IUserMinistryAsyncHub> GetUserMinistry(Guid userId);

    Task<ICommAsyncHub> GetCommunication(Guid userId);

    Task<IMemoryAsyncHub> GetMemory(Guid userId);

    Task CloseDialAsync(Guid userId);

    Task TotalShutdownAsync();
}