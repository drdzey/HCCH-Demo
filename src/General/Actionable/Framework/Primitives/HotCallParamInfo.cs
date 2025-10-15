using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Calamara.Ng.Common.Flatable;
using Newtonsoft.Json.Linq;

// ReSharper disable ConvertToPrimaryConstructor

namespace Lili.Protocol.General;

public sealed class HotCallParamInfo : IProvideFlatable
{
    internal HotCallParamInfo()
    {
    }

    public HotCallParamInfo(
        string name,
        string description,
        Type type,
        bool hasDefault,
        object defaultValue)
    {
        Name = name;
        Description = description;
        Type = type;
        HasDefault = hasDefault;
        DefaultValue = defaultValue;
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public Type Type { get; private set; }

    public bool HasDefault { get; private set; }

    public object DefaultValue { get; set; }

    private FlatableHotCallParamInfo Flatten => new(InternalHotCallParamInfo.CreateFrom(this));

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IFlatable IProvideFlatable.Flatten => Flatten;

    public static implicit operator HotCallParamInfo(HotParamAttribute input)
    {
        var param = input.Parameter;
        return new HotCallParamInfo(
            input.Name,
            input.Description,
            param.ParameterType,
            param.HasDefaultValue,
            param.DefaultValue);
    }

    #region <IProvideFlatable>

    private sealed class FlatableHotCallParamInfo : FlatableBase<InternalHotCallParamInfo, HotCallParamInfo>
    {
        public FlatableHotCallParamInfo() { }

        public FlatableHotCallParamInfo(InternalHotCallParamInfo model) : base(model) { }

        protected override void DoInflate(InternalHotCallParamInfo flat, HotCallParamInfo target)
        {
            target.Name = flat.Name;
            target.Description = flat.Description;

            if (flat.HasDefault.HasValue)
            {
                target.HasDefault = flat.HasDefault.Value;
            }

            if (!string.IsNullOrEmpty(flat.Type) && flat.Type is { } type)
            {
                target.Type = type switch
                {
                    "String" => typeof(string),
                    "Int32" => typeof(int),
                    "Boolean" => typeof(bool),
                    "Double" => typeof(double),
                    "Float" => typeof(float),
                    "Object" => typeof(object),
                    _ => throw new InvalidOperationException($"Type '{type}' is not supported.")
                };
            }

            if (flat.DefaultValue != null && target.Type != null)
            {
                try
                {
                    target.DefaultValue = flat.DefaultValue.ToObject(target.Type);
                }
                catch
                {
                    target.DefaultValue = flat.DefaultValue;
                }
            }
        }
    }

    [DataContract]
    [Serializable]
    private sealed class InternalHotCallParamInfo
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "default")]
        public bool? HasDefault { get; set; }

        [DataMember(Name = "defaultVal")]
        public JToken DefaultValue { get; set; }

        public static InternalHotCallParamInfo CreateFrom(HotCallParamInfo bo) => new()
        {
            Name = bo.Name,
            Description = bo.Description,
            Type = bo.Type?.Name,
            HasDefault = bo.HasDefault,
            DefaultValue = bo.DefaultValue != null ? JToken.FromObject(bo.DefaultValue) : null
        };
    }

    #endregion <IProvideFlatable>
}
