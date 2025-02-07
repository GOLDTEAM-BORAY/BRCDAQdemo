using BRCDAQdemo.WPF.Core.Lib;
using BRCDAQdemo.WPF.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BRCDAQdemo.WPF.Core.ViewModels
{

    public partial class MainWindowViewModel : ObservableObject
    {
        //private Scanner _scanner;
        //public MainWindowViewModel(Scanner scanner)
        //{
        //    _scanner = scanner;
        //    _scanner.OnModuleChanged += ScannerOnModuleChanged;
        //}

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ScanButtonText))]
        private bool _scanning = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectButtonText), nameof(ConnectButtonEnabled))]
        private bool connecting = false;

        public string ScanButtonText => Scanning ? "扫描中..." : "扫描";
        public string ConnectButtonText => Connecting ? "连接中..." : "连接";

        //public List<ModuleInfo> Modules => [.. _scanner.Modules.Values];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectButtonEnabled))]
        private ModuleInfo selectedModule = null;

        public bool ConnectButtonEnabled => SelectedModule != null && !Connecting;


        //[RelayCommand]
        //private static void OpenLogViewer()
        //{
        //    if (!App.Current.Windows.OfType<LogViewer>().Any())
        //    {
        //        using var scope = App.Current.Services.CreateScope();
        //        var logViewer = scope.ServiceProvider.GetRequiredService<LogViewer>();
        //        logViewer.Show();
        //    }
        //}

        //[RelayCommand]
        //private static void OpenAbout()
        //{
        //    using var scope = App.Current.Services.CreateScope();
        //    var aboutWindow = scope.ServiceProvider.GetRequiredService<AboutWindow>();
        //    aboutWindow.Show();
        //}

        //[RelayCommand]
        //private static void ExportSDK()
        //{
        //    Process.Start(new ProcessStartInfo
        //    {
        //        FileName = "http://oss.boray-sz.com/files/brc/vscope_v2/sdk.zip",
        //        UseShellExecute = true
        //    });
        //}


        [RelayCommand]
        private async Task StartScanAsync()
        {
            Scanning = true;
            var moduleCount = BRCSDK.ScanModules();
            var productName = BRCSDK.GetModuleInfoProductName(0);
            var deviceId = BRCSDK.GetModuleInfoDeviceId(0);
            var channelCount = BRCSDK.GetModuleInfoChannelCount(0);
            var sampleRateOptions = BRCSDK.GetModuleInfoSampleRateOptions(0);
            var currentOptions = BRCSDK.GetModuleInfoSampleCurrentOptions(0);
            var couplingOptions = BRCSDK.GetModuleInfoSampleCouplingOptions(0);
            Scanning = false;
        }

        //[RelayCommand]
        //private async Task ConnectAsync()
        //{
        //    Connecting = true;
        //    try
        //    {
        //        var scope = App.Current.Services.CreateScope();
        //        var scopeWindow = scope.ServiceProvider.GetRequiredService<ScopeWindow>();
        //        scopeWindow.Closed += (_, _) => scope.Dispose();
        //        if (await scopeWindow.Connect(SelectedModule!))
        //            scopeWindow.Show();
        //        else
        //            scopeWindow.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, "连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //    finally
        //    {
        //        Connecting = false;
        //    }
        //    await StartScanAsync();
        //}

        //[RelayCommand]
        //private static async Task OpenConfig(ModuleInfo moduleInfo)
        //{
        //    if (moduleInfo.Interface != InterfaceType.Usb)
        //        throw new InvalidOperationException("请使用USB接口进行设置");
        //    using var scope = App.Current.Services.CreateScope();
        //    var scopeWindowViewModel = scope.ServiceProvider.GetRequiredService<UsbDeviceConfigWindowViewModel>();
        //    scopeWindowViewModel.Scope = scope;

        //    var moduleClientFactories = scope.ServiceProvider.GetRequiredService<IEnumerable<IModuleClientFactory>>();
        //    var moduleClientFactory = moduleClientFactories.FirstOrDefault(x => x.IsAvailable(moduleInfo)) ?? throw new NotImplementedException("不支持的设备类型");
        //    var moduleClient = await moduleClientFactory.ConnectAsync(moduleInfo);
        //    await scopeWindowViewModel.ShowDialogAsync((moduleClient as UsbModuleClient)!);
        //}


        //private Task ScannerOnModuleChanged()
        //{
        //    OnPropertyChanged(nameof(Modules));
        //    return Task.CompletedTask;
        //    //return App.Current.Dispatcher.Invoke(() =>
        //    //{
        //    //    Modules = new(_scanner.Modules.Values.OrderByDescending(x => x.DeviceId));
        //    //    return Task.CompletedTask;
        //    //});
        //}

        //[RelayCommand]
        //private static async Task TrigStartViaUdpAsync()
        //{
        //    var scopeWindowViewModels = App.Current.Windows.OfType<ScopeWindow>()
        //        .Select(window => ((ScopeWindowViewModel)window.DataContext))
        //        .Where(scopeWindowViewModel => scopeWindowViewModel.ModuleClient.ModuleInfo.Interface == InterfaceType.Net)
        //        .ToList();

        //    if (scopeWindowViewModels.Count > 0)
        //    {
        //        foreach (var scopeWindowViewModel in scopeWindowViewModels)
        //        {
        //            await scopeWindowViewModel.ConfigureStartUdpAsync();
        //        }
        //        using var scope = App.Current.Services.CreateScope();
        //        var netModuleClientFactory = (NetModuleClientFactory)scope.ServiceProvider.GetRequiredService<IEnumerable<IModuleClientFactory>>().First(x => x is NetModuleClientFactory);
        //        await netModuleClientFactory.StartByUdp();
        //    }


        //}


    }


}