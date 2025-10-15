using Lili.Protocol.General;
using System;

namespace Lili.Protocol.Tests.UnitTests;

internal sealed class InternalUserIdProvider(Guid userId) : IProvideUserId
{
    public Guid GetUserId()
    {
        return userId;
    }
}