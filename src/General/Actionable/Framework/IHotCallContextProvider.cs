namespace Lili.Protocol.General;

public interface IHotCallContextProvider
{
    HotCallContext Current { get; set; }
}