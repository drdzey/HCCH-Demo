using Calamara.Ng.Common.Flatable;
using Lili.Protocol.General;
using Xunit;

namespace Lili.Protocol.Tests.UnitTests;

public sealed class HotCallHandlerTests
{
    [Fact]
    public void FlatAndInflate_DoExample_Ok()
    {
        // ARRANGE
        var input = new HotCallInfo(
            "Test",
            "This is description",
            "general",
            ["This is example."],
            [new HotCallParamInfo("directory", "directory", typeof(string), true, "bubu")])
        {
            IsLocal = true,
            Owner = "jappa"
        };

        // ACT
        var flatten = ((IProvideFlatable)input).Flatten;
        var flat = flatten.Flatify();
        var construct = HotCallInfo.FromFlatable(flat);

        // ASSERT
        Assert.Equal(input, construct);
    }
}