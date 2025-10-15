using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

// ReSharper disable ConvertConstructorToMemberInitializers

namespace Lili.Protocol.General;

[DataContract]
[Serializable]
public sealed class HotCallContext
{
    public HotCallContext()
    {
        Metadata = new JObject();
    }

    [IgnoreDataMember]
    public HotCallInfo HotCallInfo { get; internal set; }

    [DataMember(Name = "user")]
    public Guid? UserId { get; set; }

    [DataMember(Name = "thread-id")]
    public Guid? ThreadId { get; set; }

    [DataMember(Name = "thread-name")]
    public string ThreadName { get; set; }

    [DataMember(Name = "turn")]
    public Guid? TurnId { get; set; }

    [DataMember(Name = "persona")]
    public Guid? PersonaId { get; set; }

    [DataMember(Name = "essence")]
    public Guid? EssenceId { get; set; }

    [DataMember(Name = "metadata")]
    public JObject Metadata { get; set; }
}