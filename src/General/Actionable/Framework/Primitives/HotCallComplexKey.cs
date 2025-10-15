using Calamara.Ng.Common.Flatable;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable UnusedType.Global

namespace Lili.Protocol.General;

public sealed class HotCallComplexKey : HotCallSimpleKey, IEquatable<HotCallComplexKey>, IProvideFlatable
{
    private HotCallComplexKey() : base(null)
    {
        Handlers = [];
    }

    public HotCallComplexKey(string key, List<HotCallHandler> handlers) : base(key)
    {
        Handlers = handlers;
    }

    public List<HotCallHandler> Handlers { get; private set; }

    private FlatableHotCallComplexKey Flatten => new(InternalHotCallComplexKey.CreateFrom(this));

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    IFlatable IProvideFlatable.Flatten => Flatten;

    public bool Equals(HotCallComplexKey other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Key == other.Key;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) ||
               obj is HotCallSimpleKey other1 && Equals(other1) ||
               obj is HotCallComplexKey other2 && Equals(other2);
    }

    public override int GetHashCode()
    {
        return Key != null ? Key.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return $"KEY:{Key}({Handlers.Count})";
    }

    #region <IProvideFlatable>

    private sealed class FlatableHotCallComplexKey : FlatableBase<InternalHotCallComplexKey, HotCallComplexKey>
    {
        public FlatableHotCallComplexKey() { }

        public FlatableHotCallComplexKey(InternalHotCallComplexKey model) : base(model) { }

        protected override void DoInflate(InternalHotCallComplexKey flat, HotCallComplexKey target)
        {
            target.Key = flat.Key;
            target.Handlers = flat.Handlers?.ToList() ?? [];
        }
    }

    [DataContract]
    [Serializable]
    private sealed class InternalHotCallComplexKey
    {
        [DataMember(Name = "key")]
        public string Key { get; set; }

        [IgnoreDataMember]
        public HotCallHandler[] Handlers
        {
            get
            {
                if (HandlersModel == null)
                {
                    return [];
                }

                var list = new List<HotCallHandler>();
                foreach (var item in HandlersModel)
                {
                    if (item is not JObject model)
                    {
                        continue;
                    }

                    var param = new HotCallInfo();
                    var flatten = ((IProvideFlatable)param).Flatten;
                    flatten.Inflate(model, param);
                    var handler = param.AsHandler();
                    handler.IsLocal = false;
                    list.Add(handler);
                }

                return list.ToArray();
            }

            set
            {
                var array = new JArray();
                if (value == null || value.Length == 0)
                {
                    HandlersModel = array;
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

                HandlersModel = array;
            }
        }

        [DataMember(Name = "handlers")]
        public JArray HandlersModel { get; set; }

        public static InternalHotCallComplexKey CreateFrom(HotCallComplexKey bo) => new()
        {
            Key = bo.Key,
            Handlers = bo.Handlers?.ToArray() ?? []
        };
    }

    public static HotCallComplexKey FromFlatable(string flat)
    {
        var result = new HotCallComplexKey();
        var flatten = ((IProvideFlatable)result).Flatten;
        flatten.Inflate(flat, result);
        return result;
    }

    #endregion <IProvideFlatable>
}