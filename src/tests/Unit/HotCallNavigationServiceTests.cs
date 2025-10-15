using System;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Lili.Protocol.General;
using NSubstitute;
using Xunit;

namespace Lili.Protocol.Tests.UnitTests;

public sealed class HotCallNavigationServiceTests
{
    [Fact]
    public async Task GoAsync_EntersSection_AndDropsPrevious()
    {
        // ARRANGE
        var userId = Guid.Parse("a02b1f32-3cb8-4d88-b020-0482431d88c5");
        var userIdProvider = Substitute.For<IProvideUserId>();
        userIdProvider.GetUserId().Returns(_ => userId);
        var appInfo = Substitute.For<IAppInfoProvider>();
        appInfo.ApplicationName.Returns(_ => "Lili.Shell");

        var loggerFactory = Substitute.For<ISharedLoggerFactory>();
        var dynRegistry = Substitute.For<IHotCallDynRegistry>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IHotCallDynRegistry)).Returns(_ => dynRegistry);
        var navigationStack = new HotCallNavigationStack(userIdProvider, appInfo, loggerFactory);
        using var nav = new HotCallNavigationService(serviceProvider, navigationStack, loggerFactory);

        var section1 = HotCallSimpleKey.FromKey("section1");
        var section2 = HotCallSimpleKey.FromKey("section2");

        // ACT + ASSERT
        await nav.GoAsync(section1);
        await dynRegistry.Received().DropSectionAsync(Arg.Is<HotCallSimpleKey>(k => k.Key == HotCallSimpleKey.RootKey), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await dynRegistry.Received().EnterSectionAsync(Arg.Is<HotCallSimpleKey>(k => k.Key == "section1"), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        await nav.GoAsync(section2);
        await dynRegistry.Received().DropSectionAsync(Arg.Is<HotCallSimpleKey>(k => k.Key == "section1"), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await dynRegistry.Received().EnterSectionAsync(Arg.Is<HotCallSimpleKey>(k => k.Key == "section2"), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GoBackAsync_DropsCurrent_EntersPrevious_OrRoot()
    {
        // ARRANGE
        var userId = Guid.Parse("a02b1f32-3cb8-4d88-b020-0482431d88c5");
        var userIdProvider = Substitute.For<IProvideUserId>();
        userIdProvider.GetUserId().Returns(_ => userId);
        var appInfo = Substitute.For<IAppInfoProvider>();
        appInfo.ApplicationName.Returns(_ => "Lili.Shell");

        var loggerFactory = Substitute.For<ISharedLoggerFactory>();
        var dynRegistry = Substitute.For<IHotCallDynRegistry>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IHotCallDynRegistry)).Returns(_ => dynRegistry);
        var navigationStack = new HotCallNavigationStack(userIdProvider, appInfo, loggerFactory);
        using var nav = new HotCallNavigationService(serviceProvider, navigationStack, loggerFactory);

        var section1 = HotCallSimpleKey.FromKey("section1");
        var section2 = HotCallSimpleKey.FromKey("section2");

        await nav.GoAsync(section1); // stack: section1
        await nav.GoAsync(section2); // stack: section1, section2

        // ACT + ASSERT
        await nav.GoBackAsync();
        await dynRegistry.Received().DropSectionAsync(Arg.Is<HotCallSimpleKey>(k => k.Key == "section2"), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await dynRegistry.Received().EnterSectionAsync(Arg.Is<HotCallSimpleKey>(k => k.Key == "section1"), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

        await nav.GoBackAsync();
        await dynRegistry.Received().DropSectionAsync(Arg.Is<HotCallSimpleKey>(k => k.Key == "section1"), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await dynRegistry.Received().EnterSectionAsync(Arg.Is<HotCallSimpleKey>(k => k.Key == HotCallSimpleKey.RootKey), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GoBackAsync_OnEmptyStack_OnlyDoesNothing()
    {
        // ARRANGE
        var userId = Guid.Parse("a02b1f32-3cb8-4d88-b020-0482431d88c5");
        var userIdProvider = Substitute.For<IProvideUserId>();
        userIdProvider.GetUserId().Returns(_ => userId);
        var appInfo = Substitute.For<IAppInfoProvider>();
        appInfo.ApplicationName.Returns(_ => "Lili.Shell");

        var loggerFactory = Substitute.For<ISharedLoggerFactory>();
        var dynRegistry = Substitute.For<IHotCallDynRegistry>();
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IHotCallDynRegistry)).Returns(_ => dynRegistry);
        var navigationStack = new HotCallNavigationStack(userIdProvider, appInfo, loggerFactory);
        using var nav = new HotCallNavigationService(serviceProvider, navigationStack, loggerFactory);

        // ACT
        await nav.GoBackAsync();

        // ASSERT
        await dynRegistry.DidNotReceiveWithAnyArgs().DropSectionAsync(default, default);
        await dynRegistry.DidNotReceiveWithAnyArgs().EnterSectionAsync(default, default);
    }
}