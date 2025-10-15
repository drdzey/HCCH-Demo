using System;
// ReSharper disable ConvertToPrimaryConstructor
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global
#pragma warning disable S4035

namespace Lili.Protocol.General;

public class HotCallSimpleKey : IEquatable<HotCallSimpleKey>
{
    protected HotCallSimpleKey(string key)
    {
        Key = key;
    }

    public const string RootKey = "root";

    public string Key { get; protected set; }

    public static readonly HotCallSimpleKey Back = new("general_back");

    public bool Equals(HotCallSimpleKey other)
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
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((HotCallSimpleKey)obj);
    }

    public override int GetHashCode()
    {
        return Key != null ? Key.GetHashCode() : 0;
    }

    public override string ToString()
    {
        return $"KEY:{Key}";
    }

    public static HotCallSimpleKey FromKey(string key = null)
    {
        if (string.IsNullOrEmpty(key))
        {
            key = RootKey;
        }

        return new HotCallSimpleKey(key);
    }
}