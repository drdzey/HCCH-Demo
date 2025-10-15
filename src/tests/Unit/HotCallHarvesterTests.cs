using System;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Lili.Protocol.General;
using NSubstitute;
using Xunit;

namespace Lili.Protocol.Tests.UnitTests;

public class HotCallHarvesterTests
{
    [Fact]
    public async Task HarvestDynamicsAsync_RegistersComplexKey_NSubstitute()
    {
        // ARRANGE
        var serviceProvider = Substitute.For<IServiceProvider>();
        var appInfo = Substitute.For<IAppInfoProvider>();
        var registry = Substitute.For<IHotCallRegistry>();
        var dynRegistry = Substitute.For<IHotCallDynRegistry>();
        var loggerFactory = Substitute.For<ISharedLoggerFactory>();
        var logger = Substitute.For<ISharedLogger>();
        loggerFactory.GetLogger(Arg.Any<Type>()).Returns(logger);

        serviceProvider.GetService(typeof(ITestDynHotCalls)).Returns(new TestDynHotCallsImpl());

        var harvester = new HotCallHarvester(
            serviceProvider,
            appInfo,
            registry,
            dynRegistry,
            loggerFactory);

        // ACT
        await harvester.HarvestDynamicsAsync(typeof(ITestDynHotCalls).Assembly);

        // ASSERT
        await dynRegistry.Received().RegisterAsync(Arg.Is<HotCallComplexKey>(key =>
            key.Key == "test-key" &&
            key.Handlers != null &&
            key.Handlers.Count > 0));
    }

    [HotCallingDyn("test-key")]
    private interface ITestDynHotCalls
    {
        [HotCall("DoStuff", "Test call")]
        HotCallResult DoStuff();
    }

    private class TestDynHotCallsImpl : ITestDynHotCalls
    {
        public HotCallResult DoStuff() => new HotCallResult();
    }
}