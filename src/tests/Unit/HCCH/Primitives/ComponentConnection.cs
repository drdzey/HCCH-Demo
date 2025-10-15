using System;

namespace Lili.Protocol.Tests.UnitTests;

internal sealed class ComponentConnection : IEquatable<ComponentConnection>
{
    private ComponentConnection(ComponentName from, ComponentName to)
    {
        if (from == to)
        {
            throw new ArgumentException("From and To cannot be the same.");
        }

        From = from < to ? from : to;
        To = from < to ? to : from;
    }

    public ComponentName From { get; }

    public ComponentName To { get; }

    public bool Equals(ComponentConnection other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return From == other.From && To == other.To;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is ComponentConnection other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)From * 397) ^ (int)To;
        }
    }

    public static ComponentConnection Between(ComponentName from, ComponentName to)
    {
        return new ComponentConnection(from, to);
    }
}