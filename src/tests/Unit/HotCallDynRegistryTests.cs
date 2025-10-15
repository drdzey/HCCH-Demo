using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Lili.Protocol.General;
using NSubstitute;
using Xunit;

namespace Lili.Protocol.Tests.UnitTests;

public class HotCallDynRegistryTests
{
    [Fact]
    public async Task EnterSectionAsync_RegistersAllHandlersForUser()
    {
        // ARRANGE
        const string key = "section-test";
        var registry = Substitute.For<IHotCallRegistry>();
        var userIdProvider = Substitute.For<IProvideUserId>();
        var appInfoProvider = Substitute.For<IAppInfoProvider>();
        var loggerFactory = Substitute.For<ISharedLoggerFactory>();
        var logger = Substitute.For<ISharedLogger>();
        loggerFactory.GetLogger(Arg.Any<Type>()).Returns(logger);

        var testUserId = Guid.Parse("8d27fbd2-3980-4d43-84cf-c877bce04c2a");
        userIdProvider.GetUserId().Returns(testUserId);

        const string applicationName = "Lili.Shell";
        appInfoProvider.ApplicationName.Returns(applicationName);

        var handler1 = new HotCallInfo("doStuff", "Do it now", "general", [], []).AsHandler();
        var handler2 = new HotCallInfo("doStuff2", "Do it now", "general", [], []).AsHandler();
        var handlers = new List<HotCallHandler> { handler1, handler2 };

        var simpleKey = HotCallSimpleKey.FromKey(key);
        var complexKey = new HotCallComplexKey(key, handlers);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IHotCallRegistry)).Returns(_ => registry);

        var navigationStack = new HotCallNavigationStack(userIdProvider, appInfoProvider, loggerFactory);
        var dynRegistry = new HotCallDynRegistry(serviceProvider, userIdProvider, appInfoProvider, navigationStack, loggerFactory);

        // ACT
        await dynRegistry.RegisterStartAsync();
        await dynRegistry.RegisterAsync(complexKey);
        await dynRegistry.RegisterEndAsync();
        await dynRegistry.EnterSectionAsync(simpleKey);

        // ASSERT
        await registry.Received().RegisterDynAsync(testUserId, handler1, Arg.Any<CancellationToken>());
        await registry.Received().RegisterDynAsync(testUserId, handler2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DropSectionAsync_UnregistersAllHandlersForUser()
    {
        // ARRANGE
        const string key = "section-test";
        var registry = Substitute.For<IHotCallRegistry>();
        var userIdProvider = Substitute.For<IProvideUserId>();
        var appInfoProvider = Substitute.For<IAppInfoProvider>();
        var loggerFactory = Substitute.For<ISharedLoggerFactory>();
        var logger = Substitute.For<ISharedLogger>();
        loggerFactory.GetLogger(Arg.Any<Type>()).Returns(logger);

        var testUserId = Guid.NewGuid();
        userIdProvider.GetUserId().Returns(testUserId);

        const string applicationName = "Lili.Shell";
        appInfoProvider.ApplicationName.Returns(applicationName);

        var handler1 = new HotCallInfo("doStuff", "Do it now", "general", [], []).AsHandler();
        var handler2 = new HotCallInfo("doStuff2", "Do it now", "general", [], []).AsHandler();
        var handlers = new List<HotCallHandler> { handler1, handler2 };

        var simpleKey = HotCallSimpleKey.FromKey(key);
        var complexKey = new HotCallComplexKey(key, handlers);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IHotCallRegistry)).Returns(_ => registry);

        var navigationStack = new HotCallNavigationStack(userIdProvider, appInfoProvider, loggerFactory);

        var dynRegistry = new HotCallDynRegistry(serviceProvider, userIdProvider, appInfoProvider, navigationStack, loggerFactory);

        // ACT
        await dynRegistry.RegisterStartAsync();
        await dynRegistry.RegisterAsync(complexKey);
        await dynRegistry.EnterSectionAsync(simpleKey);
        await dynRegistry.DropSectionAsync(simpleKey);

        // ASSERT
        await registry.Received().UnregisterDynAsync(testUserId, handler1, Arg.Any<CancellationToken>());
        await registry.Received().UnregisterDynAsync(testUserId, handler2, Arg.Any<CancellationToken>());
    }
}
