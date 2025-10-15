using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Lili.Protocol.General;
using NSubstitute;
using Xunit;

namespace Lili.Protocol.Tests.UnitTests;

public sealed class HotCallNavigationGateTests
{
    [Fact]
    public async Task WaitForIdleAsync_FlushesQueuedCommandsInOrder()
    {
        // ARRANGE
        var userId = Guid.Parse("a02b1f32-3cb8-4d88-b020-0482431d88c5");
        var userIdProvider = Substitute.For<IProvideUserId>();
        userIdProvider.GetUserId().Returns(_ => userId);
        var appInfo = Substitute.For<IAppInfoProvider>();
        appInfo.ApplicationName.Returns(_ => "Lili.Shell");
        var navigationService = Substitute.For<IHotCallNavigationService>();
        var loggerFactory = Substitute.For<ISharedLoggerFactory>();
        var gate = new HotCallNavigationGate(navigationService, userIdProvider, appInfo, loggerFactory);

        var callOrder = new List<string>();

        navigationService
            .GoAsync(Arg.Any<HotCallSimpleKey>(), Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                callOrder.Add($"Go:{ci.Arg<HotCallSimpleKey>().Key}");
                return Task.CompletedTask;
            });

        navigationService
            .GoBackAsync(Arg.Any<Guid?>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callOrder.Add("Back");
                return Task.CompletedTask;
            });

        var registered = false;
        gate.GateIdle += (_, _, _) =>
        {
            registered = true;
            return Task.CompletedTask;
        };

        // ACT
        gate.Go(HotCallSimpleKey.FromKey("a"));
        gate.Go(HotCallSimpleKey.FromKey("b"));
        gate.GoBack();
        gate.Go(HotCallSimpleKey.FromKey("c"));
        await gate.WaitForIdleAsync();

        // ASSERT
        Assert.Equal(4, callOrder.Count);
        Assert.Equal("Go:a", callOrder[0]);
        Assert.Equal("Go:b", callOrder[1]);
        Assert.Equal("Back", callOrder[2]);
        Assert.Equal("Go:c", callOrder[3]);
        Assert.True(registered);
    }
}