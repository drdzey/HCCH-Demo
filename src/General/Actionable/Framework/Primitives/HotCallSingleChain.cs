using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Calamara.Ng.Common.Flatable;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable UnusedType.Global

namespace Lili.Protocol.General;

public sealed class HotCallSingleChain : IEquatable<HotCallSingleChain>, IProvideFlatable
{
    private HotCallSingleChain()
    {
        Keys = [];
    }

    private HotCallSingleChain(List<HotCallSimpleKey> keys)
    {
        Keys = keys;
    }

    public List<HotCallSimpleKey> Keys { get; private set; }

    private FlatableHotCallSingleChain Flatten => new(InternalHotCallSingleChain.CreateFrom(this));

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IFlatable IProvideFlatable.Flatten => Flatten;

    public bool Equals(HotCallSingleChain other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return ToString() == other.ToString();
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) ||
               obj is HotCallSimpleKey other1 && Equals(other1) ||
               obj is HotCallComplexKey other2 && Equals(other2);
    }

    public override int GetHashCode()
    {
        return ToString() != null ? ToString().GetHashCode() : 0;
    }

    public override string ToString()
    {
        return $"->{string.Join(":", Keys.Select(i => i.Key))}";
    }

    public static implicit operator HotCallSingleChain(HotCallSimpleKey[] keys)
    {
        return new HotCallSingleChain(keys?.ToList() ?? []);
    }

    #region <IProvideFlatable>

    private sealed class FlatableHotCallSingleChain : FlatableBase<InternalHotCallSingleChain, HotCallSingleChain>
    {
        public FlatableHotCallSingleChain() { }

        public FlatableHotCallSingleChain(InternalHotCallSingleChain model) : base(model) { }

        protected override void DoInflate(InternalHotCallSingleChain flat, HotCallSingleChain target)
        {
            target.Keys = flat.Keys?.ToList() ?? [];
        }
    }

    [DataContract]
    [Serializable]
    private sealed class InternalHotCallSingleChain
    {
        [IgnoreDataMember]
        public HotCallSimpleKey[] Keys
        {
            get
            {
                if (KeysModel == null)
                {
                    return [];
                }

                var list = new List<HotCallSimpleKey>();
                foreach (var item in KeysModel)
                {
                    if (item is not { } model)
                    {
                        continue;
                    }

                    var param = HotCallSimpleKey.FromKey(model);
                    list.Add(param);
                }

                return list.ToArray();
            }

            set
            {
                var array = new List<string>();
                if (value == null || value.Length == 0)
                {
                    KeysModel = array.ToArray();
                    return;
                }

                foreach (var item in value)
                {
                    array.Add(item.Key);
                }

                KeysModel = array.ToArray();
            }
        }

        [DataMember(Name = "keys")]
        public string[] KeysModel { get; set; } = [];

        public static InternalHotCallSingleChain CreateFrom(HotCallSingleChain bo) => new()
        {
            Keys = bo.Keys?.ToArray() ?? []
        };
    }

    public static HotCallSingleChain FromFlatable(string flat)
    {
        var result = new HotCallSingleChain();
        var flatten = ((IProvideFlatable)result).Flatten;
        flatten.Inflate(flat, result);
        return result;
    }

    #endregion <IProvideFlatable>
}