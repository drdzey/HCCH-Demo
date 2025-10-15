using System;
using System.Reflection;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Lili.Protocol.General;

[AttributeUsage(AttributeTargets.Parameter)]
public class HotParamAttribute : Attribute
{
    public HotParamAttribute(string name, string description = null)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; }

    public string Description { get; }

    public ParameterInfo Parameter { get; set; }
}
