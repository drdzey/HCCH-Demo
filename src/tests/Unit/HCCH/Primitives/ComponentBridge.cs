using System;
using System.Collections.Generic;
using Lili.Protocol.Core;
using NSubstitute;

namespace Lili.Protocol.Tests.UnitTests;

internal sealed class ComponentBridge
{
    public ComponentBridge(
        ComponentConnection connection,
        ComponentBox server)
    {
        Connection = connection;
        Server = server;

        ServerSide = _CreateHubFactory(server: Server);
        ClientSide = _CreateHubFactory();
    }

    public ComponentConnection Connection { get; }

    public ComponentBox Server { get; }

    public IHubFactory ClientSide { get; }

    public IHubFactory ServerSide { get; }

    private readonly Dictionary<Guid, FakedServerCommAsyncHub> _servers = new();
    private readonly Dictionary<Guid, FakedClientCommAsyncHub> _clients = new();

    private IHubFactory _CreateHubFactory(ComponentBox server = null)
    {
        var hubFactory = Substitute.For<IHubFactory>();
        hubFactory.GetCommunication(Arg.Any<Guid>()).Returns(async info =>
        {
            var userId = info.Arg<Guid>();

            ICommAsyncHub faked = null;
            if (server != null)
            {
                if (!_servers.TryGetValue(userId, out var existing))
                {
                    existing = new FakedServerCommAsyncHub(userId, server);
                    _servers[userId] = existing;
                }

                faked = existing;
            }
            else
            {
                if (!_clients.TryGetValue(userId, out var existing))
                {
                    existing = new FakedClientCommAsyncHub(userId);

                    var serverSide = await ServerSide.GetCommunication(userId);
                    existing.Server = serverSide;

                    _clients[userId] = existing;
                }

                faked = existing;
            }

            return faked;
        });

        return hubFactory;
    }
}
