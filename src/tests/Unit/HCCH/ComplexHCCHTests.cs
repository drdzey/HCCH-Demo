using System;
using System.Threading;
using System.Threading.Tasks;
using Calamara.Ng.Common.Console;
using Lili.Protocol.Core;
using Lili.Protocol.General;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Lili.Protocol.Tests.UnitTests;

public sealed class ComplexHCCHTests
{
    public ComplexHCCHTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private const string SectionBasic = "basic";
    private readonly ITestOutputHelper _output;

    /// <summary>
    /// Verifies dynamic harvesting and registration of HotCall methods("HotCalls") across all major system components.
    ///
    /// This test simulates a full-stack scenario where:
    /// - LCore, LDom, and LSHELL are bootstrapped as independent modules (component boxes)
    /// - The HotCallHarvester collects all dynamic HotCalls from the current assembly (via reflection/DI)
    /// - Section switching is performed (navigating between 'root' and 'basic' sections)
    /// - After each navigation, consistency of registered HotCalls and section paths is asserted
    ///
    /// Output includes detailed debug logs for tracking the HotCall registration lifecycle and navigation flow.
    ///
    /// STDOUT:
    /// <code>[INF] Lili.Shell::HotCallHarvester.HarvestDynamicsAsync(): HotCall ComplexKey (KEY:root(1)) registered.
    /// [INF] Lili.Shell::HotCallHarvester.HarvestDynamicsAsync(): HotCall ComplexKey (KEY:basic(1)) registered.
    /// [TRC] Lili.Core::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Core::HotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Dom::HotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Shell::HotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallNavigationStack.InitAsync(8c75, Lili.Shell): Required ENTER for (KEY:root)
    /// [TRC] Lili.Core::HotCallDynRegistry._GetRegistry(): Provided (HotCallRegistry) with IsRemote (False)
    /// [TRC] Lili.Core::HotCallRegistry.RegisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.EnterSectionAsync(KEY:root, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:root) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Core::HotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallNavigationStack.InitAsync(8c75, Lili.Shell): Required ENTER for (KEY:root)
    /// [TRC] Lili.Dom::HotCallDynRegistry._GetRegistry(): Provided (HotCallRegistry) with IsRemote (True)
    /// [TRC] Lili.Dom::HotCallRegistry.RegisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.EnterSectionAsync(KEY:root, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:root) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Dom::HotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallNavigationStack.InitAsync(8c75, Lili.Shell): Required ENTER for (KEY:root)
    /// [TRC] Lili.Shell::HotCallDynRegistry._GetRegistry(): Provided (HotCallRegistry) with IsRemote (True)
    /// [TRC] Lili.Shell::HotCallRegistry.RegisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.EnterSectionAsync(KEY:root, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:root) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Shell::HotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// ----------------------
    /// [TRC] Lili.Core::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Core::HotCallRegistry.UnregisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): Required ENTER for (KEY:basic)
    /// [TRC] Lili.Core::HotCallRegistry.RegisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:basic) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Core::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): DONE in 1 ms.
    /// [TRC] Lili.Core::HotCallNavigationService.GoAsync(KEY:basic, 8c75, Lili.Shell): DONE. Current Path (-&gt;basic)
    /// [TRC] Lili.Core::HotCallNavigationGate.WaitForIdleAsync(-&gt;basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Core::HotCallRegistry.UnregisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallRegistry.UnregisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): Required ENTER for (KEY:basic)
    /// [TRC] Lili.Core::HotCallRegistry.RegisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:basic) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Dom::HotCallRegistry.RegisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:basic) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): DONE in 3 ms.
    /// [TRC] Lili.Dom::HotCallNavigationService.GoAsync(KEY:basic, 8c75, Lili.Shell): DONE. Current Path (-&gt;basic)
    /// [TRC] Lili.Dom::HotCallNavigationGate.WaitForIdleAsync(-&gt;basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallNavigationGate.WaitForIdleAsync(-&gt;basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Core::HotCallRegistry.UnregisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallRegistry.UnregisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallRegistry.UnregisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): Required ENTER for (KEY:basic)
    /// [TRC] Lili.Core::HotCallRegistry.RegisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:basic) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Dom::HotCallRegistry.RegisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:basic) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallRegistry.RegisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:basic) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.EnterSectionAsync(KEY:basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallNavigationStack.GoAsync(KEY:basic, 8c75, Lili.Shell): DONE in 0 ms.
    /// [TRC] Lili.Shell::HotCallNavigationService.GoAsync(KEY:basic, 8c75, Lili.Shell): DONE. Current Path (-&gt;basic)
    /// [TRC] Lili.Shell::HotCallNavigationGate.WaitForIdleAsync(-&gt;basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallNavigationGate.WaitForIdleAsync(-&gt;basic, 8c75, Lili.Shell): DONE.
    /// ----------------------
    /// [INF] Lili.Shell::HotCallHarvester.HarvestDynamicsAsync(): HotCall ComplexKey (KEY:root(1)) registered.
    /// [INF] Lili.Shell::HotCallHarvester.HarvestDynamicsAsync(): HotCall ComplexKey (KEY:basic(1)) registered.
    /// [TRC] Lili.Core::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:basic)
    /// [TRC] Lili.Core::HotCallRegistry.UnregisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.DropSectionAsync(KEY:basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Core::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:basic)
    /// [TRC] Lili.Dom::HotCallRegistry.UnregisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.DropSectionAsync(KEY:basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Dom::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:basic)
    /// [TRC] Lili.Shell::HotCallRegistry.UnregisterDynAsync(8c75, general_peekaboo): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.DropSectionAsync(KEY:basic, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallNavigationStack.ClearAsync(8c75, Lili.Shell): Required DROP for (KEY:root)
    /// [TRC] Lili.Shell::HotCallDynRegistry.DropSectionAsync(KEY:root, 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.RegisterStartAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.RegisterAsync(KEY:root(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.RegisterAsync(KEY:basic(1), 8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Core::HotCallNavigationStack.InitAsync(8c75, Lili.Shell): Required ENTER for (KEY:root)
    /// [TRC] Lili.Core::HotCallRegistry.RegisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Core::HotCallDynRegistry.EnterSectionAsync(KEY:root, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:root) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Core::HotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::HotCallNavigationStack.InitAsync(8c75, Lili.Shell): Required ENTER for (KEY:root)
    /// [TRC] Lili.Dom::HotCallRegistry.RegisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Dom::HotCallDynRegistry.EnterSectionAsync(KEY:root, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:root) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Dom::HotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Dom::RemoteHotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::HotCallNavigationStack.InitAsync(8c75, Lili.Shell): Required ENTER for (KEY:root)
    /// [TRC] Lili.Shell::HotCallRegistry.RegisterDynAsync(8c75, general_helloWorld): DONE.
    /// [TRC] Lili.Shell::HotCallDynRegistry.EnterSectionAsync(KEY:root, 8c75, Lili.Shell): ComplexKey HotCall registered for section (KEY:root) and user (8c754db1-3e62-498c-a664-3b38aaa799f9).
    /// [TRC] Lili.Shell::HotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// [TRC] Lili.Shell::RemoteHotCallDynRegistry.RegisterEndAsync(8c75, Lili.Shell): DONE.
    /// Shell SectionPath: -&gt;
    /// </code>
    /// </summary>
    [Fact]
    public async Task CHHC_DynamicsHarvesting_RegistersEverywhere()
    {
        // ARRANGE
        var userId = Guid.Parse("8c754db1-3e62-498c-a664-3b38aaa799f9");
        using var lcore = _PrepareLCore();
        var lcoreToLdom = _PrepareLCoreToLDom(lcore); // middleware between
        using var ldom = _PrepareLDom(lcoreToLdom.ClientSide);
        var ldomToLshell = _PrepareLDomToLShell(ldom); // middleware between
        using var lshell = _PrepareLShell(ldomToLshell.ClientSide, userId);

        var serviceProvider = lshell.ServiceProvider;
        serviceProvider.GetService(typeof(IRootHandler)).Returns(_ => new Handlers());
        serviceProvider.GetService(typeof(IBasicHandler)).Returns(_ => new Handlers());
        var harvester = new HotCallHarvester(
            serviceProvider,
            lshell.AppInfo,
            lshell.SttRegistry,
            lshell.DynRegistry,
            _output.AsLogger(),
            lshell.AppInfo.ApplicationName);

        var navigationGate = lshell.NavigationGate;

        // ACT
        await harvester.HarvestDynamicsAsync(GetType().Assembly, CancellationToken.None);
        _output.WriteLine("----------------------");
        navigationGate.Go(HotCallSimpleKey.FromKey(SectionBasic));
        await navigationGate.WaitForIdleAsync(cancellationToken: CancellationToken.None);
        _output.WriteLine("----------------------");
        await harvester.HarvestDynamicsAsync(GetType().Assembly, CancellationToken.None);

        // ASSERT
        var shellNav = lshell.ServiceProvider.GetRequiredService<IHotCallNavigationStack>();
        var domNav = ldom.ServiceProvider.GetRequiredService<IHotCallNavigationStack>();
        var coreNav = lcore.ServiceProvider.GetRequiredService<IHotCallNavigationStack>();

        _output.WriteLine($"Shell SectionPath: {shellNav.GetSectionPath()}");
        Assert.Equal(shellNav.GetSectionPath(), domNav.GetSectionPath(userId, ldom.AppInfo.ApplicationName));
        Assert.Equal(shellNav.GetSectionPath(), coreNav.GetSectionPath(userId, lcore.AppInfo.ApplicationName));

        var callLShell = lshell.SttRegistry.GetSummary(userId).Values;
        var callLDom = ldom.SttRegistry.GetSummary(userId).Values;
        var callLCore = lcore.SttRegistry.GetSummary(userId).Values;
        Assert.Single(callLShell);
        Assert.Single(callLDom);
        Assert.Single(callLCore);
    }

