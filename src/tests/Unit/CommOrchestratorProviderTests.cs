using System;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Lili.Protocol.General;
using Lili.Protocol.Intercom;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Xunit;
using HotCallInfo = Lili.Protocol.General.HotCallInfo;

namespace Lili.Protocol.Tests.UnitTests;

public sealed class CommOrchestratorProviderTests
{
    [Fact]
    public async Task LoopSendUpstream_ResolvesLocalHotCall_ReturnsResult()
    {
        // ARRANGE
        var registry = Substitute.For<IHotCallRegistry>();
        registry.InvokeAsync("MyHotCall", Arg.Any<Guid?>()).Returns(_ => "yes".AsHotCallResult(success: true));
        var contextProvider = Substitute.For<IHotCallContextProvider>();
        var appInfo = Substitute.For<IAppInfoProvider>();
        var loggerFactory = Substitute.For<ISharedLoggerFactory>();
        var logger = Substitute.For<ISharedLogger>();

        loggerFactory.GetLogger(Arg.Any<Type>()).Returns(logger);
        appInfo.ApplicationName.Returns("TestApp");

        var handler = new HotCallInfo("MyHotCall", "this is it", ActionCategory.General, [], [])
        {
            Owner = "TestApp"
        }.AsHandler();

        registry.FindHandler("MyHotCall", Arg.Any<Guid>()).Returns(handler);

        var orchestrator = new CommOrchestratorProvider(
            registry,
            contextProvider,
            appInfo,
            loggerFactory);

        var userId = Guid.NewGuid();

        Func<CommunicationBlockRequestDto, Task<CommunicationBlockResponseDto>> fakeSendPrompt = req =>
        {
            CommunicationBlockResponseDto res;
            if (req.HotCall == null)
            {
                res = new CommunicationBlockResponseDto
                {
                    HasHotCall = true,
                    HotCall = new CommunicationBlockHotCallDto
                    {
                        Name = "MyHotCall",
                        Arguments = new JObject()
                    }
                };
            }
            else
            {
                res = new CommunicationBlockResponseDto
                {
                    HasStop = true,
                    Response = $"Is it so? {req.HotCall.Result.Value}"
                };
            }

            return Task.FromResult(res);
        };

        var initialRequest = new CommunicationBlockRequestDto();

        // ACT
        var result = await orchestrator.LoopSendUpstream(
            fakeSendPrompt,
            initialRequest,
            userId);

        // ASSERT
        Assert.NotNull(result);
        Assert.Equal("Is it so? yes", result.Response);
    }
}