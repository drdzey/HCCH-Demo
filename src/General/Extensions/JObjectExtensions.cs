using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Lili.Protocol.General;

public static class JObjectExtensions
{
    public static JObject ToJObject(this IDictionary<string, object> input)
    {
        return JObject.FromObject(input);
    }

    public static IDictionary<string, object> FromJObject(this JObject jobj)
    {
        if (jobj == null)
        {
            return new Dictionary<string, object>();
        }

        return jobj
            .Properties()
            .ToDictionary(
                prop => prop.Name,
                prop => _ConvertJToken(prop.Value));
    }

    private static object _ConvertJToken(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Integer: return token.Value<long>();
            case JTokenType.Float: return token.Value<double>();
            case JTokenType.Boolean: return token.Value<bool>();
            case JTokenType.String: return token.Value<string>();
            case JTokenType.Array:
                return token.Select(_ConvertJToken).ToList(); // List<object>
            case JTokenType.Object:
                return FromJObject((JObject)token); // recursive!
            case JTokenType.Null: return null;
            default: return token.ToString();
        }
    }
}