    /// <summary>
    /// Dummy implementation of both IRootHandler and IBasicHandler,
    /// used for testing HotCall registration and dynamic invocation.
    /// </summary>
    private sealed class Handlers : IRootHandler, IBasicHandler
    {
        public Task<HotCallResult> HelloWorld()
        {
            return Task.FromResult(new
            {
                Say = "Hello, World!"
            }.AsHotCallResult());
        }

        public Task<HotCallResult> Peekaboo()
        {
            return Task.FromResult(new
            {
                Peekaboo = "yes"
            }.AsHotCallResult());
        }
    }

    /// <summary>
    /// Defines dynamic HotCall endpoints for the "root" section.
    /// Used for dynamic registration during tests.
    /// </summary>
    [HotCallingDyn]
    private interface IRootHandler
    {
        [HotCall(
            "HelloWorld",
            "A method that says hello world.",
            Category = "General",
            Examples = [ "This is it."])]
        Task<HotCallResult> HelloWorld();
    }

    /// <summary>
    /// Defines dynamic HotCall endpoints for the "basic" section.
    /// Used for dynamic registration during tests.
    /// </summary>
    [HotCallingDyn(SectionBasic)]
    private interface IBasicHandler
    {
        [HotCall(
            "Peekaboo",
            "A method that says peekaboo to the given name.",
            Category = "General",
            Examples = [ "This is it." ])]
        Task<HotCallResult> Peekaboo();
    }

