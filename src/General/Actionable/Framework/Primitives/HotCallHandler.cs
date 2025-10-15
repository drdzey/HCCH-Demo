using System;
using System.Threading.Tasks;

// ReSharper disable ConvertToPrimaryConstructor

namespace Lili.Protocol.General;

public sealed class HotCallHandler : HotCallInfo
{
    public HotCallHandler(
        string name,
        string description,
        string category,
        string[] examples,
        HotCallParamInfo[] parameters,
        Func<object[], HotCallResult> syncHandler,
        Func<object[], Task<HotCallResult>> asyncHandler)
        : base(name, description, category, examples, parameters)
    {
        SyncHandler = syncHandler;
        AsyncHandler = asyncHandler;
    }

    public bool? IsAsync
    {
        get
        {
            if (SyncHandler != null)
            {
                return false;
            }

            if (AsyncHandler != null)
            {
                return true;
            }

            return null;
        }
    }

    public Func<object[], HotCallResult> SyncHandler { get; }

    public Func<object[], Task<HotCallResult>> AsyncHandler { get; }
}