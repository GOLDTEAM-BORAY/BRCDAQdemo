using Avalonia.Controls;
using BRCDAQdemo.Core.Lib;
using BRCDAQdemo.Core.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using System;

namespace BRCDAQdemo.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        WeakReferenceMessenger.Default.Register<BRCSDK.ModuleInfo>(this, async (recipient, moduleInfo) =>
        {
            try
            {
                var device = BRCSDK.Connect(moduleInfo);
                var scopeWindow = new ScopeWindow(device);
                scopeWindow.Show();
            }
            catch (Exception ex)
            {
                await ErrorDialog.ShowAsync(this, "连接失败", ex.Message);
            }
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        base.OnClosed(e);
    }
}
