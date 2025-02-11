using BRCDAQdemo.WPF.Core.Lib;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BRCDAQdemo.WPF.Core.ViewModels
{

    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ScanButtonText))]
        private bool _scanning = false;

        [ObservableProperty]
        private bool connecting = false;

        public string ScanButtonText => Scanning ? "扫描中..." : "扫描";

        [ObservableProperty]
        public ObservableCollection<BRCSDK.ModuleInfo> _moduleInfos = new ObservableCollection<BRCSDK.ModuleInfo>();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectButtonEnabled))]
        private BRCSDK.ModuleInfo? selectedModule = null;

        public bool ConnectButtonEnabled => SelectedModule != null && !Connecting;




        [RelayCommand]
        private async Task StartScanAsync()
        {
            Scanning = true;
            await Task.Run(() =>
            {
                ModuleInfos = new ObservableCollection<BRCSDK.ModuleInfo>(BRCSDK.ScanModules());
            });

            Scanning = false;
        }

        [RelayCommand]
        private async Task Connect()
        {
            WeakReferenceMessenger.Default.Send(SelectedModule!);
            await StartScanAsync();
        }


    }


}