using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Calamara.Core;
using Calamara.Ng.Common.Flatable;
using Newtonsoft.Json.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable ConvertToPrimaryConstructor

#pragma warning disable S4035
namespace Lili.Protocol.General;

public class HotCallInfo : IEquatable<HotCallInfo>, IProvideFlatable
{
    internal HotCallInfo()
    {
    }

    public HotCallInfo(
        string name,
        string description,
        string category,
        string[] examples,
        HotCallParamInfo[] parameters)
    {
        Name = name;
        Description = description;
        Category = category;
        Examples = examples;
        Parameters = parameters ?? [];
    }

    public string Name { get; private set; }

    public string Description { get; private set; }

    public string Category { get; private set; }

    public string[] Examples { get; private set; }

    public HotCallParamInfo[] Parameters { get; private set; }

    public bool IsLocal { get; set; }

    public string Owner { get; set; }

    private FlatableHotCallInfo Flatten => new(InternalHotCallInfo.CreateFrom(this));

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IFlatable IProvideFlatable.Flatten => Flatten;

    public bool Equals(HotCallInfo other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Name == other.Name;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is HotCallInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Name != null ? Name.GetHashCode() : 0;
    }

    public static implicit operator HotCallInfo(HotCallAttribute input)
    {
        return new HotCallInfo(
            input.Name,
            input.Description,
            input.Category,
            input.Examples,
            input.Params.Select(i => (HotCallParamInfo)i).ToArray());
    }

    public HotCallHandler CreateHandler(Func<object[], HotCallResult> syncHandler)
    {
        return new HotCallHandler(
            Name,
            Description,
            Category,
            Examples,
            Parameters,
            syncHandler,
            null)
        {
            IsLocal = IsLocal,
            Owner = Owner
        };
    }

    public HotCallHandler CreateHandler(Func<object[], Task<HotCallResult>> asyncHandler)
    {
        return new HotCallHandler(
            Name,
            Description,
            Category,
            Examples,
            Parameters,
            null,
            asyncHandler)
        {
            IsLocal = IsLocal,
            Owner = Owner
        };
    }

    public HotCallHandler AsHandler()
    {
        return new HotCallHandler(
            Name,
            Description,
            Category,
            Examples,
            Parameters,
            null,
            null)
        {
            IsLocal = IsLocal,
            Owner = Owner
        };
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Category))
        {
            return $"{Name.ConvertFirstLetterToLowerCase()}";
        }

        return $"{Category.ConvertFirstLetterToLowerCase()}{Separator}{Name.ConvertFirstLetterToLowerCase()}";
    }

    private const string Separator = "_";

    public static string GetName(string hotCall, out string category)
    {
        if (hotCall.EndsWith("()", StringComparison.InvariantCultureIgnoreCase))
        {
            hotCall = hotCall.Substring(0, hotCall.Length - 2);
        }

        const string sep = Separator;
        var idx = hotCall.IndexOf(sep, StringComparison.InvariantCulture);

        if (idx >= 0)
        {
            category = hotCall.Substring(0, idx);
            var name = hotCall.Substring(idx + sep.Length);
            return name;
        }
        else
        {
            category = null;
            return hotCall;
        }
    }

    public static HotCallInfo FromFlatable(string flat)
    {
        var result = new HotCallInfo();
        var flatten = ((IProvideFlatable)result).Flatten;
        flatten.Inflate(flat, result);
        return result;
    }

    #region <IProvideFlatable>

    private sealed class FlatableHotCallInfo : FlatableBase<InternalHotCallInfo, HotCallInfo>
    {
        public FlatableHotCallInfo() { }

        public FlatableHotCallInfo(InternalHotCallInfo model) : base(model) { }

        protected override void DoInflate(InternalHotCallInfo flat, HotCallInfo target)
        {
            target.Name = flat.Name;
            target.Description = flat.Description;
            target.Category = flat.Category;
            target.Examples = flat.Examples?.ToArray();

            target.Parameters = flat.Parameters?.ToArray();
            target.IsLocal = flat.IsLocal ?? false;
            target.Owner = flat.Owner;
        }
    }

    [DataContract]
    [Serializable]
    private sealed class InternalHotCallInfo
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "category")]
        public string Category { get; set; }

        [DataMember(Name = "examples")]
        public string[] Examples { get; set; }

        [IgnoreDataMember]
        public HotCallParamInfo[] Parameters
        {
            get
            {
                if (ParametersModel == null)
                {
                    return [];
                }

                var list = new List<HotCallParamInfo>();
                foreach (var item in ParametersModel)
                {
                    if (item is not JObject model)
                    {
                        continue;
                    }

                    var param = new HotCallParamInfo();
                    var flatten = ((IProvideFlatable)param).Flatten;
                    flatten.Inflate(model, param);
                    list.Add(param);
                }

                return list.ToArray();
            }

            set
            {
                var array = new JArray();
                if (value == null || value.Length == 0)
                {
                    ParametersModel = array;
                    return;
                }

                foreach (var item in value)
                {
                    var flatten = ((IProvideFlatable)item).Flatten;
                    var model = flatten.FlatifyAsJObject();
                    if (model != null)
                    {
                        array.Add(model);
                    }
                }

                ParametersModel = array;
            }
        }

        [DataMember(Name = "params")]
        public JArray ParametersModel { get; set; }

        [DataMember(Name = "local")]
        public bool? IsLocal { get; set; }

        [DataMember(Name = "owner")]
        public string Owner { get; set; }

        public static InternalHotCallInfo CreateFrom(HotCallInfo bo) => new()
        {
            Name = bo.Name,
            Description = bo.Description,
            Category = bo.Category,
            Examples = bo.Examples,
            Parameters = bo.Parameters?.ToArray() ?? [],
            IsLocal = bo.IsLocal,
            Owner = bo.Owner
        };
    }

    #endregion <IProvideFlatable>
}