using System;
using System.Runtime.Serialization;

namespace Lili.Protocol.General;

[DataContract]
[Serializable]
public class HotCallResult
{
    [DataMember(Name = "success", EmitDefaultValue = false)]
    public bool? Success { get; set; }

    [DataMember(Name = "val")]
    public object Value { get; set; }

    [DataMember(Name = "error")]
    public string ErrorMessage { get; set; }

    public static HotCallResult Failure(string message)
    {
        var result = new HotCallResult
        {
            Success = false,
            ErrorMessage = message
        };

        return result;
    }
}