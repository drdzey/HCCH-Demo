// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

namespace Lili.Protocol.General;

public static class HotCallExtensions
{
    public static HotCallResult AsHotCallResult(this object input, bool? success = null)
    {
        return new HotCallResult
        {
            Value = input,
            Success = success
        };
    }
}