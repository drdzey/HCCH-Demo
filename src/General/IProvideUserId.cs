using System;

namespace Lili.Protocol.General;

public interface IProvideUserId
{
    Guid GetUserId();
}