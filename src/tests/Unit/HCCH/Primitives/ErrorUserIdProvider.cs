using Lili.Protocol.General;
using System;

namespace Lili.Protocol.Tests.UnitTests;

internal sealed class ErrorUserIdProvider : IProvideUserId
{
    public Guid GetUserId()
    {
        throw new NotSupportedException();
    }
}