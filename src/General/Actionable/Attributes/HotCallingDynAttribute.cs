using System;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ConvertToPrimaryConstructor

namespace Lili.Protocol.General;

[AttributeUsage(AttributeTargets.Interface)]
public class HotCallingDynAttribute : Attribute
{
    public HotCallingDynAttribute(string key = null)
    {
        Key = key ?? HotCallSimpleKey.RootKey;
    }

    public string Key { get; }
}
