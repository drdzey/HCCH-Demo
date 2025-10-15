using System;
using Calamara.Ng.Common.Console;
using Lili.Protocol.General;
using PublicDisposableUnit = Calamara.Core.PublicDisposableUnit;

// ReSharper disable ConvertToPrimaryConstructor

namespace Lili.Protocol.Tests.UnitTests;

internal sealed class ComponentBox : PublicDisposableUnit
{
    public ComponentBox(
        ComponentName key,
        IServiceProvider serviceProvider,
        IHotCallRegistry sttRegistry,
        IHotCallDynRegistry dynRegistry,
        IHotCallNavigationService navigationService,
        IHotCallNavigationGate navigationGate,
        IAppInfoProvider appInfo)
    {
        Key = key;
        ServiceProvider = serviceProvider;
        SttRegistry = sttRegistry;
        DynRegistry = dynRegistry;
        NavigationService = navigationService;
        NavigationGate = navigationGate;
        AppInfo = appInfo;
    }

    public ComponentName Key { get; }

    public IServiceProvider ServiceProvider { get; }

    public string Name => Key.To();

    public IHotCallRegistry SttRegistry { get; }

    public IHotCallDynRegistry DynRegistry { get; }

    public IHotCallNavigationService NavigationService { get; }

    public IHotCallNavigationGate NavigationGate { get; }

    public IAppInfoProvider AppInfo { get; }

    protected override void DoDispose()
    {
        NavigationService?.Dispose();
    }
}