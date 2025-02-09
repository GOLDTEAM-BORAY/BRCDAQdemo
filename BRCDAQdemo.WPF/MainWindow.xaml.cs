using BRCDAQdemo.WPF.Core.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Windows;
using static BRCDAQdemo.WPF.Core.Lib.BRCSDK;

namespace BRCDAQdemo.WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();

            WeakReferenceMessenger.Default.Register<ModuleInfo>(this, (r, moduleInfo) =>
            {
                try
                {
                    var brcDevice = Connect(moduleInfo);
                    var scopeWindow = new ScopeWindow();
                    var scopeWindowViewModel = new ScopeWindowViewModel()
                    {
                        Renderer = scopeWindow,
                        Device = brcDevice
                    };
                    scopeWindow.DataContext = scopeWindowViewModel;
                    scopeWindow.Closed += (s, e) => scopeWindowViewModel.Dispose();
                    scopeWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "连接失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

        }

    }
}
