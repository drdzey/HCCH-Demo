using System;
using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ConvertToPrimaryConstructor

#pragma warning disable S3604
namespace Lili.Protocol.General;

[AttributeUsage(AttributeTargets.Method)]
public class HotCallAttribute(string name, string description) : Attribute
{
    public string Name { get; } = name;

    public string Description { get; } = description;

    public string Category { get; set; }

    public string[] Examples { get; set; } = [];

    public List<HotParamAttribute> Params { get; set; } = [];
}