    /// <summary>
    /// Prepares a simulated bridge (middleware connection) between two components.
    /// Used to link modules for HotCall propagation in integration tests.
    /// </summary>
    private static ComponentBridge _PrepareLDomToLShell(ComponentBox ldom)
    {
        return _PrepareBridge(ldom, ComponentName.LDom, ComponentName.LShell);
    }

    /// <summary>
    /// ditto.
    /// </summary>
    private static ComponentBridge _PrepareLCoreToLDom(ComponentBox lcore)
    {
        return _PrepareBridge(lcore, ComponentName.LCore, ComponentName.LDom);
    }

    /// <summary>
    /// ditto.
    /// </summary>
    private static ComponentBridge _PrepareBridge(ComponentBox box, ComponentName from, ComponentName to)
    {
        var connection = ComponentConnection.Between(from, to);
        var bridge = new ComponentBridge(connection, box);
        return bridge;
    }

    /// <summary>
    /// Sets up the LSHELL ("Lili Shell") test component,
    /// including all required services, navigation and HotCall registries.
    /// Optionally binds to a specific userId, so user-scoped navigation and HotCalls are simulated correctly.
    /// </summary>
    /// <param name="hubFactory">The hub factory instance (SignalR mock or test double).</param>
    /// <param name="userId">The test user GUID (optional, use when simulating user context).</param>
    /// <returns>ComponentBox representing the fully-initialized LSHELL instance.</returns>
    private ComponentBox _PrepareLShell(IHubFactory hubFactory, Guid userId)
    {
        return _PrepareUpperComponents(hubFactory, ComponentName.LShell, userId);
    }

    /// <summary>
    /// Sets up the LDOM ("Lili Daemonette") test component,
    /// including all required service providers and navigation/HotCall registries.
    /// </summary>
    /// <param name="hubFactory">The hub factory instance (SignalR mock or test double).</param>
    /// <returns>ComponentBox representing the fully-initialized LDOM instance.</returns>
    private ComponentBox _PrepareLDom(IHubFactory hubFactory)
    {
        return _PrepareUpperComponents(hubFactory, ComponentName.LDom);
    }

    /// <summary>
    /// Helper for instantiating a test component (LSHELL or LDOM) with all service dependencies,
    /// navigation stack, HotCall dynamic registries and simulated user context.
    /// This is the "core factory" for test component setup.
    /// </summary>
    private ComponentBox _PrepareUpperComponents(IHubFactory hubFactory, ComponentName name, Guid? userId = null)
    {
        var contextProvider = new HotCallContextProvider();
        IProvideUserId userIdProvider = null;
        if (userId.HasValue)
        {
            userIdProvider = new InternalUserIdProvider(userId.Value);
        }
        else
        {
            userIdProvider = new ErrorUserIdProvider();
        }

        var appInfo = Substitute.For<IAppInfoProvider>();
        appInfo.ApplicationName.Returns(_ => name.To());

        // local registry
        var sttRegistry = new HotCallRegistry(contextProvider, _output.AsLogger(), appInfo.ApplicationName);
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(HotCallRegistry)).Returns(_ => sttRegistry);

        var navigationStack = new HotCallNavigationStack(userIdProvider, appInfo, _output.AsLogger(), appInfo.ApplicationName);
        serviceProvider.GetService(typeof(IHotCallNavigationStack)).Returns(_ => navigationStack);

        var dynRegistry = new HotCallDynRegistry(
            serviceProvider,
            userIdProvider,
            appInfo,
            navigationStack,
            _output.AsLogger(),
            appInfo.ApplicationName);

        // dynamic registry
        var sttRegistryRemote = new RemoteHotCallRegistry(sttRegistry, hubFactory, _output.AsLogger(), appInfo.ApplicationName);
        var dynRegistryRemote = new RemoteHotCallDynRegistry(dynRegistry, hubFactory, userIdProvider, appInfo, _output.AsLogger(), appInfo.ApplicationName);

        serviceProvider.GetService(typeof(HotCallRegistry)).Returns(_ => sttRegistry);
        serviceProvider.GetService(typeof(IHotCallRegistry)).Returns(_ => sttRegistryRemote);
        serviceProvider.GetService(typeof(HotCallDynRegistry)).Returns(_ => dynRegistry);
        serviceProvider.GetService(typeof(IHotCallDynRegistry)).Returns(_ => dynRegistryRemote);

        var navigationService = new HotCallNavigationService(serviceProvider, navigationStack, _output.AsLogger(), appInfo.ApplicationName);
        var navigationGate = new HotCallNavigationGate(navigationService, userIdProvider, appInfo, _output.AsLogger(), appInfo.ApplicationName);
        var navigationGateRemote = new RemoteHotCallNavigationGate(hubFactory, userIdProvider, appInfo, navigationGate, _output.AsLogger(), appInfo.ApplicationName);

        var box = new ComponentBox(
            name,
            serviceProvider,
            sttRegistryRemote,
            dynRegistryRemote,
            navigationService,
            navigationGateRemote,
            appInfo);

        return box;
    }

    /// <summary>
    /// Prepares the LCORE ("Lili Core") test component, which is the lowest layer
    /// in the test stack. Sets up registry, navigation, and context services for test HotCalls.
    /// </summary>
    private ComponentBox _PrepareLCore()
    {
        const ComponentName name = ComponentName.LCore;

        var appInfo = Substitute.For<IAppInfoProvider>();
        appInfo.ApplicationName.Returns(_ => name.To());
        var contextProvider = new HotCallContextProvider();
        var sttRegistry = new HotCallRegistry(contextProvider, _output.AsLogger(), appInfo.ApplicationName);
        var userIdProvider = new ErrorUserIdProvider();

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(HotCallRegistry)).Returns(_ => sttRegistry);
        serviceProvider.GetService(typeof(IHotCallRegistry)).Returns(_ => sttRegistry);

        var navigationStack = new HotCallNavigationStack(userIdProvider, appInfo, _output.AsLogger(), appInfo.ApplicationName);
        serviceProvider.GetService(typeof(IHotCallNavigationStack)).Returns(_ => navigationStack);

        var dynRegistry = new HotCallDynRegistry(
            serviceProvider,
            userIdProvider,
            appInfo,
            navigationStack,
            _output.AsLogger(),
            appInfo.ApplicationName);

        serviceProvider.GetService(typeof(HotCallDynRegistry)).Returns(_ => dynRegistry);
        serviceProvider.GetService(typeof(IHotCallDynRegistry)).Returns(_ => dynRegistry);

        var navigationService = new HotCallNavigationService(serviceProvider, navigationStack, _output.AsLogger(), appInfo.ApplicationName);
        var navigationGate = new HotCallNavigationGate(navigationService, userIdProvider, appInfo, _output.AsLogger(), appInfo.ApplicationName);

        var box = new ComponentBox(
            name,
            serviceProvider,
            sttRegistry,
            dynRegistry,
            navigationService,
            navigationGate,
            appInfo);

        return box;
    }
}